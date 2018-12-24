using System.Collections.Generic;
using Improbable.Worker.CInterop.Internal;
using System;
using System.Runtime.InteropServices;
using EntityId = System.Int64;
using RequestId = System.UInt32;
using Uint32 = System.UInt32;

namespace Improbable.Worker.CInterop
{
    public enum ConnectionStatusCode
    {
        Success = 1,
        InternalError = 2,
        InvalidArgument = 3,
        NetworkError = 4,
        Timeout = 5,
        Cancelled = 6,
        Rejected = 7,
        PlayerIdentityTokenExpired = 8,
        LoginTokenExpired = 9,
        CapacityExceeded = 10,
        RateExceeded = 11,
        ServerShutdown = 12,
    }

    public enum ComponentUpdateLoopback
    {
        None,
        ShortCircuited,
    }

    public class InterestOverride
    {
        public bool IsInterested;
    }

    public class UpdateParameters
    {
        public ComponentUpdateLoopback Loopback = ComponentUpdateLoopback.ShortCircuited;
    }

    public class CommandParameters
    {
        public bool AllowShortCircuiting = false;
    }

    public sealed unsafe class Connection : IDisposable
    {
        private readonly ConnectionHandle connection;
        private readonly List<WrappedGcHandle> componentVtableHandles;

        public static Future<Connection> ConnectAsync(
            string receptionistHostname,
            ushort receptionistPort,
            string workerId,
            ConnectionParameters connectionParams)
        {
            ConnectionFutureHandle future = null;
            List<WrappedGcHandle> componentVtableHandles = null;
            ParameterConversion.ConvertConnectionParameters(connectionParams, (parameters, handles) =>
            {
                componentVtableHandles = handles;
                fixed (byte* hostnameBytes = ApiInterop.ToUtf8Cstr(receptionistHostname))
                fixed (byte* workerIdBytes = ApiInterop.ToUtf8Cstr(workerId))
                {
                    future = CWorker.ConnectAsync(hostnameBytes, receptionistPort, workerIdBytes, parameters);
                }
            });
            return new Future<Connection>(future,
                ParameterConversion.ConnectionFutureGet(future, componentVtableHandles));
        }

        internal Connection(ConnectionHandle connection, List<WrappedGcHandle> componentVtableHandles)
        {
            this.connection = connection;
            this.componentVtableHandles = componentVtableHandles;
        }

        /// <inheritdoc cref="IDisposable"/>
        public void Dispose()
        {
            connection.Dispose();
            ParameterConversion.FreeVtableHandles(componentVtableHandles);
        }

        [System.Obsolete("Equivalent to Connection.GetConnectionStatusCode() == ConnectionStatusCode.Success.")]
        public bool IsConnected => !connection.IsClosed && CWorker.Connection_IsConnected(connection);

        public ConnectionStatusCode GetConnectionStatusCode()
        {
            return (ConnectionStatusCode) CWorker.Connection_GetConnectionStatusCode(connection);
        }

        public String GetConnectionStatusCodeDetailString()
        {
            return ApiInterop.FromUtf8Cstr(CWorker.Connection_GetConnectionStatusDetailString(connection));
        }

        public OpList GetOpList(uint timeoutMillis)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            return new OpList(CWorker.Connection_GetOpList(connection, timeoutMillis));
        }

        public string GetWorkerId()
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            return ApiInterop.FromUtf8Cstr(CWorker.Connection_GetWorkerId(connection));
        }

        public List<string> GetWorkerAttributes()
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            var workerAttributes = CWorker.Connection_GetWorkerAttributes(connection);

            var attributesList = new List<string>();
            for (int i = 0; i < workerAttributes->AttributeCount; ++i)
            {
                attributesList.Add(ApiInterop.FromUtf8Cstr(workerAttributes->Attributes[i]));
            }

            return attributesList;
        }

        public string GetWorkerFlag(string flagName)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            string flagValue = null;
            // Collection.Option is a value type with an object in it - there is no way to
            // create a pointer to it.
            Action<string> callback = value => { flagValue = value; };

            fixed (byte* nameBytes = ApiInterop.ToUtf8Cstr(flagName))
            {
                using (var callbackHandle = new WrappedGcHandle(callback))
                {
                    CWorker.Connection_GetFlag(
                        connection, nameBytes, (void*) callbackHandle.Get(),
                        CallbackThunkDelegates.GetFlagCallbackDelegate);
                }
            }

            return flagValue;
        }

        public void SendLogMessage(
            LogLevel level,
            string loggerName,
            string message,
            EntityId? entityId = null)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            CWorker.LogMessage logMessage;
            fixed (byte* loggerNameBytes = ApiInterop.ToUtf8Cstr(loggerName))
            fixed (byte* messageBytes = ApiInterop.ToUtf8Cstr(message))
            {
                logMessage.LogLevel = (byte) level;
                logMessage.LoggerName = loggerNameBytes;
                logMessage.Message = messageBytes;
                EntityId entityIdValue = entityId ?? 0;
                logMessage.EntityId = entityId.HasValue ? &entityIdValue : null;
                CWorker.Connection_SendLogMessage(connection, &logMessage);
            }
        }

        public void SendMetrics(Metrics metrics)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            var gaugeCount = metrics.GaugeMetrics.Count;
            var histogramCount = metrics.HistogramMetrics.Count;

            CWorker.Metrics protocolMetrics;
            double loadValue = metrics.Load ?? 0;
            protocolMetrics.Load = metrics.Load.HasValue ? &loadValue : null;
            protocolMetrics.GaugeMetricCount = (uint) gaugeCount;
            protocolMetrics.HistogramMetricCount = (uint) histogramCount;

            var gaugeKeys = new List<string>();
            foreach (var entry in metrics.GaugeMetrics)
            {
                gaugeKeys.Add(entry.Key);
            }

            var histogramKeys = new List<string>();
            var totalBucketCount = 0;
            foreach (var entry in metrics.HistogramMetrics)
            {
                histogramKeys.Add(entry.Key);
                totalBucketCount += entry.Value.buckets.Count;
            }

            IList<int> gaugeKeysIndexes;
            IList<int> histogramKeysIndexes;
            fixed (CWorker.GaugeMetric* gaugeBuffer = new CWorker.GaugeMetric[gaugeCount])
            fixed (CWorker.HistogramMetric* histogramBuffer = new CWorker.HistogramMetric[histogramCount])
            fixed (CWorker.HistogramMetricBucket* bucketBuffer = new CWorker.HistogramMetricBucket[totalBucketCount])
            fixed (byte* gaugeKeysBuffer = ApiInterop.ToPackedUtf8Cstr(gaugeKeys, out gaugeKeysIndexes))
            fixed (byte* histogramKeysBuffer = ApiInterop.ToPackedUtf8Cstr(histogramKeys, out histogramKeysIndexes))
            {
                var i = 0;
                var bucketIndex = 0;
                foreach (var entry in metrics.GaugeMetrics)
                {
                    gaugeBuffer[i].Key = gaugeKeysBuffer + gaugeKeysIndexes[i];
                    gaugeBuffer[i].Value = entry.Value;
                    ++i;
                }

                i = 0;
                foreach (var entry in metrics.HistogramMetrics)
                {
                    var bucketCount = entry.Value.buckets.Count;
                    for (var j = 0; j < bucketCount; ++j)
                    {
                        bucketBuffer[j + bucketIndex].UpperBound = entry.Value.buckets[j].UpperBound;
                        bucketBuffer[j + bucketIndex].Samples = entry.Value.buckets[j].Samples;
                    }

                    histogramBuffer[i].Key = histogramKeysBuffer + histogramKeysIndexes[i];
                    histogramBuffer[i].Sum = entry.Value.sum;
                    histogramBuffer[i].BucketCount = (uint) bucketCount;
                    histogramBuffer[i].Buckets = bucketIndex + bucketBuffer;
                    bucketIndex += bucketCount;
                    entry.Value.ClearObservations();
                    ++i;
                }

                protocolMetrics.GaugeMetrics = gaugeBuffer;
                protocolMetrics.HistogramMetrics = histogramBuffer;
                CWorker.Connection_SendMetrics(connection, &protocolMetrics);
            }
        }

        [System.Obsolete("Use SendReserveEntityIdsRequest instead.")]
        public RequestId SendReserveEntityIdRequest(
            uint? timeoutMillis)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            uint localTimeoutMillis = timeoutMillis ?? 0;
            uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;

            return CWorker.Connection_SendReserveEntityIdRequest(connection, ptrTimeoutMillis);
        }

        public RequestId SendReserveEntityIdsRequest(
            uint numberOfEntityIds,
            uint? timeoutMillis)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            uint localTimeoutMillis = timeoutMillis ?? 0;
            uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;

            return CWorker.Connection_SendReserveEntityIdsRequest(
                connection, numberOfEntityIds, ptrTimeoutMillis);
        }

        public RequestId SendCreateEntityRequest(Entity entity, EntityId? entityId, uint? timeoutMillis)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            uint localTimeoutMillis = timeoutMillis ?? 0;
            uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;
            EntityId localEntityId = entityId ?? 0;
            EntityId* ptrEntityId = entityId.HasValue ? &localEntityId : null;

            var componentCount = entity.components.Count;
            var componentStorage = new CWorker.ComponentData[componentCount];
            var componentIndex = 0;

            foreach (var entry in entity.components)
            {
                componentStorage[componentIndex].ComponentId = entry.Key;
                componentStorage[componentIndex].UserHandle = (void*) entry.Value.UserHandle;
                if (entry.Value.SchemaData != null)
                {
                    componentStorage[componentIndex].SchemaType = entry.Value.SchemaData.Value.handle;
                }
                ++componentIndex;
            }

            RequestId requestId;
            fixed (CWorker.ComponentData* componentBuffer = componentStorage)
            {
                requestId = CWorker.Connection_SendCreateEntityRequest(
                    connection, (uint) componentCount, componentBuffer, ptrEntityId, ptrTimeoutMillis);
            }

            return requestId;
        }

        public RequestId SendDeleteEntityRequest(EntityId entityId, uint? timeoutMillis)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            uint localTimeoutMillis = timeoutMillis ?? 0;
            uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;
            return CWorker.Connection_SendDeleteEntityRequest(
                connection, entityId, ptrTimeoutMillis);
        }

        public RequestId SendEntityQueryRequest(Query.EntityQuery entityQuery, uint? timeoutMillis)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            uint localTimeoutMillis = timeoutMillis ?? 0;
            uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;

            CWorker.EntityQuery protocolQuery;
            protocolQuery.SnapshotResultTypeComponentIdCount = 0;
            uint[] componentIdArray = null;

            bool isCyclic =
                ParameterConversion.ConstraintIsCyclic(entityQuery.Constraint, new HashSet<Query.IConstraint>());
            if (isCyclic)
            {
                throw new ArgumentException("EntityQuery constraint graph contains a cycle!");
            }

            var constraintStorageSize = ParameterConversion.CountConstraintStorageRequirements(entityQuery.Constraint);
            var constraintArray = new CWorker.Constraint[constraintStorageSize];

            if (entityQuery.ResultType is Query.CountResultType)
            {
                protocolQuery.ResultType = (byte) CWorker.ResultType.Count;
            }
            else if (entityQuery.ResultType is Query.SnapshotResultType)
            {
                var snapshotResultType = (Query.SnapshotResultType) entityQuery.ResultType;
                protocolQuery.ResultType = (byte) CWorker.ResultType.Snapshot;

                if (snapshotResultType.ComponentIds != null)
                {
                    var count = snapshotResultType.ComponentIds.Count;
                    // Need to ensure the fixed statement doesn't produce null, which it's allowed
                    // to do for zero-length arrays.
                    componentIdArray = new uint[count > 0 ? count : 1];
                    protocolQuery.SnapshotResultTypeComponentIdCount = (uint) count;
                    for (int i = 0; i < count; ++i)
                    {
                        componentIdArray[i] = snapshotResultType.ComponentIds[i];
                    }
                }
            }
            else
            {
                throw new ArgumentException(
                    "Query result type is unknown or not set. Valid query result types are: " +
                    "CountResultType, SnapshotResultType.");
            }

            fixed (uint* componentIdBuffer = componentIdArray)
            fixed (CWorker.Constraint* constraintBuffer = constraintArray)
            {
                var constraintStorage = constraintBuffer;
                // We are not using a ternary operator here, because it breaks the IL2CPP compilation for iOS.
                protocolQuery.SnapshotResultTypeComponentIds = null;
                if (componentIdArray != null)
                {
                    protocolQuery.SnapshotResultTypeComponentIds = componentIdBuffer;
                }

                protocolQuery.Constraint =
                    ParameterConversion.ConvertConstraint(entityQuery.Constraint, ref constraintStorage);

                return CWorker.Connection_SendEntityQueryRequest(
                    connection, &protocolQuery, ptrTimeoutMillis);
            }
        }

        public void SendComponentInterest(EntityId entityId, Dictionary<uint, InterestOverride> interestOverrides)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            var protocolOverrides = new CWorker.InterestOverride[interestOverrides.Count];
            int i = 0;
            foreach (var entry in interestOverrides)
            {
                protocolOverrides[i].ComponentId = entry.Key;
                protocolOverrides[i].IsInterested = (byte) (entry.Value.IsInterested ? 1 : 0);
                ++i;
            }

            fixed (CWorker.InterestOverride* overridesBuffer = protocolOverrides)
            {
                CWorker.Connection_SendComponentInterest(
                    connection, entityId, overridesBuffer, (uint) interestOverrides.Count);
            }
        }

        public void SendAuthorityLossImminentAcknowledgement(EntityId entityId, uint componentId)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);
            CWorker.Connection_SendAuthorityLossImminentAcknowledgement(connection, entityId, componentId);
        }

        public void SendComponentUpdate(
            EntityId entityId,
            ComponentUpdate update,
            UpdateParameters parameters = null)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            var update_parameters = new CWorker.Alpha_UpdateParameters();
            if (parameters != null)
            {
                switch (parameters.Loopback)
                {
                case ComponentUpdateLoopback.None:
                    update_parameters.Loopback = (byte)CWorker.ComponentLoopback.None;
                    break;
                case ComponentUpdateLoopback.ShortCircuited:
                    update_parameters.Loopback = (byte)CWorker.ComponentLoopback.ShortCircuited;
                    break;
                }
            }
            var wrapper = new CWorker.ComponentUpdate
            {
                ComponentId = update.ComponentId,
                UserHandle = (void*) update.UserHandle,
                SchemaType = update.SchemaData != null ? update.SchemaData.Value.handle : null
            };
            CWorker.Alpha_Connection_SendComponentUpdate(connection, entityId, &wrapper, parameters != null ? &update_parameters : null);
        }

        public RequestId SendCommandRequest(
            EntityId entityId,
            CommandRequest request,
            Uint32 commandIndex,
            uint? timeoutMillis,
            CommandParameters parameters = default(CommandParameters))
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            var wrapper = new CWorker.CommandRequest
            {
                ComponentId = request.ComponentId,
                UserHandle = (void*) request.UserHandle,
                SchemaType = request.SchemaData != null ? request.SchemaData.Value.handle : null
            };

            uint localTimeoutMillis = timeoutMillis ?? 0;
            uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;

            var protocolParameters = new CWorker.CommandParameters
            {
                AllowShortCircuiting = (byte) (parameters != null && parameters.AllowShortCircuiting ? 1 : 0)
            };

            return CWorker.Connection_SendCommandRequest(
                connection, entityId, &wrapper, commandIndex, ptrTimeoutMillis, &protocolParameters);
        }

        public void SendCommandResponse(RequestId requestId, CommandResponse response)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            var wrapper = new CWorker.CommandResponse
            {
                ComponentId = response.ComponentId,
                UserHandle = (void*) response.UserHandle,
                SchemaType = response.SchemaData != null ? response.SchemaData.Value.handle : null
            };
            CWorker.Connection_SendCommandResponse(connection, requestId, &wrapper);
        }

        public void SendCommandFailure(RequestId requestId, string message)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            fixed (byte* messageBytes = ApiInterop.ToUtf8Cstr(message))
            {
                CWorker.Connection_SendCommandFailure(connection, requestId, messageBytes);
            }
        }

        public void SetProtocolLoggingEnabled(bool enabled)
        {
            Contract.Requires<ObjectDisposedException>(!connection.IsClosed, GetType().Name);

            CWorker.Connection_SetProtocolLoggingEnabled(connection, (byte) (enabled ? 1 : 0));
        }

        // In C#, creating a delegate from a method allocates memory and has its own lifetime.
        // To ensure that we don't run into any memory lifecycle issues, hide the methods and
        // only expose static delegates.
        private class CallbackThunkDelegates
        {
            public static readonly CWorker.GetFlagCallback GetFlagCallbackDelegate =
                GetFlagCallback;

            [MonoPInvokeCallback(typeof(CWorker.GetFlagCallback))]
            private static void GetFlagCallback(void* callbackHandlePtr, byte* value)
            {
                var callbackHandle = GCHandle.FromIntPtr((IntPtr) callbackHandlePtr);
                var callback = (Action<string>) callbackHandle.Target;

                if (value != null)
                {
                    callback(ApiInterop.FromUtf8Cstr(value));
                }
            }
        }
    }
}
