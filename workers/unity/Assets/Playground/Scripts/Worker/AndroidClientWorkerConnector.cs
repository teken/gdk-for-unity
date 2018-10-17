using Improbable.Gdk.Core;
using Improbable.Gdk.Mobile;
#if UNITY_ANDROID
using Improbable.Gdk.Mobile.Android;
#endif
using System;
using Improbable.Worker;
using Improbable.Worker.Core;
using UnityEngine;


namespace Playground
{
    public class AndroidClientWorkerConnector : MobileWorkerConnector, IMobileClient
    {
        public string IpAddress { get; set; }
        public ConnectionScreenController ConnectionScreenController { get; set; }

        [SerializeField] private GameObject level;

        // Fields needed to do cloud deployments
        [SerializeField] private bool useLocator;
        [SerializeField] private string projectName;
        [SerializeField] private string deploymentName;
        [SerializeField] private string loginToken;

        private GameObject levelInstance;

        protected override LocatorConfig GetLocatorConfig(string workerType)
        {
            return new LocatorConfig
            {
                LocatorParameters =
                {
                    CredentialsType = LocatorCredentialsType.LoginToken,
                    ProjectName = projectName,
                    LoginToken = new LoginTokenCredentials
                    {
                        Token = loginToken

                    }
                },
                WorkerType = workerType,
                WorkerId = CreateNewWorkerId(workerType)
            };
        }

        protected override bool ShouldUseLocator()
        {
            return useLocator;
        }

        protected override string SelectDeploymentName(DeploymentList deployments)
        {
            return deploymentName;
        }

        public async void TryConnect()
        {
            await Connect(WorkerUtils.AndroidClient, new ForwardingDispatcher()).ConfigureAwait(false);
        }

        protected override void HandleWorkerConnectionEstablished()
        {
            ConnectionScreenController.OnSuccess();

            WorkerUtils.AddClientSystems(Worker.World);
            if (level == null)
            {
                return;
            }

            levelInstance = Instantiate(level, transform);
            levelInstance.transform.SetParent(null);
        }

        protected override void HandleWorkerConnectionFailure()
        {
            ConnectionScreenController.OnConnectionFailed();
        }

        protected override string GetHostIp()
        {
#if UNITY_ANDROID
            if (Application.isMobilePlatform && DeviceInfo.IsAndroidStudioEmulator() && IpAddress.Equals(string.Empty))
            {
                return DeviceInfo.AndroidStudioEmulatorDefaultCallbackIp;
            }

            return IpAddress;
#else
            throw new PlatformNotSupportedException(
                "This method is only defined for the Android platform. Please check your build settings.");
#endif
        }
    }
}
