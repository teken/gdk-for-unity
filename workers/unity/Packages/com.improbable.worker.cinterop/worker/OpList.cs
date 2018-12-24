using Improbable.Worker.CInterop.Internal;
using System;
using System.Collections.Generic;
using EntityId = System.Int64;
using RequestId = System.UInt32;
using Uint32 = System.UInt32;

namespace Improbable.Worker.CInterop
{
    public enum OpType
    {
        Disconnect = 1,
        FlagUpdate = 2,
        LogMessage = 3,
        Metrics = 4,
        CriticalSection = 5,
        AddEntity = 6,
        RemoveEntity = 7,
        ReserveEntityIdResponse = 8,
        ReserveEntityIdsResponse = 9,
        CreateEntityResponse = 10,
        DeleteEntityResponse = 11,
        EntityQueryResponse = 12,
        AddComponent = 13,
        RemoveComponent = 14,
        AuthorityChange = 15,
        ComponentUpdate = 16,
        CommandRequest = 17,
        CommandResponse = 18,
    }

    public enum StatusCode
    {
        Success = 1,
        Timeout = 2,
        NotFound = 3,
        AuthorityLost = 4,
        PermissionDenied = 5,
        ApplicationError = 6,
        InternalError = 7,
    }

    public enum Authority
    {
        NotAuthoritative = 0,
        Authoritative = 1,
        AuthorityLossImminent = 2,
    }

    public struct DisconnectOp
    {
        public ConnectionStatusCode ConnectionStatusCode;
        public string Reason;
    }

    public struct FlagUpdateOp
    {
        public string Name;
        public string Value;
    }

    public struct LogMessageOp
    {
        public string Message;
        public LogLevel Level;
    }

    public struct MetricsOp
    {
        public Metrics Metrics;
    }

    public struct CriticalSectionOp
    {
        public bool InCriticalSection;
    }

    public struct AddEntityOp
    {
        public EntityId EntityId;
    }

    public struct RemoveEntityOp
    {
        public EntityId EntityId;
    }
    
    public struct ReserveEntityIdResponseOp
    {
        public RequestId RequestId;
        public StatusCode StatusCode;
        public string Message;
        public EntityId? EntityId;
    }

    public struct ReserveEntityIdsResponseOp
    {
        public RequestId RequestId;
        public StatusCode StatusCode;
        public string Message;
        public EntityId? FirstEntityId;
        public int NumberOfEntityIds;
    }

    public struct CreateEntityResponseOp
    {
        public RequestId RequestId;
        public StatusCode StatusCode;
        public string Message;
        public EntityId? EntityId;
    }

    public struct DeleteEntityResponseOp
    {
        public RequestId RequestId;
        public EntityId EntityId;
        public StatusCode StatusCode;
        public string Message;
    }

    public struct EntityQueryResponseOp
    {
        public RequestId RequestId;
        public StatusCode StatusCode;
        public string Message;
        public int ResultCount;
        public Dictionary<EntityId, Entity> Result;
    }

    public struct AddComponentOp
    {
        public EntityId EntityId;
        public ComponentData Data;
    }

    public struct RemoveComponentOp
    {
        public EntityId EntityId;
        public uint ComponentId;
    }

    public struct AuthorityChangeOp
    {
        public EntityId EntityId;
        public uint ComponentId;
        public Authority Authority;
    }

    public struct ComponentUpdateOp
    {
        public EntityId EntityId;
        public ComponentUpdate Update;
    }

    public struct CommandRequestOp
    {
        public RequestId RequestId;
        public EntityId EntityId;
        public uint TimeoutMillis;
        public string CallerWorkerId;
        public List<string> CallerAttributeSet;
        public CommandRequest Request;
    }

    public struct CommandResponseOp
    {
        public RequestId RequestId;
        public EntityId EntityId;
        public StatusCode StatusCode;
        public string Message;
        public CommandResponse Response;
        public Uint32 CommandIndex;
    }

    public sealed unsafe class OpList : IDisposable
    {
        private readonly OpListHandle opList;

        internal OpList(OpListHandle opList)
        {
            this.opList = opList;
        }

        /// <inheritdoc cref="IDisposable"/>
        public void Dispose()
        {
            opList.Dispose();
        }

        public int GetOpCount()
        {
            return (int) opList.GetUnderlying()->OpCount;
        }

        public OpType GetOpType(int opIndex)
        {
            return (OpType) GetOpInternal(opIndex)->OpType;
        }

        private CWorker.Op* GetOpInternal(int opIndex)
        {
            if (opIndex < 0 || opIndex >= opList.GetUnderlying()->OpCount)
            {
                throw new IndexOutOfRangeException();
            }
            return &opList.GetUnderlying()->Ops[opIndex];
        }

        public DisconnectOp GetDisconnectOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.Disconnect;

            DisconnectOp wrapper;
            wrapper.ConnectionStatusCode = (ConnectionStatusCode) op->ConnectionStatusCode;
            wrapper.Reason = ApiInterop.FromUtf8Cstr(op->Reason);
            return wrapper;
        }

        public FlagUpdateOp GetFlagUpdateOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.FlagUpdate;

            FlagUpdateOp wrapper;
            wrapper.Name = ApiInterop.FromUtf8Cstr(op->Name);
            wrapper.Value = null;
            if (op->Value != null)
            {
                wrapper.Value = ApiInterop.FromUtf8Cstr(op->Value);
            }
            return wrapper;
        }

        public LogMessageOp GetLogMessageOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.LogMessage;

            LogMessageOp wrapper;
            wrapper.Level = (LogLevel) op->LogLevel;
            wrapper.Message = ApiInterop.FromUtf8Cstr(op->Message);
            return wrapper;
        }

        public MetricsOp GetMetricsOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.Metrics;

            MetricsOp wrapper;
            wrapper.Metrics = new Metrics();
            for (uint i = 0; i < op->Metrics.GaugeMetricCount; ++i)
            {
                wrapper.Metrics.GaugeMetrics.Add(ApiInterop.FromUtf8Cstr(op->Metrics.GaugeMetrics[i].Key),
                                                                      op->Metrics.GaugeMetrics[i].Value);
            }
            return wrapper;
        }

        public CriticalSectionOp GetCriticalSectionOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.CriticalSection;

            CriticalSectionOp wrapper;
            wrapper.InCriticalSection = op->InCriticalSection != 0;
            return wrapper;
        }

        public AddEntityOp GetAddEntityOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.AddEntity;

            AddEntityOp wrapper;
            wrapper.EntityId = op->EntityId;
            return wrapper;
        }

        public RemoveEntityOp GetRemoveEntityOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.RemoveEntity;

            RemoveEntityOp wrapper;
            wrapper.EntityId = op->EntityId;
            return wrapper;
        }

        [System.Obsolete(@"Use GetReserveEntityIdsResponseOp in conjunction with
                         Connection.SendReserveEntityIdsRequest instead.")]
        public ReserveEntityIdResponseOp GetReserveEntityIdResponseOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.ReserveEntityIdResponse;

            ReserveEntityIdResponseOp wrapper;
            wrapper.RequestId = op->RequestId;
            wrapper.StatusCode = (StatusCode) op->StatusCode;
            wrapper.Message = ApiInterop.FromUtf8Cstr(op->Message);
            wrapper.EntityId = op->StatusCode == (byte) StatusCode.Success ? op->EntityId : (EntityId?)null;
            return wrapper;
        }

        public ReserveEntityIdsResponseOp GetReserveEntityIdsResponseOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.ReserveEntityIdsResponse;

            ReserveEntityIdsResponseOp wrapper;
            wrapper.RequestId = op->RequestId;
            wrapper.StatusCode = (StatusCode) op->StatusCode;
            wrapper.Message = ApiInterop.FromUtf8Cstr(op->Message);
            wrapper.FirstEntityId = op->StatusCode == (byte) StatusCode.Success ? op->FirstEntityId : (EntityId?)null;
            wrapper.NumberOfEntityIds = (int) op->NumberOfEntityIds;
            return wrapper;
        }

        public CreateEntityResponseOp GetCreateEntityResponseOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.CreateEntityResponse;

            CreateEntityResponseOp wrapper;
            wrapper.RequestId = op->RequestId;
            wrapper.StatusCode = (StatusCode) op->StatusCode;
            wrapper.Message = ApiInterop.FromUtf8Cstr(op->Message);
            wrapper.EntityId = op->StatusCode == (byte) StatusCode.Success ? op->EntityId : (EntityId?)null;
            return wrapper;
        }

        public DeleteEntityResponseOp GetDeleteEntityResponseOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.DeleteEntityResponse;

            DeleteEntityResponseOp wrapper;
            wrapper.RequestId = op->RequestId;
            wrapper.EntityId = op->EntityId;
            wrapper.StatusCode = (StatusCode) op->StatusCode;
            wrapper.Message = ApiInterop.FromUtf8Cstr(op->Message);
            return wrapper;
        }

        public EntityQueryResponseOp GetEntityQueryResponseOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.EntityQueryResponse;

            EntityQueryResponseOp wrapper;
            wrapper.RequestId = op->RequestId;
            wrapper.StatusCode = (StatusCode) op->StatusCode;
            wrapper.Message = ApiInterop.FromUtf8Cstr(op->Message);
            wrapper.ResultCount = (int) op->ResultCount;
            wrapper.Result = new Dictionary<EntityId, Entity>();
            for (uint i = 0; op->Results != null && i < op->ResultCount; ++i)
            {
                var entity = new Entity();
                for (uint j = 0; j < op->Results[i].ComponentCount; ++j)
                {
                    var componentId = op->Results[i].Components[j].ComponentId;
                    entity.components.Add(componentId, new ComponentData(&op->Results[i].Components[j]));
                }
                wrapper.Result.Add(op->Results[i].EntityId, entity);
            }
            return wrapper;
        }

        public AddComponentOp GetAddComponentOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.AddComponent;

            AddComponentOp wrapper;
            wrapper.EntityId = op->EntityId;
            wrapper.Data = new ComponentData(&op->Data);
            return wrapper;
        }

        public RemoveComponentOp GetRemoveComponentOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.RemoveComponent;

            RemoveComponentOp wrapper;
            wrapper.EntityId = op->EntityId;
            wrapper.ComponentId = op->ComponentId;
            return wrapper;
        }

        public AuthorityChangeOp GetAuthorityChangeOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.AuthorityChange;

            AuthorityChangeOp wrapper;
            wrapper.EntityId = op->EntityId;
            wrapper.ComponentId = op->ComponentId;
            wrapper.Authority = (Authority) op->Authority;
            return wrapper;
        }

        public ComponentUpdateOp GetComponentUpdateOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.ComponentUpdate;

            ComponentUpdateOp wrapper;
            wrapper.EntityId = op->EntityId;
            wrapper.Update = new ComponentUpdate(&op->Update);
            return wrapper;
        }

        public CommandRequestOp GetCommandRequestOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.CommandRequest;

            CommandRequestOp wrapper;
            wrapper.RequestId = op->RequestId;
            wrapper.EntityId = op->EntityId;
            wrapper.TimeoutMillis = op->TimeoutMillis;
            wrapper.CallerWorkerId = ApiInterop.FromUtf8Cstr(op->CallerWorkerId);
            wrapper.CallerAttributeSet = new List<string>();
            for (uint i = 0; i < op->CallerAttributeSet.AttributeCount; ++i)
            {
                wrapper.CallerAttributeSet.Add(ApiInterop.FromUtf8Cstr(op->CallerAttributeSet.Attributes[i]));
            }
            wrapper.Request = new CommandRequest(&op->Request);

            return wrapper;
        }

        public CommandResponseOp GetCommandResponseOp(int opIndex)
        {
            var op = &GetOpInternal(opIndex)->OpUnion.CommandResponse;

            CommandResponseOp wrapper;
            wrapper.RequestId = op->RequestId;
            wrapper.EntityId = op->EntityId;
            wrapper.StatusCode = (StatusCode) op->StatusCode;
            wrapper.Message = ApiInterop.FromUtf8Cstr(op->Message);
            wrapper.CommandIndex = op->CommandIndex;
            wrapper.Response = new CommandResponse(&op->Response);

            return wrapper;
        }
    }
}
