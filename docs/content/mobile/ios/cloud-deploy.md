# Connect to a cloud deployment

Before reading this document, make sure you are familiar with:

* [Setting up iOS Support for the GDK]({{urlRoot}}/content/mobile/ios/setup)
* [Ways to test your iOS client]({{urlRoot}}/content/mobile/ios/ways-to-test)
* [Development Authentication Flow](https://docs.improbable.io/reference/latest/shared/auth/development-authentication)
* [Creating workers with WorkerConnector](https://docs.improbable.io/unity/alpha/content/gameobject/creating-workers-with-workerconnector)

To connect your mobile application to a cloud deployment, you need to authenticate against our services.
This guide describes how to authenticate using the development authentication flow which we provide for early stages in development.
If you want to create your own authentication server, follow [this guide](https://docs.improbable.io/reference/latest/shared/auth/integrate-authentication-platform-sdk).

## Connecting your iOS device/simulator to a cloud deployment

1. [Build your server-workers.]({{urlRoot}}/content/build)
1. Upload your server workers using `spatial cloud upload`.
1. Start your cloud deployment using `spatial cloud launch`.
1. Tag your cloud deployment with the `dev_login` tag.
1. [Create a `DevelopmentAuthenticationToken`.](https://docs.improbable.io/reference/latest/shared/auth/development-authentication#developmentauthenticationtoken-maintenance)
1. Implement the [`MobileWorkerConnector`](https://github.com/spatialos/gdk-for-unity/blob/master/workers/unity/Packages/com.improbable.gdk.mobile/Worker/MobileWorkerConnector.cs) and add it to your Worker GameObject.
1. The `MobileWorkerConnector` provides a `DevelopmentAuthToken` field. Assign this to the authentication token that you created.
1. In the Unity Editor, navigate to **SpatialOS** > **Build for cloud**. Select your iOS worker, and wait for the build to complete.
1. Open the generated XCode project in `workers/unity/build/iOSClient@iOS`.
1. Code sign the project and build it.
1. Play the game on your iOS device/simulator.
