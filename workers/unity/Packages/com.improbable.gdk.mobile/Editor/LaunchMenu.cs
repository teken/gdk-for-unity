using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Improbable.Gdk.Core;
using Improbable.Gdk.Tools;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Improbable.Gdk.Mobile
{
    public static class LaunchMenu
    {
        private const string RootBuildPath = "build";
        private static string AbsoluteAppBuildPath => Path.GetFullPath(Path.Combine(Application.dataPath, Path.Combine("..", RootBuildPath)));
        private static string LibIDeviceInstallerBinary => Common.DiscoverLocation("ideviceinstaller");
        private static string LibIDeviceDebugBinary => Common.DiscoverLocation("idevicedebug");

        private const string MenuLaunchAndroid = "SpatialOS/Launch mobile client/Android Device";
        private const string MenuLaunchiOSDevice = "SpatialOS/Launch mobile client/iOS Device";
        private const string MenuLaunchiOSSimulator = "SpatialOS/Launch mobile client/iOS Simulator";

        [MenuItem(MenuLaunchAndroid, false, 73)]
        private static void LaunchAndroidClient()
        {
            try
            {
                // Find ADB tool
                var sdkRootPath = EditorPrefs.GetString("AndroidSdkRoot");
                if (string.IsNullOrEmpty(sdkRootPath))
                {
                    Debug.LogError($"Could not find Android SDK. Please set the SDK location in your editor preferences.");
                    return;
                }

                var adbPath = Path.Combine(sdkRootPath, "platform-tools", "adb");

                EditorUtility.DisplayProgressBar("Launching Android Client", "Installing APK", 0.3f);

                // Find apk to install
                var apkPath = Directory.GetFiles(AbsoluteAppBuildPath, "*.apk", SearchOption.AllDirectories).FirstOrDefault();
                if (apkPath == string.Empty)
                {
                    Debug.LogError($"Could not find a built out Android binary in \"{AbsoluteAppBuildPath}\" to launch.");
                    return;
                }

                // Ensure an android device/emulator is present
                if (RedirectedProcess.Run(adbPath, "get-state") != 0)
                {
                    Debug.LogError("No Android device/emulator detected.");
                    return;
                }

                // Install apk on connected phone / emulator
                RedirectedProcess.Run(adbPath, "install", "-r", apkPath);

                EditorUtility.DisplayProgressBar("Launching Android Client", "Launching Client", 0.9f);

                // Optional arguments to be passed, same as standalone
                // Use this to pass through the local ip to connect to
                var runtimeIp = GdkToolsConfiguration.GetOrCreateInstance().RuntimeIp;
                var arguments = new StringBuilder();
                if (!string.IsNullOrEmpty(runtimeIp))
                {
                    arguments.Append($"+{RuntimeConfigNames.ReceptionistHost} {runtimeIp}");
                }

                // Get chosen android package id and launch
                var bundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
                RedirectedProcess.Run(adbPath, "shell", "am", "start", "-S",
                    "-n", $"{bundleId}/com.unity3d.player.UnityPlayerActivity",
                    "-e", "\"arguments\"", $"\\\"{arguments.ToString()}\\\"");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem(MenuLaunchiOSDevice, false, 74)]
        private static void LaunchiOSDeviceClient()
        {
            try
            {
                // Ensure needed tools are installed
                if (string.IsNullOrEmpty(LibIDeviceInstallerBinary))
                {
                    Debug.LogError("Could not find ideviceinstaller tool. Please ensure it is installed. " +
                        "See https://github.com/libimobiledevice/ideviceinstaller fore more details.");
                    return;
                }

                if (string.IsNullOrEmpty(LibIDeviceDebugBinary))
                {
                    Debug.LogError("Could not find idevicedebug tool. Please ensure libimobiledevice is installed. " +
                        "See https://helpmanual.io/help/idevicedebug/ for more details.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Launching iOS Device Client", "Installing archive", 0.3f);

                // Find archive to install
                var ipaPath = Directory.GetFiles(AbsoluteAppBuildPath, "*.ipa", SearchOption.AllDirectories).FirstOrDefault();
                if (string.IsNullOrEmpty(ipaPath))
                {
                    Debug.LogError($"Could not find a built out iOS archive in \"{AbsoluteAppBuildPath}\" to launch.");
                    return;
                }

                if (RedirectedProcess.Run(LibIDeviceInstallerBinary, "-i", ipaPath) != 0)
                {
                    Debug.LogError("Error while installing archive to the device. Please check the log for details about the error.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Launching iOS Device Client", "Launching Client", 0.9f);

                // Get chosen ios package id and launch
                var bundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);

                // Optional arguments to be passed, same as standalone
                // Use this to pass through the local ip to connect to
                var runtimeIp = GdkToolsConfiguration.GetOrCreateInstance().RuntimeIp;
                var arguments = new StringBuilder();
                if (!string.IsNullOrEmpty(runtimeIp))
                {
                    arguments.Append($"-e SPATIALOS_ARGUMENTS=\"+{RuntimeConfigNames.ReceptionistHost} {runtimeIp}\"");
                }

                RedirectedProcess.RunWithImmediateOutput(LibIDeviceDebugBinary, arguments.ToString(), "run", bundleId);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem(MenuLaunchiOSSimulator, false, 75)]
        private static void LaunchiOSSimulatorClient()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Launching iOS Simulator Client", "Launching iOS simulator", 0.2f);

                // Open iOS simulator if it is not yet open
                RedirectedProcess.RunExtractOutput("xcode-select", out var xcodePath, "-p");
                if (string.IsNullOrEmpty(xcodePath))
                {
                    Debug.LogError("Couldn't run xcode-select. Please make sure XCode is installed");
                    return;
                }

                RedirectedProcess.Run("open", $"{xcodePath}/Applications/Simulator.app/");

                EditorUtility.DisplayProgressBar("Launching iOS Simulator Client", "Installing client app", 0.5f);

                // Find a built app to install
                var bundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
                var bundleName = bundleId.Split('.').LastOrDefault();
                var appPath = Directory.GetDirectories(AbsoluteAppBuildPath, $"{bundleName}.app", SearchOption.AllDirectories).FirstOrDefault();
                if (string.IsNullOrEmpty(appPath))
                {
                    Debug.LogError($"Could not find a built out iOS app in \"{AbsoluteAppBuildPath}\" to launch.");
                    return;
                }

                // iOS simulator might not have finished starting at this point, so we want to give it some time
                DateTime timeout = DateTime.Now.AddSeconds(30);
                while (RedirectedProcess.Run("xcrun", "simctl", "install", "booted", appPath) != 0)
                {
                    if (DateTime.Now > timeout)
                    {
                        Debug.LogError("Error while installing app to the simulator. Please check the log for details about the error.");
                    }

                    System.Threading.Thread.Sleep(1000);
                }

                EditorUtility.DisplayProgressBar("Launching iOS Simulator Client", "Launching Client app", 0.8f);

                // Optional arguments to be passed, same as standalone
                // Use this to pass through the local ip to connect to
                var runtimeIp = GdkToolsConfiguration.GetOrCreateInstance().RuntimeIp;
                var arguments = new StringBuilder();
                if (!string.IsNullOrEmpty(runtimeIp))
                {
                    arguments.Append($"+{RuntimeConfigNames.ReceptionistHost} {runtimeIp}");
                }

                var envVars = new Dictionary<string, string>
                {
                    { $"SIMCTL_CHILD_SPATIALOS_ARGUMENTS", arguments.ToString() }
                };
                if (RedirectedProcess.RunWithEnvVars("xcrun", envVars, "simctl", "launch", "booted", bundleId) != 0)
                {
                    Debug.LogError("Error while launching app on the simulator. Please check the log for details about the error.");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
