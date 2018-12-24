using System;
using System.Runtime.InteropServices;
using EntityId = System.Int64;
using ComponentId = System.UInt32;
using RequestId = System.UInt32;
using Uint64 = System.UInt64;
using Uint32 = System.UInt32;
using Uint16 = System.UInt16;
using Uint8 = System.Byte;
using Char = System.Byte;
using FunctionPtr = System.IntPtr;
using IntPtr = System.IntPtr;

// This file must match the C API (worker_sdk/c/include/improbable/c_worker.h) *exactly*!
namespace Improbable.Worker.CInterop.Internal
{
    internal unsafe class CWorker
    {
        public enum ComponentLoopback
        {
            None = 0,
            ShortCircuited = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LogMessage
        {
            public Uint8 LogLevel;
            public Char* LoggerName;
            public Char* Message;
            public EntityId* EntityId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GaugeMetric
        {
            public Char* Key;
            public double Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HistogramMetricBucket
        {
            public double UpperBound;
            public Uint32 Samples;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HistogramMetric
        {
            public Char* Key;
            public double Sum;
            public Uint32 BucketCount;
            public HistogramMetricBucket* Buckets;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Metrics
        {
            public double* Load;
            public Uint32 GaugeMetricCount;
            public GaugeMetric* GaugeMetrics;
            public Uint32 HistogramMetricCount;
            public HistogramMetric* HistogramMetrics;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CommandRequestFree(
            ComponentId component_id, void* user_data, void* handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CommandResponseFree(
            ComponentId component_id, void* user_data, void* handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ComponentDataFree(
            ComponentId component_id, void* user_data, void* handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ComponentUpdateFree(
            ComponentId component_id, void* user_data, void* handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* CommandRequestCopy(
            ComponentId component_id, void* user_data, void* handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* CommandResponseCopy(
            ComponentId component_id, void* user_data, void* handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* ComponentDataCopy(
            ComponentId component_id, void* user_data, void* handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* ComponentUpdateCopy(
            ComponentId component_id, void* user_data, void* handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Uint8 CommandRequestDeserialize(
            ComponentId component_id, void* user_data, CSchema.CommandRequest* source, void** handle_out);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Uint8 CommandResponseDeserialize(
            ComponentId component_id, void* user_data, CSchema.CommandResponse* source, void** handle_out);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Uint8 ComponentDataDeserialize(
            ComponentId component_id, void* user_data, CSchema.ComponentData* source, void** handle_out);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Uint8 ComponentUpdateDeserialize(
            ComponentId component_id, void* user_data, CSchema.ComponentUpdate* source, void** handle_out);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CommandRequestSerialize(
            ComponentId component_id, void* user_data, void* handle, CSchema.CommandRequest** target_out);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CommandResponseSerialize(
            ComponentId component_id, void* user_data, void* handle, CSchema.CommandResponse** target_out);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ComponentDataSerialize(
            ComponentId component_id, void* user_data, void* handle, CSchema.ComponentData** target_out);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ComponentUpdateSerialize(
            ComponentId component_id, void* user_data, void* handle, CSchema.ComponentUpdate** target_out);

        [StructLayout(LayoutKind.Sequential)]
        public struct ComponentVtable
        {
            public ComponentId ComponentId;
            public void* UserData;
            public FunctionPtr CommandRequestFree;
            public FunctionPtr CommandRequestCopy;
            public FunctionPtr CommandRequestDeserialize;
            public FunctionPtr CommandRequestSerialize;
            public FunctionPtr CommandResponseFree;
            public FunctionPtr CommandResponseCopy;
            public FunctionPtr CommandResponseDeserialize;
            public FunctionPtr CommandResponseSerialize;
            public FunctionPtr ComponentDataFree;
            public FunctionPtr ComponentDataCopy;
            public FunctionPtr ComponentDataDeserialize;
            public FunctionPtr ComponentDataSerialize;
            public FunctionPtr ComponentUpdateFree;
            public FunctionPtr ComponentUpdateCopy;
            public FunctionPtr ComponentUpdateDeserialize;
            public FunctionPtr ComponentUpdateSerialize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CommandRequest
        {
            public void* Reserved;
            public ComponentId ComponentId;
            public CSchema.CommandRequest* SchemaType;
            public void* UserHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CommandResponse
        {
            public void* Reserved;
            public ComponentId ComponentId;
            public CSchema.CommandResponse* SchemaType;
            public void* UserHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ComponentData
        {
            public void* Reserved;
            public ComponentId ComponentId;
            public CSchema.ComponentData* SchemaType;
            public void* UserHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ComponentUpdate
        {
            public void* Reserved;
            public ComponentId ComponentId;
            public CSchema.ComponentUpdate* SchemaType;
            public void* UserHandle;
        }

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_AcquireCommandRequest")]
        public static extern CommandRequest* AcquireCommandRequest(CommandRequest* request);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_AcquireCommandResponse")]
        public static extern CommandResponse* AcquireCommandResponse(CommandResponse* response);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_AcquireComponentData")]
        public static extern ComponentData* AcquireComponentData(ComponentData* data);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_AcquireComponentUpdate")]
        public static extern ComponentUpdate* AcquireComponentUpdate(ComponentUpdate* update);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_ReleaseCommandRequest")]
        public static extern void ReleaseCommandRequest(CommandRequest* request);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_ReleaseCommandResponse")]
        public static extern void ReleaseCommandResponse(CommandResponse* response);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_ReleaseComponentData")]
        public static extern void ReleaseComponentData(ComponentData* data);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_ReleaseComponentUpdate")]
        public static extern void ReleaseComponentUpdate(ComponentUpdate* update);

        [StructLayout(LayoutKind.Sequential)]
        public struct Entity
        {
            public EntityId EntityId;
            public Uint32 ComponentCount;
            public ComponentData* Components;
        }

        public enum ConstraintType
        {
            EntityId = 1,
            Component = 2,
            Sphere = 3,
            And = 4,
            Or = 5,
            Not = 6,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EntityIdConstraint
        {
            public EntityId EntityId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ComponentConstraint
        {
            public Uint32 ComponentId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SphereConstraint
        {
            public double X;
            public double Y;
            public double Z;
            public double Radius;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AndConstraint
        {
            public Uint32 ConstraintCount;
            public Constraint* Constraints;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OrConstraint
        {
            public Uint32 ConstraintCount;
            public Constraint* Constraints;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NotConstraint
        {
            public Constraint* Constraint;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Constraint
        {
            public Uint8 ConstraintType;
            public Union ConstraintTypeUnion;

            [StructLayout(LayoutKind.Explicit)]
            public struct Union
            {
                [FieldOffset(0)]
                public EntityIdConstraint EntityIdConstraint;
                [FieldOffset(0)]
                public ComponentConstraint ComponentConstraint;
                [FieldOffset(0)]
                public SphereConstraint SphereConstraint;
                [FieldOffset(0)]
                public AndConstraint AndConstraint;
                [FieldOffset(0)]
                public OrConstraint OrConstraint;
                [FieldOffset(0)]
                public NotConstraint NotConstraint;
            }
        }

        public enum ResultType
        {
            Count = 1,
            Snapshot = 2,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EntityQuery
        {
            public Constraint Constraint;
            public Uint8 ResultType;
            public Uint32 SnapshotResultTypeComponentIdCount;
            public ComponentId* SnapshotResultTypeComponentIds;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct InterestOverride
        {
            public Uint32 ComponentId;
            public Uint8 IsInterested;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WorkerAttributes
        {
            public Uint32 AttributeCount;
            public Char** Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DisconnectOp
        {
            public Uint8 ConnectionStatusCode;
            public Char* Reason;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FlagUpdateOp
        {
            public Char* Name;
            public Char* Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LogMessageOp
        {
            public Uint8 LogLevel;
            public Char* Message;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MetricsOp
        {
            public Metrics Metrics;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CriticalSectionOp
        {
            public Uint8 InCriticalSection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AddEntityOp
        {
            public EntityId EntityId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RemoveEntityOp
        {
            public EntityId EntityId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ReserveEntityIdResponseOp
        {
            public RequestId RequestId;
            public Uint8 StatusCode;
            public Char* Message;
            public EntityId EntityId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ReserveEntityIdsResponseOp
        {
            public RequestId RequestId;
            public Uint8 StatusCode;
            public Char* Message;
            public EntityId FirstEntityId;
            public Uint32 NumberOfEntityIds;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CreateEntityResponseOp
        {
            public RequestId RequestId;
            public Uint8 StatusCode;
            public Char* Message;
            public EntityId EntityId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeleteEntityResponseOp
        {
            public RequestId RequestId;
            public EntityId EntityId;
            public Uint8 StatusCode;
            public Char* Message;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EntityQueryResponseOp
        {
            public RequestId RequestId;
            public Uint8 StatusCode;
            public Char* Message;
            public Uint32 ResultCount;
            public Entity* Results;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AddComponentOp
        {
            public EntityId EntityId;
            public ComponentData Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RemoveComponentOp
        {
            public EntityId EntityId;
            public ComponentId ComponentId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AuthorityChangeOp
        {
            public EntityId EntityId;
            public ComponentId ComponentId;
            public Uint8 Authority;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ComponentUpdateOp
        {
            public EntityId EntityId;
            public ComponentUpdate Update;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CommandRequestOp
        {
            public RequestId RequestId;
            public EntityId EntityId;
            public Uint32 TimeoutMillis;
            public Char* CallerWorkerId;
            public WorkerAttributes CallerAttributeSet;
            public CommandRequest Request;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CommandResponseOp
        {
            public RequestId RequestId;
            public EntityId EntityId;
            public Uint8 StatusCode;
            public Char* Message;
            public CommandResponse Response;
            public Uint32 CommandIndex;
        }

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

        [StructLayout(LayoutKind.Sequential)]
        public struct Op
        {
            public Uint8 OpType;
            public Union OpUnion;

            [StructLayout(LayoutKind.Explicit)]
            public struct Union
            {
                [FieldOffset(0)]
                public DisconnectOp Disconnect;
                [FieldOffset(0)]
                public FlagUpdateOp FlagUpdate;
                [FieldOffset(0)]
                public LogMessageOp LogMessage;
                [FieldOffset(0)]
                public MetricsOp Metrics;
                [FieldOffset(0)]
                public CriticalSectionOp CriticalSection;
                [FieldOffset(0)]
                public AddEntityOp AddEntity;
                [FieldOffset(0)]
                public RemoveEntityOp RemoveEntity;
                [FieldOffset(0)]
                public ReserveEntityIdResponseOp ReserveEntityIdResponse;
                [FieldOffset(0)]
                public ReserveEntityIdsResponseOp ReserveEntityIdsResponse;
                [FieldOffset(0)]
                public CreateEntityResponseOp CreateEntityResponse;
                [FieldOffset(0)]
                public DeleteEntityResponseOp DeleteEntityResponse;
                [FieldOffset(0)]
                public EntityQueryResponseOp EntityQueryResponse;
                [FieldOffset(0)]
                public AddComponentOp AddComponent;
                [FieldOffset(0)]
                public RemoveComponentOp RemoveComponent;
                [FieldOffset(0)]
                public AuthorityChangeOp AuthorityChange;
                [FieldOffset(0)]
                public ComponentUpdateOp ComponentUpdate;
                [FieldOffset(0)]
                public CommandRequestOp CommandRequest;
                [FieldOffset(0)]
                public CommandResponseOp CommandResponse;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OpList
        {
            public Op* Ops;
            public Uint32 OpCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RakNetNetworkParameters
        {
            public Uint32 HeartbeatTimeoutMillis;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TcpNetworkParameters
        {
            public Uint8 MultiplexLevel;
            public Uint32 SendBufferSize;
            public Uint32 ReceiveBufferSize;
            public Uint8 NoDelay;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_ErasureCodecParameters
        {
            public Uint8 OriginalPacketCount;
            public Uint8 RecoveryPacketCount;
            public Uint8 WindowSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_HeartbeatParameters
        {
            public Uint64 IntervalMillis;
            public Uint64 TimeoutMillis;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_KcpNetworkParameters
        {
            public Uint8 FastRetransmission;
            public Uint8 EarlyRetransmission;
            public Uint8 NonConcessionalFlowControl;
            public Uint32 MultiplexLevel;
            public Uint32 UpdateIntervalMillis;
            public Uint32 MinRtoMillis;
            public Uint32 WindowSize;
            public Uint8 EnableErasureCodec;
            public Alpha_ErasureCodecParameters ErasureCodec;
            public Alpha_HeartbeatParameters Heartbeat;
        }

        public enum NetworkConnectionType
        {
            Tcp = 0,
            RakNet = 1,
            Kcp = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NetworkParameters
        {
            public Uint8 UseExternalIp;
            public Uint8 ConnectionType;
            public RakNetNetworkParameters RakNet;
            public TcpNetworkParameters Tcp;
            public Alpha_KcpNetworkParameters Kcp;
            public Uint64 ConnectionTimeoutMillis;
            public Uint32 DefaultCommandTimeoutMillis;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProtocolLoggingParameters
        {
            public Char* LogPrefix;
            public Uint32 MaxLogFiles;
            public Uint32 MaxLogFileSizeBytes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ThreadAffinityParameters
        {
            public Uint64 ReceiveThreadsAffinityMask;
            public Uint64 SendThreadsAffinityMask;
            public Uint64 TemporaryThreadsAffinityMask;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ConnectionParameters
        {
            public Char* WorkerType;
            public NetworkParameters Network;
            public Uint32 SendQueueCapacity;
            public Uint32 ReceiveQueueCapacity;
            public Uint32 LogMessageQueueCapacity;
            public Uint32 BuiltInMetricsReportPeriodMillis;
            public ProtocolLoggingParameters ProtocolLogging;
            public Uint8 EnableProtocolLoggingAtStartup;
            public ThreadAffinityParameters ThreadAffinity;
            public Uint32 ComponentVtableCount;
            public ComponentVtable* ComponentVtables;
            public ComponentVtable* DefaultComponentVtable;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LoginTokenCredentials
        {
            public Char* Token;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SteamCredentials
        {
            public Char* Ticket;
            public Char* DeploymentTag;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_PlayerIdentityCredentials
        {
            public Char* PlayerIdentityToken;
            public Char* LoginToken;
        }

        public enum LocatorCredentialsType
        {
            LoginToken = 1,
            Steam = 2,
            PlayerIdentityCredentials = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LocatorParameters
        {
            public Char* ProjectName;
            public Uint8 CredentialsType;
            public LoginTokenCredentials LoginToken;
            public SteamCredentials Steam;
            public ProtocolLoggingParameters Logging;
            public Uint8 EnableLogging;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_LocatorParameters
        {
            public Alpha_PlayerIdentityCredentials PlayerIdentity;
            public Uint8 UseInsecureConnection;
            public ProtocolLoggingParameters Logging;
            public Uint8 EnableLogging;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Deployment
        {
            public Char* DeploymentName;
            public Char* AssemblyName;
            public Char* Description;
            public uint UsersConnected;
            public uint UsersCapacity;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeploymentList
        {
            public Uint32 DeploymentCount;
            public Deployment* Deployments;
            public Char* Error;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct QueueStatus
        {
            public Uint32 PositionInQueue;
            public Char* Error;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_UpdateParameters
        {
            public Uint8 Loopback;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CommandParameters
        {
            public Uint8 AllowShortCircuiting;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_PlayerIndentityTokenRequest
        {
            public Char* DevelopmentAuthenticationTokenId;
            public Char* PlayerId;
            public Uint32* DurationSeconds;
            public Char* DisplayName;
            public Char* Metadata;
            public Uint8 UseInsecureConnection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_PlayerIdentityTokenResponse
        {
            public Char* PlayerIdentityToken;
            public Uint8 ConnectionStatusCode;
            public Char* Error;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_LoginTokensRequest
        {
            public Char* PlayerIdentityToken;
            public Char* WorkerType;
            public Uint32* DurationSeconds;
            public Uint8 UseInsecureConnection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_LoginTokenDetails
        {
            public Char* DeploymentId;
            public Char* DeploymentName;
            public Uint32 TagCount;
            public Char** Tags;
            public Char* LoginToken;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Alpha_LoginTokensResponse
        {
            public Uint32 LoginTokenCount;
            public Alpha_LoginTokenDetails* LoginToken;
            public Uint8 ConnectionStatusCode;
            public Char* Error;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DeploymentListCallback(void* user_data, DeploymentList* deployment_list);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Uint8 QueueStatusCallback(void* user_data, QueueStatus* queue_status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetFlagCallback(void* user_data, Char* value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Alpha_PlayerIdentityTokenResponseCallback(void* user_data, Alpha_PlayerIdentityTokenResponse* pit);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Alpha_LoginTokensResponseCallback(void* user_data, Alpha_LoginTokensResponse* pit);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_DefaultConnectionParameters")]
        public static extern ConnectionParameters DefaultConnectionParameters();

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Locator_Create")]
        public static extern LocatorHandle Locator_Create(Char* hostname, LocatorParameters* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Locator_Destroy")]
        public static extern void Locator_Destroy(IntPtr locator);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_Locator_Create")]
        public static extern Alpha_LocatorHandle Alpha_Locator_Create(Char* hostname, Uint16 port, Alpha_LocatorParameters* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_Locator_Destroy")]
        public static extern void Alpha_Locator_Destroy(IntPtr locator);
    
        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_CreateDevelopmentPlayerIdentityTokenAsync")]
        public static extern Alpha_PlayerIdentityTokenResponseFutureHandle
            Alpha_CreateDevelopmentPlayerIdentityTokenAsync(Char* hostname, Uint16 port,
                        Alpha_PlayerIndentityTokenRequest* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_PlayerIdentityTokenResponseFuture_Destroy")]
        public static extern void Alpha_PlayerIdentityTokenResponseFuture_Destroy(IntPtr future);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_PlayerIdentityTokenResponseFuture_Get")]
        public static extern Alpha_PlayerIdentityTokenResponse*
            Alpha_PlayerIdentityTokenResponseFuture_Get(
                        Alpha_PlayerIdentityTokenResponseFutureHandle future, Uint32* timeout_millis,
                        void* data, Alpha_PlayerIdentityTokenResponseCallback callback);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_CreateDevelopmentLoginTokensAsync")]
        public static extern Alpha_LoginTokensResponseFutureHandle
            Alpha_CreateDevelopmentLoginTokensAsync(Char* hostname, Uint16 port,
                        Alpha_LoginTokensRequest* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_LoginTokensResponseFuture_Destroy")]
         public static extern void Alpha_LoginTokensResponseFuture_Destroy(IntPtr future);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_LoginTokensResponseFuture_Get")]
        public static extern Alpha_LoginTokensResponse* Alpha_LoginTokensResponseFuture_Get(
                    Alpha_LoginTokensResponseFutureHandle future, Uint32* timeout_millis,
                    void* data, Alpha_LoginTokensResponseCallback callback);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Locator_GetDeploymentListAsync")]
        public static extern DeploymentListFutureHandle Locator_GetDeploymentListAsync(LocatorHandle locator);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Locator_ConnectAsync")]
        public static extern ConnectionFutureHandle Locator_ConnectAsync(
            LocatorHandle locator, Char* deployment_name,
            ConnectionParameters* parameters, void* data, QueueStatusCallback callback);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_Locator_ConnectAsync")]
        public static extern ConnectionFutureHandle Alpha_Locator_ConnectAsync(
            Alpha_LocatorHandle locator, ConnectionParameters* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_ConnectAsync")]
        public static extern ConnectionFutureHandle ConnectAsync(Char* receptionist_hostname,
                                                                 Uint16 receptionist_port,
                                                                 Char* worker_id,
                                                                 ConnectionParameters* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Test_ConnectDirectAsync")]
        public static extern ConnectionFutureHandle ConnectDirectAsync(Char* bridge_hostname,
                                                                       Uint16 bridge_port,
                                                                       Char* bridge_session_token,
                                                                       Char* deployment_certificate,
                                                                       Uint64 deployment_certificate_length,
                                                                       ConnectionParameters* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_DeploymentListFuture_Destroy")]
        public static extern void DeploymentListFuture_Destroy(IntPtr future);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_DeploymentListFuture_Get")]
        public static extern void DeploymentListFuture_Get(
            DeploymentListFutureHandle future, Uint32* timeout_millis,
            void* data, DeploymentListCallback callback);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_ConnectionFuture_Destroy")]
        public static extern void ConnectionFuture_Destroy(IntPtr future);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_ConnectionFuture_Get")]
        public static extern ConnectionHandle ConnectionFuture_Get(
            ConnectionFutureHandle future, Uint32* timeout_millis);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_Destroy")]
        public static extern void Connection_Destroy(IntPtr connection);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendLogMessage")]
        public static extern void Connection_SendLogMessage(ConnectionHandle connection, LogMessage* log_message);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendMetrics")]
        public static extern void Connection_SendMetrics(ConnectionHandle connection, Metrics* metrics);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendReserveEntityIdRequest")]
        public static extern RequestId Connection_SendReserveEntityIdRequest(
            ConnectionHandle connection, Uint32* timeout_millis);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendReserveEntityIdsRequest")]
        public static extern RequestId Connection_SendReserveEntityIdsRequest(
            ConnectionHandle connection, Uint32 number_of_entity_ids, Uint32* timeout_millis);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendCreateEntityRequest")]
        public static extern RequestId Connection_SendCreateEntityRequest(
            ConnectionHandle connection, Uint32 component_count, ComponentData* components,
            EntityId* entity_id, Uint32* timeout_millis);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendDeleteEntityRequest")]
        public static extern RequestId Connection_SendDeleteEntityRequest(
            ConnectionHandle connection, EntityId entity_id, Uint32* timeout_millis);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendEntityQueryRequest")]
        public static extern RequestId Connection_SendEntityQueryRequest(
            ConnectionHandle connection, EntityQuery* entity_query, Uint32* timeout_millis);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendComponentUpdate")]
        public static extern void Connection_SendComponentUpdate(
            ConnectionHandle connection, EntityId entity_id, ComponentUpdate* component_update);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Alpha_Connection_SendComponentUpdate")]
        public static extern void Alpha_Connection_SendComponentUpdate(
            ConnectionHandle connection, EntityId entity_id, ComponentUpdate* component_update,
            Alpha_UpdateParameters* update_parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendCommandRequest")]
        public static extern RequestId Connection_SendCommandRequest(
            ConnectionHandle connection, EntityId entity_id,
            CommandRequest* request, Uint32 command_id, Uint32* timeout_millis, CommandParameters* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendCommandResponse")]
        public static extern void Connection_SendCommandResponse(
            ConnectionHandle connection, RequestId request_id, CommandResponse* response);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendCommandFailure")]
        public static extern void Connection_SendCommandFailure(
            ConnectionHandle connection, RequestId request_id, Char* message);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendComponentInterest")]
        public static extern void Connection_SendComponentInterest(
            ConnectionHandle connection, EntityId entity_id,
            InterestOverride* interest_overrides, Uint32 interest_override_count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SendAuthorityLossImminentAcknowledgement")]
        public static extern void Connection_SendAuthorityLossImminentAcknowledgement(
            ConnectionHandle connection, EntityId entity_id, ComponentId component_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_SetProtocolLoggingEnabled")]
        public static extern void Connection_SetProtocolLoggingEnabled(
            ConnectionHandle connection, Uint8 enabled);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_IsConnected")]
        public static extern bool Connection_IsConnected(ConnectionHandle connection);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_GetConnectionStatusCode")]
        public static extern Uint8 Connection_GetConnectionStatusCode(ConnectionHandle connection);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_GetConnectionStatusDetailString")]
        public static extern Char* Connection_GetConnectionStatusDetailString(ConnectionHandle connection);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_GetWorkerId")]
        public static extern Char* Connection_GetWorkerId(ConnectionHandle connection);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_GetWorkerAttributes")]
        public static extern WorkerAttributes* Connection_GetWorkerAttributes(ConnectionHandle connection);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_GetFlag")]
        public static extern void Connection_GetFlag(
            ConnectionHandle connection, Char* name, void* user_data, GetFlagCallback callback);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_Connection_GetOpList")]
        public static extern OpListHandle Connection_GetOpList(ConnectionHandle connection, Uint32 timeout_millis);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_OpList_Destroy")]
        public static extern void OpList_Destroy(IntPtr op_list);

        [StructLayout(LayoutKind.Sequential)]
        public struct SnapshotParameters
        {
            public Uint32 ComponentVtableCount;
            public ComponentVtable* ComponentVtables;
            public ComponentVtable* DefaultComponentVtable;
        }

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_SnapshotInputStream_Create")]
        public static extern SnapshotInputStreamHandle SnapshotInputStream_Create(
            Char* filename, SnapshotParameters* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_SnapshotInputStream_Destroy")]
        public static extern void SnapshotInputStream_Destroy(
            IntPtr input_stream);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                    EntryPoint = "Worker_SnapshotInputStream_HasNext")]
        public static extern Uint8 SnapshotInputStream_HasNext(
            SnapshotInputStreamHandle input_stream);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_SnapshotInputStream_ReadEntity")]
        public static extern Entity* SnapshotInputStream_ReadEntity(
            SnapshotInputStreamHandle input_stream);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_SnapshotInputStream_GetError")]
        public static extern Char* SnapshotInputStream_GetError(
            SnapshotInputStreamHandle input_stream);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_SnapshotOutputStream_Create")]
        public static extern SnapshotOutputStreamHandle SnapshotOutputStream_Create(
            Char* filename, SnapshotParameters* parameters);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_SnapshotOutputStream_Destroy")]
        public static extern void SnapshotOutputStream_Destroy(
            IntPtr output_stream);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_SnapshotOutputStream_WriteEntity")]
        public static extern Uint8 SnapshotOutputStream_WriteEntity(
            SnapshotOutputStreamHandle output_stream, Entity* entity);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Worker_SnapshotOutputStream_GetError")]
        public static extern Char* SnapshotOutputStream_GetError(
            SnapshotOutputStreamHandle output_stream);

        /*
         * Utility functions used for working with raw byte arrays.
         */
        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "CoreSdk_Internal_Memcmp")]
        public static unsafe extern int Memcmp(byte* a, byte* b, UIntPtr count);
        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "CoreSdk_Internal_Memcpy")]
        public static unsafe extern IntPtr Memcpy(byte* dst, byte* src, UIntPtr count);
    }
}
