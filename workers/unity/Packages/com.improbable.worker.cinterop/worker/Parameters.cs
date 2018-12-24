using System;
using System.Collections.Generic;
using ComponentId = System.UInt32;

namespace Improbable.Worker.CInterop
{
    public enum NetworkConnectionType
    {
        Tcp = 0,
        RakNet = 1,
        Kcp = 2,
    }

    public enum LocatorCredentialsType
    {
        LoginToken = 1,
        Steam = 2,
    }

    public enum LogLevel
    {
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5,
    }

    public class RakNetNetworkParameters
    {
        public uint HeartbeatTimeoutMillis = Defaults.RakNetHeartbeatTimeoutMillis;
    }

    public class TcpNetworkParameters
    {
        public byte MultiplexLevel = Defaults.TcpMultiplexLevel;
        public uint SendBufferSize = Defaults.TcpSendBufferSize;
        public uint ReceiveBufferSize = Defaults.TcpReceiveBufferSize;
        public bool NoDelay = Defaults.TcpNoDelay;
    }

    public class NetworkParameters
    {
        public bool UseExternalIp = Defaults.UseExternalIp;
        public NetworkConnectionType ConnectionType = NetworkConnectionType.Tcp;
        public RakNetNetworkParameters RakNet = new RakNetNetworkParameters();
        public TcpNetworkParameters Tcp = new TcpNetworkParameters();
        public Alpha.KcpNetworkParameters Kcp = new Alpha.KcpNetworkParameters();
        public System.UInt64 ConnectionTimeoutMillis = Defaults.ConnectionTimeoutMillis;
        public System.UInt32 DefaultCommandTimeoutMillis = Defaults.DefaultCommandTimeoutMillis;
    }

    public class ProtocolLoggingParameters
    {
        public string LogPrefix = Defaults.LogPrefix;
        public uint MaxLogFiles = Defaults.MaxLogFiles;
        public uint MaxLogFileSizeBytes = Defaults.MaxLogFileSizeBytes;
    }

    public class ThreadAffinityParameters
    {
        public System.UInt64 ReceiveThreadsAffinityMask = 0;
        public System.UInt64 SendThreadsAffinityMask = 0;
        public System.UInt64 TemporaryThreadsAffinityMask = 0;
    }

    public class LoginTokenCredentials
    {
        public string Token;
    }

    public class SteamCredentials
    {
        public string Ticket;
        public string DeploymentTag;
    }

    public class LocatorParameters
    {
        public string ProjectName;
        public LocatorCredentialsType CredentialsType;
        public LoginTokenCredentials LoginToken = new LoginTokenCredentials();
        public SteamCredentials Steam = new SteamCredentials();
        public ProtocolLoggingParameters Logging = new ProtocolLoggingParameters();
        public bool EnableLogging = false;
    }

    // Vtable function delegates.
    public unsafe delegate void CommandRequestFree(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);
    public unsafe delegate void CommandResponseFree(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);
    public unsafe delegate void ComponentDataFree(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);
    public unsafe delegate void ComponentUpdateFree(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);

    public unsafe delegate UIntPtr CommandRequestCopy(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);
    public unsafe delegate UIntPtr CommandResponseCopy(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);
    public unsafe delegate UIntPtr ComponentDataCopy(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);
    public unsafe delegate UIntPtr ComponentUpdateCopy(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);

    public unsafe delegate bool CommandRequestDeserialize(
        ComponentId componentId, UIntPtr userData, SchemaCommandRequest source, out UIntPtr handleOut);
    public unsafe delegate bool CommandResponseDeserialize(
        ComponentId componentId, UIntPtr userData, SchemaCommandResponse source, out UIntPtr handleOut);
    public unsafe delegate bool ComponentDataDeserialize(
        ComponentId componentId, UIntPtr userData, SchemaComponentData source, out UIntPtr handleOut);
    public unsafe delegate bool ComponentUpdateDeserialize(
        ComponentId componentId, UIntPtr userData, SchemaComponentUpdate source, out UIntPtr handleOut);

    public unsafe delegate SchemaCommandRequest? CommandRequestSerialize(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);
    public unsafe delegate SchemaCommandResponse? CommandResponseSerialize(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);
    public unsafe delegate SchemaComponentData? ComponentDataSerialize(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);
    public unsafe delegate SchemaComponentUpdate? ComponentUpdateSerialize(
        ComponentId componentId, UIntPtr userData, UIntPtr handle);

    /// <summary>
    /// Acts as a container of function callbacks which are called by the network threads when
    /// component data needs to be serialized or deserialized. You should ensure that no exceptions
    /// are thrown from any of these callbacks, as this can cause a crash in the native thread.
    /// </summary>
    public class ComponentVtable
    {
        public ComponentId ComponentId;
        public UIntPtr UserData;
        public CommandRequestFree CommandRequestFree;
        public CommandRequestCopy CommandRequestCopy;
        public CommandRequestDeserialize CommandRequestDeserialize;
        public CommandRequestSerialize CommandRequestSerialize;
        public CommandResponseFree CommandResponseFree;
        public CommandResponseCopy CommandResponseCopy;
        public CommandResponseDeserialize CommandResponseDeserialize;
        public CommandResponseSerialize CommandResponseSerialize;
        public ComponentDataFree ComponentDataFree;
        public ComponentDataCopy ComponentDataCopy;
        public ComponentDataDeserialize ComponentDataDeserialize;
        public ComponentDataSerialize ComponentDataSerialize;
        public ComponentUpdateFree ComponentUpdateFree;
        public ComponentUpdateCopy ComponentUpdateCopy;
        public ComponentUpdateDeserialize ComponentUpdateDeserialize;
        public ComponentUpdateSerialize ComponentUpdateSerialize;
    }

    public class ConnectionParameters
    {
        public string WorkerType;
        public NetworkParameters Network = new NetworkParameters();
        public uint SendQueueCapacity = Defaults.SendQueueCapacity;
        public uint ReceiveQueueCapacity = Defaults.ReceiveQueueCapacity;
        public uint LogMessageQueueCapacity = Defaults.LogMessageQueueCapacity;
        public uint BuiltInMetricsReportPeriodMillis = Defaults.BuiltInMetricsReportPeriodMillis;
        public ProtocolLoggingParameters ProtocolLogging = new ProtocolLoggingParameters();
        public bool EnableProtocolLoggingAtStartup = false;
        public ThreadAffinityParameters ThreadAffinity = new ThreadAffinityParameters();

        /// <summary>
        /// Vtables used by the connection when saving and reading snapshots. Separate behavior
        /// can be specified per component type, per handle type (data, update, command request,
        /// command response) and per direction (serialization, deserialization).
        ///</summary>
        /// <remarks>
        /// If no vtable is provided for given component, the default vtable will be used instead
        /// if it is not null, otherwise an error will occur when dealing with that component.
        /// </remarks>
        public Dictionary<uint, ComponentVtable> ComponentVtables = new Dictionary<uint, ComponentVtable>();

        /// <summary>
        /// The default vtable used by Connection when there's no component-specific vtable
        /// specified.
        /// </summary>
        public ComponentVtable DefaultComponentVtable = null;
    }

    public class SnapshotParameters
    {
        /// <summary>
        /// Vtables used by the connection when saving and reading snapshots. Separate behavior
        /// can be specified per component type, per handle type (data, update, command request,
        /// command response) and per direction (serialization, deserialization).
        ///</summary>
        /// <remarks>
        /// If no vtable is provided for given component, the default vtable will be used instead
        /// if it is not null, otherwise an error will occur when dealing with that component.
        /// </remarks>
        public Dictionary<uint, ComponentVtable> ComponentVtables = new Dictionary<uint, ComponentVtable>();

        /// <summary>
        /// The default vtable used by Connection when there's no component-specific vtable
        /// specified.
        /// </summary>
        public ComponentVtable DefaultComponentVtable = null;
    }
}

namespace Improbable.Worker.CInterop.Alpha
{
    public class PlayerIdentityCredentials
    {
        public string PlayerIdentityToken;
        public string LoginToken;
    }

    public class PlayerIdentityTokenRequest
    {
        public string DevelopmentAuthenticationTokenId;
        public string PlayerId;
        public uint? DurationSeconds = null;
        public string DisplayName;
        public string Metadata;
        public bool UseInsecureConnection = false;
    }

    public class LoginTokensRequest
    {
        public string PlayerIdentityToken;
        public uint? DurationSeconds = null;
        public string WorkerType;
        public bool UseInsecureConnection = false;
    }

    public class LocatorParameters
    {
        /// <summary>
        /// Parameters used to authenticate. Usually obtained from a game authentication server.
        /// </summary>
        public PlayerIdentityCredentials PlayerIdentity = new PlayerIdentityCredentials();

        /// <summary>
        /// Parameters for configuring logging.
        /// </summary>
        public ProtocolLoggingParameters Logging = new ProtocolLoggingParameters();

        /// <summary>
        /// Whether to use an insecure (non-TLS) connection for local development.
        /// </summary>
        public bool UseInsecureConnection = Defaults.LocatorUseInsecureConnection;

        /// <summary>
        /// Whether to enable logging for the Locator flow.
        /// </summary>
        public bool EnableLogging = Defaults.LocatorEnableLogging;
    }

    /// <summary>
    /// Parameters to configure erasure coding, a forward error correction technique which
    /// increases bandwidth usage but may improve latency on unreliable networks.
    /// </summary>
    public class ErasureCodecParameters
    {
        /// <summary>
        /// Number of consecutive packets to send before sending redundant recovery packets.
        /// </summary>
        public byte OriginalPacketCount = Defaults.ErasureCodecOriginalPacketCount;
        /// <summary>
        /// Number of redundant recovery packets to send for each group of consecutive original
        /// packets. These packets are used to recover up to the same number of lost original
        /// packets.
        /// </summary>
        public byte RecoveryPacketCount = Defaults.ErasureCodecRecoveryPacketCount;
        /// <summary>
        /// Number of batches that can be stored in memory, where a batch contains packets belonging to
        /// the same group of consecutive original packets and the corresponding recovery packets. Each
        /// batch contains up to OriginalPacketCount plus RecoveryPacketCount packets.
        /// </summary>
        public byte WindowSize = Defaults.ErasureCodecWindowSize;
    }

    /// <summary>
    /// Parameters to configure internal heartbeating which can detect unresponsive peers. If an
    /// unresponsive peer is detected, a Improbable.Worker.DisconnectOp will be enqueued in the op
    /// list.
    /// </summary>
    public class HeartbeatParameters
    {
        /// <summary>
        /// Minimum interval, in milliseconds, between which heartbeat messages are sent to the
        /// peer. A new heartbeat won't be sent before a response for the original heartbeat is
        /// received.
        /// </summary>
        public ulong IntervalMillis = Defaults.HeartbeatIntervalMillis;
        /// <summary>
        /// Time, in milliseconds, after which the peer will be deemed unresponsive.
        /// </summary>
        public ulong TimeoutMillis = Defaults.HeartbeatTimeoutMillis;
    }

    /// <summary>
    /// Parameters for configuring a KCP connection. Used by NetworkParameters.
    /// </summary>
    public class KcpNetworkParameters
    {
        /// <summary>
        /// Whether to enable fast retransmission, which causes retransmission delays to increase
        /// more slowly when retransmitting timed out packets multiple times.
        /// </summary>
        public bool FastRetransmission = Defaults.KcpFastRetransmission;
        /// <summary>
        /// Whether to enable early retransmission, which causes optimistic retransmission of
        /// earlier packets when acknowledgements are received for packets which were sent later,
        /// rather than waiting until the retransmission timeout has expired.
        /// </summary>
        public bool EarlyRetransmission = Defaults.KcpEarlyRetransmission;
        /// <summary>
        /// Whether to enable non-concessional flow control, which disables the usage of
        /// congestion windows (which are used to reduce packet loss across congested networks).
        /// Enabling non-concessional flow control can help optimize for low-latency delivery of
        /// small messages.
        /// </summary>
        public bool NonConcessionalFlowControl = Defaults.KcpNonConcessionalFlowControl;
        /// <summary>
        /// Number of multiplexed KCP streams.
        /// </summary>
        public uint MultiplexLevel = Defaults.KcpMultiplexLevel;
        /// <summary>
        /// Interval, in milliseconds, between which the KCP transport layer sends and receives
        /// packets waiting in its send and receive buffers respectively.
        /// </summary>
        public uint UpdateIntervalMillis = Defaults.KcpUpdateIntervalMillis;
        /// <summary>
        /// Hard limit on the minimum retransmission timeout. A packet will be resent if an
        /// acknowledgment has not been received from the peer within a time period known as the
        /// retransmission timeout. The retransmission timeout is calculated based on estimated
        /// round trip times to the remote peer, but it will never be set to a value lower than the
        /// minimum retransmission timeout. If you set this parameter to a value which is much
        /// higher than the average round trip time to a peer, it will likely result in packets not
        /// being resent as early as they could be, increasing latency for retransmitted packets.
        /// However, if you set this parameter to a value which is lower than the average round trip
        /// time (or ping), packets will be retransmitted even if they are not lost, which will
        /// cause unnecessary bandwidth overhead until round trip times are calculated. For more
        /// information on retransmission timeouts and their calculation, see
        /// https://tools.ietf.org/html/rfc6298. Note, however, that the RFC pertains to TCP, and
        /// therefore it focuses on avoiding unnecessary retransmissions rather than optimizing for
        /// latency.
        /// Set to zero to use default, which is lower when KcpNetworkParameters.FastRetransmission
        /// is enabled.
        /// </summary>
        public uint MinRtoMillis = Defaults.KcpMinRtoMillis;
        /// <summary>
        /// KCP flow-control window size, in number of packets. The window applies to sending
        /// across all streams. This means that messages being sent to the remote peer will be
        /// delayed if there are KcpNetworkParameters.WindowSize packets still waiting to be
        /// acknowledged by the peer. The same window also applies to receiving packets, but it only
        /// applies to each KCP stream independently. This limits the rate at which a peer sends
        /// data. However, it does not bound the total memory that may be used by KCP internally
        /// for buffering incoming messages in the way it does for outgoing messages.
        /// </summary>
        public uint WindowSize = Defaults.KcpWindowSize;
        /// <summary>
        /// Whether to enable the erasure codec.
        /// </summary>
        public bool EnableErasureCodec = Defaults.KcpEnableErasureCodec;
        /// <summary>
        /// Erasure codec parameters.
        /// </summary>
        public ErasureCodecParameters ErasureCodec = new ErasureCodecParameters();
        /// <summary>
        /// Heartbeat parameters.
        /// </summary>
        public HeartbeatParameters Heartbeat = new HeartbeatParameters();
    }
}
