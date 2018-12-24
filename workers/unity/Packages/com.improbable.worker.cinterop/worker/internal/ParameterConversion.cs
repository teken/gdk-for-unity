using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Improbable.Worker.CInterop.Query;
using Uint8 = System.Byte;
using ComponentId = System.UInt32;

namespace Improbable.Worker.CInterop.Internal
{
    internal static class ParameterConversion
    {
        public unsafe delegate void ConnectionParametersCallback(CWorker.ConnectionParameters* parameters, List<WrappedGcHandle> componentVtableHandles);
        public unsafe delegate void SnapshotParametersCallback(CWorker.SnapshotParameters* parameters, List<WrappedGcHandle> componentVtableHandles);
        public unsafe delegate void PlayerIdentityTokenRequestCallback(CWorker.Alpha_PlayerIndentityTokenRequest* request);
        public unsafe delegate void LoginTokensRequestCallback(CWorker.Alpha_LoginTokensRequest* request);

        private static List<WrappedGcHandle> ConvertComponentVtables(
            Dictionary<uint, ComponentVtable> componentVtables, ComponentVtable defaultComponentVtable,
            out CWorker.ComponentVtable[] internalComponentVtables, out CWorker.ComponentVtable internalDefaultVtable)
        {
            internalComponentVtables = new CWorker.ComponentVtable[componentVtables.Count];
            int i = 0;
            var componentVtableHandles = new List<WrappedGcHandle>(internalComponentVtables.Length);
            foreach (var vtable in componentVtables)
            {
                var vtableHandle = ConvertVtable(vtable.Key, vtable.Value, ref internalComponentVtables[i]);
                if (vtableHandle != null)
                {
                    componentVtableHandles.Add(vtableHandle);
                }

                ++i;
            }

            internalDefaultVtable = new CWorker.ComponentVtable();
            if (defaultComponentVtable != null)
            {
                var defaultVtableHandle = ConvertVtable(0, defaultComponentVtable, ref internalDefaultVtable);
                componentVtableHandles.Add(defaultVtableHandle);
            }

            return componentVtableHandles;
        }

        private static unsafe WrappedGcHandle ConvertVtable(uint componentId, ComponentVtable vtable,
            ref CWorker.ComponentVtable internalVtable)
        {
            var wrappedVtableObject = new WrappedGcHandle(vtable);

            internalVtable.ComponentId = componentId;
            internalVtable.UserData = (void*) wrappedVtableObject.Get();
            internalVtable.CommandRequestFree = vtable.CommandRequestFree == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.commandRequestFree);
            internalVtable.CommandRequestCopy = vtable.CommandRequestCopy == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.commandRequestCopy);
            internalVtable.CommandRequestDeserialize = vtable.CommandRequestDeserialize == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.commandRequestDeserialize);
            internalVtable.CommandRequestSerialize = vtable.CommandRequestSerialize == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.commandRequestSerialize);
            internalVtable.CommandResponseFree = vtable.CommandResponseFree == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.commandResponseFree);
            internalVtable.CommandResponseCopy = vtable.CommandResponseCopy == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.commandResponseCopy);
            internalVtable.CommandResponseDeserialize = vtable.CommandResponseDeserialize == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.commandResponseDeserialize);
            internalVtable.CommandResponseSerialize = vtable.CommandResponseSerialize == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.commandResponseSerialize);
            internalVtable.ComponentDataFree = vtable.ComponentDataFree == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.componentDataFree);
            internalVtable.ComponentDataCopy = vtable.ComponentDataCopy == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.componentDataCopy);
            internalVtable.ComponentDataDeserialize = vtable.ComponentDataDeserialize == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.componentDataDeserialize);
            internalVtable.ComponentDataSerialize = vtable.ComponentDataSerialize == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.componentDataSerialize);
            internalVtable.ComponentUpdateFree = vtable.ComponentUpdateFree == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.componentUpdateFree);
            internalVtable.ComponentUpdateCopy = vtable.ComponentUpdateCopy == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.componentUpdateCopy);
            internalVtable.ComponentUpdateDeserialize = vtable.ComponentUpdateDeserialize == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.componentUpdateDeserialize);
            internalVtable.ComponentUpdateSerialize = vtable.ComponentUpdateSerialize == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CallbackThunkDelegates.componentUpdateSerialize);

            return wrappedVtableObject;
        }

        public static bool ConstraintIsCyclic(IConstraint constraint,
                                              HashSet<IConstraint> constraintSet)
        {
            if (constraintSet.Contains(constraint))
            {
                return true;
            }

            constraintSet.Add(constraint);
            if (constraint is List<IConstraint>)
            {
                var combinedConstraint = (List<IConstraint>) constraint;
                foreach (var c in combinedConstraint)
                {
                    if (ConstraintIsCyclic(c, constraintSet))
                    {
                        return true;
                    }
                }
            }
            constraintSet.Remove(constraint);
            return false;
        }

        public static uint CountConstraintStorageRequirements(IConstraint constraint)
        {
            uint result = 0;
            if (constraint is List<IConstraint>)
            {
                var combinedConstraint = (List<IConstraint>)constraint;
                result += (uint)combinedConstraint.Count;
                foreach (var c in combinedConstraint)
                {
                    result += CountConstraintStorageRequirements(c);
                }
            }
            else if (constraint is NotConstraint)
            {
                result += 1;
            }
            return result;
        }

        public static unsafe CWorker.Constraint ConvertConstraint(
            IConstraint constraint, ref CWorker.Constraint* storage)
        {
            // The storage parameter should point to a buffer pre-alloced with enough space to store
            // all transitive children of the given constraint (this is computed by calling
            // CountConstraintStorageRequirements). We flatten the constraint graph into this single
            // buffer such that the children of a single node are always stored in a contiguous
            // subarray, and all children are pointers back into the buffer. This is so that we can
            // pass the constraint graph to the AST with the usual C# limitation that we can fix at
            // most a constant number of buffers.
            CWorker.Constraint result;
            result.ConstraintType = 0;
            result.ConstraintTypeUnion = new CWorker.Constraint.Union();

            if (constraint is EntityIdConstraint)
            {
                var entityId = (EntityIdConstraint)constraint;
                result.ConstraintType = (byte)CWorker.ConstraintType.EntityId;
                result.ConstraintTypeUnion.EntityIdConstraint.EntityId = entityId.EntityId;
            }
            else if (constraint is ComponentConstraint)
            {
                var component = (ComponentConstraint)constraint;

                result.ConstraintType = (byte)CWorker.ConstraintType.Component;
                result.ConstraintTypeUnion.ComponentConstraint.ComponentId = component.ComponentId;
            }
            else if (constraint is SphereConstraint)
            {
                var sphere = (SphereConstraint)constraint;
                result.ConstraintType = (byte)CWorker.ConstraintType.Sphere;
                result.ConstraintTypeUnion.SphereConstraint.X = sphere.X;
                result.ConstraintTypeUnion.SphereConstraint.Y = sphere.Y;
                result.ConstraintTypeUnion.SphereConstraint.Z = sphere.Z;
                result.ConstraintTypeUnion.SphereConstraint.Radius = sphere.Radius;
            }
            else if (constraint is List<IConstraint>)
            {
                var combinedConstraint = (List<IConstraint>)constraint;
                var startOfStorage = storage;
                storage += combinedConstraint.Count;
                for (int i = 0; i < combinedConstraint.Count; ++i)
                {
                    startOfStorage[i] = ConvertConstraint(combinedConstraint[i], ref storage);
                }

                if (constraint is AndConstraint)
                {
                    result.ConstraintType = (byte)CWorker.ConstraintType.And;
                    result.ConstraintTypeUnion.AndConstraint.ConstraintCount = (uint)combinedConstraint.Count;
                    result.ConstraintTypeUnion.AndConstraint.Constraints = startOfStorage;
                }
                if (constraint is OrConstraint)
                {
                    result.ConstraintType = (byte)CWorker.ConstraintType.Or;
                    result.ConstraintTypeUnion.OrConstraint.ConstraintCount = (uint)combinedConstraint.Count;
                    result.ConstraintTypeUnion.OrConstraint.Constraints = startOfStorage;
                }
            }
            else if (constraint is NotConstraint)
            {
                var notConstraint = (NotConstraint)constraint;
                var startOfStorage = storage;
                storage++;
                startOfStorage[0] = ConvertConstraint(notConstraint.Constraint, ref storage);
                result.ConstraintType = (byte)CWorker.ConstraintType.Not;
                result.ConstraintTypeUnion.NotConstraint.Constraint = startOfStorage;
            }
            else
            {
                throw new ArgumentException("Query constraint is unknown or not set. Valid query constraints are: EntityIdConstraint, " +
                                            "ComponentConstraint, SphereConstraint, AndConstraint, OrConstraint, NotConstraint.");
            }
            return result;
        }

        public static unsafe void ConvertPlayerIdentityTokenRequest(
            Alpha.PlayerIdentityTokenRequest request,
            PlayerIdentityTokenRequestCallback callback)
        {
            CWorker.Alpha_PlayerIndentityTokenRequest interop_request;
            uint durationSeconds = request.DurationSeconds ?? 0;
            fixed (byte* devAuthTokenIdBytes = ApiInterop.ToUtf8Cstr(request.DevelopmentAuthenticationTokenId))
            fixed (byte* playerIdBytes = ApiInterop.ToUtf8Cstr(request.PlayerId))
            fixed (byte* displayNameBytes = ApiInterop.ToUtf8Cstr(request.DisplayName))
            fixed (byte* metaDataBytes = ApiInterop.ToUtf8Cstr(request.Metadata))
            {
                interop_request.DevelopmentAuthenticationTokenId = devAuthTokenIdBytes;
                interop_request.PlayerId = playerIdBytes;
                interop_request.DurationSeconds = request.DurationSeconds.HasValue ? &durationSeconds : null;
                interop_request.DisplayName = displayNameBytes;
                interop_request.Metadata = metaDataBytes;
                interop_request.UseInsecureConnection = (byte) (request.UseInsecureConnection ? 1 : 0);
                callback(&interop_request);
            }
        }

        public static unsafe void ConvertLoginTokensRequest(Alpha.LoginTokensRequest request,
                                                            LoginTokensRequestCallback callback)
        {
            CWorker.Alpha_LoginTokensRequest interop_request;
            uint durationSeconds = request.DurationSeconds ?? 0;
            fixed (byte* playerIdentityTokenBytes = ApiInterop.ToUtf8Cstr(request.PlayerIdentityToken))
            fixed (byte* workerTypeBytes = ApiInterop.ToUtf8Cstr(request.WorkerType))
            {
                interop_request.PlayerIdentityToken = playerIdentityTokenBytes;
                interop_request.WorkerType = workerTypeBytes;
                interop_request.DurationSeconds = request.DurationSeconds.HasValue ? &durationSeconds : null;
                interop_request.UseInsecureConnection = (byte) (request.UseInsecureConnection ? 1 : 0);
                callback(&interop_request);
            }
        }

        public static unsafe void ConvertConnectionParameters(ConnectionParameters connectionParams,
                                                              ConnectionParametersCallback callback)
        {
            CWorker.ConnectionParameters parameters;

            parameters.Network.UseExternalIp = (byte)(connectionParams.Network.UseExternalIp ? 1 : 0);
            switch (connectionParams.Network.ConnectionType)
            {
                case NetworkConnectionType.Tcp:
                    parameters.Network.ConnectionType = (byte)CWorker.NetworkConnectionType.Tcp;
                    break;
                case NetworkConnectionType.RakNet:
                    parameters.Network.ConnectionType = (byte)CWorker.NetworkConnectionType.RakNet;
                    break;
                case NetworkConnectionType.Kcp:
                    parameters.Network.ConnectionType = (byte)CWorker.NetworkConnectionType.Kcp;
                    break;
            }
            parameters.Network.ConnectionTimeoutMillis = connectionParams.Network.ConnectionTimeoutMillis;
            parameters.Network.DefaultCommandTimeoutMillis = connectionParams.Network.DefaultCommandTimeoutMillis;
            parameters.Network.Tcp.MultiplexLevel = connectionParams.Network.Tcp.MultiplexLevel;
            parameters.Network.Tcp.NoDelay = (byte)(connectionParams.Network.Tcp.NoDelay ? 1 : 0);
            parameters.Network.Tcp.SendBufferSize = connectionParams.Network.Tcp.SendBufferSize;
            parameters.Network.Tcp.ReceiveBufferSize = connectionParams.Network.Tcp.ReceiveBufferSize;
            parameters.Network.RakNet.HeartbeatTimeoutMillis = connectionParams.Network.RakNet.HeartbeatTimeoutMillis;
            parameters.Network.Kcp.FastRetransmission =
                (byte)(connectionParams.Network.Kcp.FastRetransmission ? 1 : 0);
            parameters.Network.Kcp.EarlyRetransmission =
                (byte)(connectionParams.Network.Kcp.EarlyRetransmission ? 1 : 0);
            parameters.Network.Kcp.NonConcessionalFlowControl =
                (byte)(connectionParams.Network.Kcp.NonConcessionalFlowControl ? 1 : 0);
            parameters.Network.Kcp.MultiplexLevel = connectionParams.Network.Kcp.MultiplexLevel;
            parameters.Network.Kcp.UpdateIntervalMillis =
                connectionParams.Network.Kcp.UpdateIntervalMillis;
            parameters.Network.Kcp.MinRtoMillis = connectionParams.Network.Kcp.MinRtoMillis;
            parameters.Network.Kcp.WindowSize = connectionParams.Network.Kcp.WindowSize;
            parameters.Network.Kcp.EnableErasureCodec =
                (byte)(connectionParams.Network.Kcp.EnableErasureCodec ? 1 : 0);
            parameters.Network.Kcp.ErasureCodec.OriginalPacketCount =
                connectionParams.Network.Kcp.ErasureCodec.OriginalPacketCount;
            parameters.Network.Kcp.ErasureCodec.RecoveryPacketCount =
                connectionParams.Network.Kcp.ErasureCodec.RecoveryPacketCount;
            parameters.Network.Kcp.ErasureCodec.WindowSize =
                connectionParams.Network.Kcp.ErasureCodec.WindowSize;
            parameters.Network.Kcp.Heartbeat.IntervalMillis =
                connectionParams.Network.Kcp.Heartbeat.IntervalMillis;
            parameters.Network.Kcp.Heartbeat.TimeoutMillis =
                connectionParams.Network.Kcp.Heartbeat.TimeoutMillis;
            parameters.SendQueueCapacity = connectionParams.SendQueueCapacity;
            parameters.ReceiveQueueCapacity = connectionParams.ReceiveQueueCapacity;
            parameters.LogMessageQueueCapacity = connectionParams.LogMessageQueueCapacity;
            parameters.BuiltInMetricsReportPeriodMillis = connectionParams.BuiltInMetricsReportPeriodMillis;
            parameters.EnableProtocolLoggingAtStartup = (byte)(connectionParams.EnableProtocolLoggingAtStartup ? 1 : 0);
            parameters.ThreadAffinity.ReceiveThreadsAffinityMask = connectionParams.ThreadAffinity.ReceiveThreadsAffinityMask;
            parameters.ThreadAffinity.SendThreadsAffinityMask = connectionParams.ThreadAffinity.SendThreadsAffinityMask;
            parameters.ThreadAffinity.TemporaryThreadsAffinityMask = connectionParams.ThreadAffinity.TemporaryThreadsAffinityMask;
            parameters.ProtocolLogging.MaxLogFiles = connectionParams.ProtocolLogging.MaxLogFiles;
            parameters.ProtocolLogging.MaxLogFileSizeBytes = connectionParams.ProtocolLogging.MaxLogFileSizeBytes;

            // Convert vtables.
            CWorker.ComponentVtable[] componentVtables;
            CWorker.ComponentVtable defaultComponentVtable;
            var componentVtableHandles = ConvertComponentVtables(connectionParams.ComponentVtables,
                connectionParams.DefaultComponentVtable, out componentVtables, out defaultComponentVtable);
            parameters.ComponentVtableCount = (uint) connectionParams.ComponentVtables.Count;
            parameters.DefaultComponentVtable = connectionParams.DefaultComponentVtable != null ? &defaultComponentVtable : null;

            fixed (byte* workerTypeBytes = ApiInterop.ToUtf8Cstr(connectionParams.WorkerType))
            fixed (CWorker.ComponentVtable* componentVtableBuffer = componentVtables)
            fixed (byte* logPrefixBytes = ApiInterop.ToUtf8Cstr(connectionParams.ProtocolLogging.LogPrefix))
            {
                parameters.WorkerType = workerTypeBytes;
                parameters.ComponentVtables = componentVtableBuffer;
                parameters.ProtocolLogging.LogPrefix = logPrefixBytes;

                callback(&parameters, componentVtableHandles);
            }
        }

        internal static unsafe Func<uint?, Connection> ConnectionFutureGet(ConnectionFutureHandle future, List<WrappedGcHandle> componentVtableHandles)
        {
            return timeoutMillis =>
            {
                uint localTimeoutMillis = timeoutMillis ?? 0;
                uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;
                ConnectionHandle connection = CWorker.ConnectionFuture_Get(future, ptrTimeoutMillis);
                return !connection.IsInvalid ? new Connection(connection, componentVtableHandles) : null;
            };
        }

        internal unsafe static Func<uint?, Alpha.PlayerIdentityTokenResponse?>
            PlayerIdentityTokenResponseFutureGet(Alpha_PlayerIdentityTokenResponseFutureHandle future)
        {
           return timeoutMillis =>
           {
                uint localTimeoutMillis = timeoutMillis ?? 0;
                uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;
                Alpha.PlayerIdentityTokenResponse? result = null;

                Action<Alpha.PlayerIdentityTokenResponse> callback = value => result = value;
                using (var callbackHandle = new WrappedGcHandle(callback))
                {
                    CWorker.Alpha_PlayerIdentityTokenResponseFuture_Get(future, ptrTimeoutMillis,
                        (void*) callbackHandle.Get(), CallbackThunkDelegates.PlayerIdentityTokenResponseThunkDelegate);
                }

                return result;
           };
        }

        internal unsafe static Func<uint?, Alpha.LoginTokensResponse?>
            LoginTokenDetailsFutureGet(Alpha_LoginTokensResponseFutureHandle future)
        {
            return timeoutMillis =>
            {
                uint localTimeoutMillis = timeoutMillis.HasValue ? timeoutMillis.Value : 0;
                uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;
                Alpha.LoginTokensResponse? result = null;

                Action<Alpha.LoginTokensResponse> callback = value => result = value;
                using (var callbackHandle = new WrappedGcHandle(callback))
                {
                    CWorker.Alpha_LoginTokensResponseFuture_Get(future, ptrTimeoutMillis,
                        (void*) callbackHandle.Get(), CallbackThunkDelegates.LoginTokensResponseThunkDelegate);
                }

                return result;
            };
        }

        internal static unsafe Func<uint?, DeploymentList?>
        DeploymentListFutureGet(DeploymentListFutureHandle future)
        {
            return timeoutMillis =>
            {
                uint localTimeoutMillis = timeoutMillis ?? 0;
                uint* ptrTimeoutMillis = timeoutMillis.HasValue ? &localTimeoutMillis : null;
                DeploymentList? result = null;

                Action<DeploymentList> callback = value => result = value;
                using (var callbackHandle = new WrappedGcHandle(callback))
                {
                    CWorker.DeploymentListFuture_Get(
                        future, ptrTimeoutMillis, (void*) callbackHandle.Get(),
                        CallbackThunkDelegates.DeploymentListThunkDelegate);
                }

                return result;
            };
        }

        public static unsafe void ConvertSnapshotParameters(SnapshotParameters snapshotParams,
                                                            SnapshotParametersCallback callback)
        {
            CWorker.SnapshotParameters parameters;

            CWorker.ComponentVtable[] componentVtables;
            CWorker.ComponentVtable defaultComponentVtable;
            var componentVtableHandles = ConvertComponentVtables(
                snapshotParams.ComponentVtables, snapshotParams.DefaultComponentVtable,
                out componentVtables, out defaultComponentVtable);
            parameters.ComponentVtableCount = (uint)componentVtables.Length;
            parameters.DefaultComponentVtable = snapshotParams.DefaultComponentVtable != null ? &defaultComponentVtable : null;

            fixed (CWorker.ComponentVtable* componentVtablesBuffer = componentVtables)
            {
                parameters.ComponentVtables = componentVtablesBuffer;
                callback(&parameters, componentVtableHandles);
            }
        }

        public static void FreeVtableHandles(List<WrappedGcHandle> vtableHandles)
        {
            foreach (var vtableHandle in vtableHandles)
            {
                vtableHandle.Dispose();
            }
        }

        // In C#, creating a delegate from a method allocates memory and has its own lifetime.
        // To ensure that we don't run into any memory lifecycle issues, hide the methods and
        // only expose static delegates.
        private unsafe class CallbackThunkDelegates
        {
            public static readonly unsafe CWorker.CommandRequestFree commandRequestFree = CommandRequestFree;
            public static readonly unsafe CWorker.CommandResponseFree commandResponseFree = CommandResponseFree;
            public static readonly unsafe CWorker.ComponentDataFree componentDataFree = ComponentDataFree;
            public static readonly unsafe CWorker.ComponentUpdateFree componentUpdateFree = ComponentUpdateFree;

            public static readonly unsafe CWorker.CommandRequestCopy commandRequestCopy = CommandRequestCopy;
            public static readonly unsafe CWorker.CommandResponseCopy commandResponseCopy = CommandResponseCopy;
            public static readonly unsafe CWorker.ComponentDataCopy componentDataCopy = ComponentDataCopy;
            public static readonly unsafe CWorker.ComponentUpdateCopy componentUpdateCopy = ComponentUpdateCopy;

            public static readonly unsafe CWorker.CommandRequestDeserialize commandRequestDeserialize = CommandRequestDeserialize;
            public static readonly unsafe CWorker.CommandResponseDeserialize commandResponseDeserialize = CommandResponseDeserialize;
            public static readonly unsafe CWorker.ComponentDataDeserialize componentDataDeserialize = ComponentDataDeserialize;
            public static readonly unsafe CWorker.ComponentUpdateDeserialize componentUpdateDeserialize = ComponentUpdateDeserialize;

            public static readonly unsafe CWorker.CommandRequestSerialize commandRequestSerialize = CommandRequestSerialize;
            public static readonly unsafe CWorker.CommandResponseSerialize commandResponseSerialize = CommandResponseSerialize;
            public static readonly unsafe CWorker.ComponentDataSerialize componentDataSerialize = ComponentDataSerialize;
            public static readonly unsafe CWorker.ComponentUpdateSerialize componentUpdateSerialize = ComponentUpdateSerialize;

            public static readonly CWorker.Alpha_PlayerIdentityTokenResponseCallback
                PlayerIdentityTokenResponseThunkDelegate = PlayerIdentityTokenResponseThunk;
            public static readonly CWorker.Alpha_LoginTokensResponseCallback
                LoginTokensResponseThunkDelegate = LoginTokensResponseThunk;
            public static readonly CWorker.DeploymentListCallback DeploymentListThunkDelegate =
                DeploymentListThunk;

            [MonoPInvokeCallback(typeof(CWorker.CommandRequestFree))]
            private static unsafe void CommandRequestFree(
                ComponentId componentId, void* userData, void* handle)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                vtable.CommandRequestFree(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
            }

            [MonoPInvokeCallback(typeof(CWorker.CommandResponseFree))]
            private static unsafe void CommandResponseFree(
                ComponentId componentId, void* userData, void* handle)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                vtable.CommandResponseFree(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
            }

            [MonoPInvokeCallback(typeof(CWorker.ComponentDataFree))]
            private static unsafe void ComponentDataFree(
                ComponentId componentId, void* userData, void* handle)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                vtable.ComponentDataFree(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
            }

            [MonoPInvokeCallback(typeof(CWorker.ComponentUpdateFree))]
            private static unsafe void ComponentUpdateFree(
                ComponentId componentId, void* userData, void* handle)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                vtable.ComponentUpdateFree(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
            }

            [MonoPInvokeCallback(typeof(CWorker.CommandRequestCopy))]
            private static unsafe void* CommandRequestCopy(
                ComponentId componentId, void* userData, void* handle)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                return (void*) vtable.CommandRequestCopy(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
            }

            [MonoPInvokeCallback(typeof(CWorker.CommandResponseCopy))]
            private static unsafe void* CommandResponseCopy(
                ComponentId componentId, void* userData, void* handle)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                return (void*) vtable.CommandResponseCopy(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
            }

            [MonoPInvokeCallback(typeof(CWorker.ComponentDataCopy))]
            private static unsafe void* ComponentDataCopy(
                ComponentId componentId, void* userData, void* handle)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                return (void*) vtable.ComponentDataCopy(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
            }

            [MonoPInvokeCallback(typeof(CWorker.ComponentUpdateCopy))]
            private static unsafe void* ComponentUpdateCopy(
                ComponentId componentId, void* userData, void* handle)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                return (void*) vtable.ComponentUpdateCopy(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
            }

            [MonoPInvokeCallback(typeof(CWorker.CommandRequestDeserialize))]
            private static unsafe Uint8 CommandRequestDeserialize(
                ComponentId componentId, void* userData, CSchema.CommandRequest* source, void** handleOut)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                UIntPtr resultHandle;
                *handleOut = null;
                if (vtable.CommandRequestDeserialize(componentId, (UIntPtr) vtable.UserData, new SchemaCommandRequest(source), out resultHandle))
                {
                    *handleOut = (void*) resultHandle;
                    return 1;
                }
                return 0;
            }

            [MonoPInvokeCallback(typeof(CWorker.CommandResponseDeserialize))]
            private static unsafe Uint8 CommandResponseDeserialize(
                ComponentId componentId, void* userData, CSchema.CommandResponse* source, void** handleOut)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                UIntPtr resultHandle;
                *handleOut = null;
                if (vtable.CommandResponseDeserialize(componentId, (UIntPtr) vtable.UserData, new SchemaCommandResponse(source), out resultHandle))
                {
                    *handleOut = (void*) resultHandle;
                    return 1;
                }
                return 0;
            }

            [MonoPInvokeCallback(typeof(CWorker.ComponentDataDeserialize))]
            private static unsafe Uint8 ComponentDataDeserialize(
                ComponentId componentId, void* userData, CSchema.ComponentData* source, void** handleOut)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                UIntPtr resultHandle;
                *handleOut = null;
                if (vtable.ComponentDataDeserialize(componentId, (UIntPtr) vtable.UserData, new SchemaComponentData(source), out resultHandle))
                {
                    *handleOut = (void*) resultHandle;
                    return 1;
                }
                return 0;
            }

            [MonoPInvokeCallback(typeof(CWorker.ComponentUpdateDeserialize))]
            private static unsafe Uint8 ComponentUpdateDeserialize(
                ComponentId componentId, void* userData, CSchema.ComponentUpdate* source, void** handleOut)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                UIntPtr resultHandle;
                *handleOut = null;
                if (vtable.ComponentUpdateDeserialize(componentId, (UIntPtr) vtable.UserData, new SchemaComponentUpdate(source), out resultHandle))
                {
                    *handleOut = (void*) resultHandle;
                    return 1;
                }
                return 0;
            }

            [MonoPInvokeCallback(typeof(CWorker.CommandRequestSerialize))]
            private static unsafe void CommandRequestSerialize(
                ComponentId componentId, void* userData, void* handle, CSchema.CommandRequest** targetOut)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                var target = vtable.CommandRequestSerialize(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
                *targetOut = target != null ? target.Value.handle : null;
            }

            [MonoPInvokeCallback(typeof(CWorker.CommandResponseSerialize))]
            private static unsafe void CommandResponseSerialize(
                ComponentId componentId, void* userData, void* handle, CSchema.CommandResponse** targetOut)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                var target = vtable.CommandResponseSerialize(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
                *targetOut = target != null ? target.Value.handle : null;
            }

            [MonoPInvokeCallback(typeof(CWorker.ComponentDataSerialize))]
            private static unsafe void ComponentDataSerialize(
                ComponentId componentId, void* userData, void* handle, CSchema.ComponentData** targetOut)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                var target = vtable.ComponentDataSerialize(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
                *targetOut = target != null ? target.Value.handle : null;
            }

            [MonoPInvokeCallback(typeof(CWorker.ComponentUpdateSerialize))]
            private static unsafe void ComponentUpdateSerialize(
                ComponentId componentId, void* userData, void* handle, CSchema.ComponentUpdate** targetOut)
            {
                var vtable = (ComponentVtable) GCHandle.FromIntPtr((IntPtr) userData).Target;
                var target = vtable.ComponentUpdateSerialize(componentId, (UIntPtr) vtable.UserData, (UIntPtr) handle);
                *targetOut = target != null ? target.Value.handle : null;
            }

            [MonoPInvokeCallback(typeof(CWorker.Alpha_PlayerIdentityTokenResponseCallback))]
            private static unsafe void PlayerIdentityTokenResponseThunk(void* callbackHandlePtr,
                CWorker.Alpha_PlayerIdentityTokenResponse* response)
            {
                var callbackHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr) callbackHandlePtr);
                var callback = (Action<Alpha.PlayerIdentityTokenResponse>) callbackHandle.Target;

                var wrapper = new Alpha.PlayerIdentityTokenResponse
                {
                    PlayerIdentityToken = ApiInterop.FromUtf8Cstr(response->PlayerIdentityToken),
                    Status = (ConnectionStatusCode) response->ConnectionStatusCode,
                    Error = response->Error == null ? null : ApiInterop.FromUtf8Cstr(response->Error),
                };
                callback(wrapper);
            }

            [MonoPInvokeCallback(typeof(CWorker.Alpha_LoginTokensResponseCallback))]
            private static unsafe void LoginTokensResponseThunk(void* callbackHandlePtr,
                CWorker.Alpha_LoginTokensResponse* response)
            {
                var callbackHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr) callbackHandlePtr);
                var callback = (Action<Alpha.LoginTokensResponse>) callbackHandle.Target;

                var list = new List<Alpha.LoginTokenDetails>();
                for (int i = 0; i < response->LoginTokenCount; i++)
                {
                    var tags = new List<string>();
                    for (int j = 0; j < response->LoginToken[i].TagCount; j++)
                    {
                        tags.Add(ApiInterop.FromUtf8Cstr(response->LoginToken[i].Tags[j]));
                    }

                    var loginToken = new Alpha.LoginTokenDetails
                    {
                        DeploymentId = ApiInterop.FromUtf8Cstr(response->LoginToken[i].DeploymentId),
                        DeploymentName = ApiInterop.FromUtf8Cstr(response->LoginToken[i].DeploymentName),
                        Tags = tags,
                        LoginToken = ApiInterop.FromUtf8Cstr(response->LoginToken[i].LoginToken),
                    };
                    list.Add(loginToken);
                }

                var wrapper = new Alpha.LoginTokensResponse
                {
                    LoginTokens = list,
                    Status = (ConnectionStatusCode) response->ConnectionStatusCode,
                    Error = response->Error == null ? null : ApiInterop.FromUtf8Cstr(response->Error),
                };
                callback(wrapper);
            }

            [MonoPInvokeCallback(typeof(CWorker.DeploymentListCallback))]
            private static unsafe void DeploymentListThunk(void* callbackHandlePtr, CWorker.DeploymentList* deploymentList)
            {
                var callbackHandle = GCHandle.FromIntPtr((IntPtr) callbackHandlePtr);
                var callback = (Action<DeploymentList>) callbackHandle.Target;

                var wrapper = new DeploymentList { Deployments = new List<Deployment>() };
                for (uint i = 0; i < deploymentList->DeploymentCount; ++i)
                {
                    var deployment = new Deployment
                    {
                        DeploymentName = ApiInterop.FromUtf8Cstr(deploymentList->Deployments[i].DeploymentName),
                        AssemblyName = ApiInterop.FromUtf8Cstr(deploymentList->Deployments[i].AssemblyName),
                        Description = ApiInterop.FromUtf8Cstr(deploymentList->Deployments[i].Description),
                        UsersConnected = deploymentList->Deployments[i].UsersConnected,
                        UsersCapacity = deploymentList->Deployments[i].UsersCapacity,
                    };
                    wrapper.Deployments.Add(deployment);
                }
                wrapper.Error = deploymentList->Error == null ? null : ApiInterop.FromUtf8Cstr(deploymentList->Error);
                callback(wrapper);
            }
        }
    }
}