using Improbable.Worker.CInterop.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Improbable.Worker.CInterop
{
    public struct Deployment
    {
        public string DeploymentName;
        public string AssemblyName;
        public string Description;
        public uint UsersConnected;
        public uint UsersCapacity;
    }

    public struct DeploymentList
    {
        public List<Deployment> Deployments;
        public string Error;
    }

    public struct QueueStatus
    {
        public uint PositionInQueue;
        public string Error;
    }

    public sealed unsafe class Locator : IDisposable
    {
        private readonly LocatorHandle locator;

        public Locator(string hostname, LocatorParameters locatorParams)
        {
            CWorker.LocatorParameters parameters;
            switch (locatorParams.CredentialsType)
            {
                case LocatorCredentialsType.LoginToken:
                    parameters.CredentialsType = (byte)CWorker.LocatorCredentialsType.LoginToken;
                    break;
                case LocatorCredentialsType.Steam:
                    parameters.CredentialsType = (byte)CWorker.LocatorCredentialsType.Steam;
                    break;
            }

            parameters.EnableLogging = (byte)(locatorParams.EnableLogging  ? 1 : 0);
            parameters.Logging.MaxLogFiles = locatorParams.Logging.MaxLogFiles;
            parameters.Logging.MaxLogFileSizeBytes = locatorParams.Logging.MaxLogFileSizeBytes;

            fixed (byte* hostnameBytes = ApiInterop.ToUtf8Cstr(hostname))
            fixed (byte* projectNameBytes = ApiInterop.ToUtf8Cstr(locatorParams.ProjectName))
            fixed (byte* tokenBytes = ApiInterop.ToUtf8Cstr(locatorParams.LoginToken.Token))
            fixed (byte* ticketBytes = ApiInterop.ToUtf8Cstr(locatorParams.Steam.Ticket))
            fixed (byte* deploymentTagBytes = ApiInterop.ToUtf8Cstr(locatorParams.Steam.DeploymentTag))
            fixed (byte* logPrefixBytes = ApiInterop.ToUtf8Cstr(locatorParams.Logging.LogPrefix))
            {
                parameters.ProjectName = projectNameBytes;
                parameters.LoginToken.Token = tokenBytes;
                parameters.Steam.Ticket = ticketBytes;
                parameters.Steam.DeploymentTag = deploymentTagBytes;
                parameters.Logging.LogPrefix = logPrefixBytes;
                locator = CWorker.Locator_Create(hostnameBytes, &parameters);
            }
        }

        /// <inheritdoc cref="IDisposable"/>
        public void Dispose()
        {
            locator.Dispose();
        }

        public Future<DeploymentList?> GetDeploymentListAsync()
        {
            Contract.Requires<ObjectDisposedException>(!locator.IsClosed, GetType().Name);

            var future = CWorker.Locator_GetDeploymentListAsync(locator);
            return new Future<DeploymentList?>(future, ParameterConversion.DeploymentListFutureGet(future));
        }

        /// <remark>
        /// Ensure that the QueueStatus callback does not throw an exception. Otherwise, a fatal
        /// crash may occur.
        /// </remark>
        public Future<Connection> ConnectAsync(string deploymentName, ConnectionParameters connectionParams,
                                               Func<QueueStatus, bool> callback)
        {
            Contract.Requires<ObjectDisposedException>(!locator.IsClosed, this.GetType().Name);
            Contract.Requires<ArgumentNullException>(callback != null, typeof(Func<QueueStatus, bool>).FullName,
                "Locator queueing callback cannot be null.");

            var userCallbackHandle = new WrappedGcHandle(callback);

            ConnectionFutureHandle future = null;
            List<WrappedGcHandle> componentVtableHandles = null;
            ParameterConversion.ConvertConnectionParameters(connectionParams, (parameters, inComponentVtableHandles) =>
            {
                componentVtableHandles = inComponentVtableHandles;
                fixed (byte* deploymentNameBytes = ApiInterop.ToUtf8Cstr(deploymentName))
                {
                    future = CWorker.Locator_ConnectAsync(
                        locator,
                        deploymentNameBytes,
                        parameters,
                        (void*) userCallbackHandle.Get(),
                        CallbackThunkDelegates.QueueStatusCallbackThunkDelegate);
                }
            });

            return new ConnectAsyncFuture(future, componentVtableHandles, userCallbackHandle);
        }

        // In C#, creating a delegate from a method allocates memory and has its own lifetime.
        // To ensure that we don't run into any memory lifecycle issues, hide the methods and
        // only expose static delegates.
        private class CallbackThunkDelegates
        {
            public static readonly CWorker.QueueStatusCallback QueueStatusCallbackThunkDelegate =
                QueueStatusThunk;

            [MonoPInvokeCallback(typeof(CWorker.QueueStatusCallback))]
            private static byte QueueStatusThunk(void* userCallbackHandlePtr, CWorker.QueueStatus* queueStatus)
            {
                var userCallbackHandle = GCHandle.FromIntPtr((IntPtr) userCallbackHandlePtr);
                var callback = (Func<QueueStatus, bool>) userCallbackHandle.Target;

                QueueStatus wrapper;
                wrapper.PositionInQueue = queueStatus->PositionInQueue;
                wrapper.Error = queueStatus->Error == null ? null : ApiInterop.FromUtf8Cstr(queueStatus->Error);
                return (byte) (callback(wrapper) ? 1 : 0);
            }
        }

        private class ConnectAsyncFuture : Future<Connection>
        {
            // This class exists to ensure that the callback handle (constructor argument)
            // is not disposed by the GC until after the future.
            private WrappedGcHandle userCallbackHandle;

            public ConnectAsyncFuture(ConnectionFutureHandle future, List<WrappedGcHandle> componentVtableHandles, WrappedGcHandle userCallbackHandle) :
                base(future, ParameterConversion.ConnectionFutureGet(future, componentVtableHandles))
            {
                this.userCallbackHandle = userCallbackHandle;
            }

            public override void Dispose()
            {
                base.Dispose();
                userCallbackHandle.Dispose();
            }
        }
    }
}

namespace Improbable.Worker.CInterop.Alpha
{
    public sealed unsafe class Locator : IDisposable
    {
        private readonly Alpha_LocatorHandle _locator;

        public Locator(string hostname, LocatorParameters locatorParameters) : this(hostname, 0
            /* Use cloud port. */, locatorParameters) {}

        public Locator(string hostname, ushort port, LocatorParameters locatorParameters)
        {
            CWorker.Alpha_LocatorParameters parameters;
            parameters.EnableLogging = (byte) (locatorParameters.EnableLogging ? 1 : 0);
            parameters.Logging.MaxLogFiles = locatorParameters.Logging.MaxLogFiles;
            parameters.Logging.MaxLogFileSizeBytes = locatorParameters.Logging.MaxLogFileSizeBytes;
            parameters.UseInsecureConnection = (byte) (locatorParameters.UseInsecureConnection ? 1 : 0);

            fixed (byte* hostnameBytes = ApiInterop.ToUtf8Cstr(hostname))
            fixed (byte* loginTokenBytes = ApiInterop.ToUtf8Cstr(locatorParameters.PlayerIdentity.LoginToken))
            fixed (byte* playerIdentityTokenBytes = ApiInterop.ToUtf8Cstr(locatorParameters.PlayerIdentity.PlayerIdentityToken))
            fixed (byte* logPrefixBytes = ApiInterop.ToUtf8Cstr(locatorParameters.Logging.LogPrefix))
            {
                parameters.PlayerIdentity.PlayerIdentityToken = playerIdentityTokenBytes;
                parameters.PlayerIdentity.LoginToken = loginTokenBytes;
                parameters.Logging.LogPrefix = logPrefixBytes;
                _locator = CWorker.Alpha_Locator_Create(hostnameBytes, port, &parameters);
            }
        }

        public Future<Connection> ConnectAsync(ConnectionParameters connectionParams)
        {
            Contract.Requires<ObjectDisposedException>(!_locator.IsClosed, GetType().Name);

            ConnectionFutureHandle future = null;
            List<WrappedGcHandle> componentVtableHandles = null;
            ParameterConversion.ConvertConnectionParameters(connectionParams, (parameters, inComponentVtableHandles) =>
            {
                componentVtableHandles = inComponentVtableHandles;
                future = CWorker.Alpha_Locator_ConnectAsync(_locator, parameters);
            });
            
            return new Future<Connection>(future, ParameterConversion.ConnectionFutureGet(future, componentVtableHandles));
        }
        
        /// <inheritdoc cref="IDisposable"/>
        public void Dispose()
        {
            _locator.Dispose();
        }
    }
}