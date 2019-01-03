using System.Diagnostics;
using System.IO;
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
        private const string rootApkPath = "build";
        private static string AbsoluteAppBuildPath => Path.GetFullPath(Path.Combine(Application.dataPath, Path.Combine("..", rootApkPath)));
        public static string IDeviceInstallerBinary => Common.DiscoverLocation("ideviceinstaller");
        public static string IDeviceDebugBinary => Common.DiscoverLocation("idevicedebug");

        private const string MenuLaunchAndroid = "SpatialOS/Launch mobile client/Android Device";
        private const string MenuLaunchiOSDevice = "SpatialOS/Launch mobile client/iOS Device";

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
                var apkPath = TryGetArchivePath(AbsoluteAppBuildPath, "*.apk");
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
                if (RedirectedProcess.Run(adbPath, "install", "-r", apkPath) != 0)
                {
                    Debug.LogError("Failed to install the apk on the device/emulator. If the application is already installed on your device/emulator, " +
                        "try uninstalling it before launching the mobile client.");
                    return;
                }

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

                EditorUtility.DisplayProgressBar("Launching Android Client", "Done", 1.0f);
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
                if (IDeviceInstallerBinary == string.Empty)
                {
                    Debug.LogError("Could not find ideviceinstaller tool. Please ensure it is installed.");
                    return;
                }

                if (IDeviceDebugBinary == string.Empty)
                {
                    Debug.LogError("Could not find idevicedebug tool. Please ensure libimobiledevice is installed.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Launching iOS Device Client", "Installing archive", 0.3f);

                // Find archive to install
                var ipaPath = TryGetArchivePath(AbsoluteAppBuildPath, "*.ipa");
                if (ipaPath == string.Empty)
                {
                    Debug.LogError($"Could not find a built out iOS archive in \"{AbsoluteAppBuildPath}\" to launch.");
                    return;
                }

                if (RedirectedProcess.Run(IDeviceInstallerBinary, "-i", ipaPath) != 0)
                {
                    Debug.LogError("Error while installing archive to the device. Please check the log for details about the error.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Launching iOS Device Client", "Launching Client", 0.9f);

                // Get chosen android package id and launch
                var bundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);

                // Optional arguments to be passed, same as standalone
                // Use this to pass through the local ip to connect to
                var runtimeIp = GdkToolsConfiguration.GetOrCreateInstance().RuntimeIp;
                var arguments = new StringBuilder();
                if (!string.IsNullOrEmpty(runtimeIp))
                {
                    arguments.Append($"-e SPATIALOS_ARGUMENTS=\"+{RuntimeConfigNames.ReceptionistHost} {runtimeIp}\"");
                }

                StartiOSDeviceProcess(arguments.ToString(), bundleId);

                EditorUtility.DisplayProgressBar("Launching iOS Device Client", "Done", 1.0f);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static string TryGetArchivePath(string rootPath, string archivePattern)
        {
            foreach (var file in Directory.GetFiles(rootPath, archivePattern, SearchOption.AllDirectories))
            {
                return file;
            }

            return string.Empty;
        }

        private static void StartiOSDeviceProcess(string arguments, string bundleId)
        {
            var processInfo = new ProcessStartInfo(IDeviceDebugBinary, $"{arguments} run {bundleId}")
            {
                CreateNoWindow = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = AbsoluteAppBuildPath
            };

            var processOutput = new StringBuilder();

            void OnReceived(object sender, DataReceivedEventArgs args)
            {
                if (string.IsNullOrEmpty(args.Data))
                {
                    return;
                }

                lock (processOutput)
                {
                    Debug.Log(args.Data.Trim());
                }
            }

            var process = Process.Start(processInfo);
            if (process == null)
            {
                Debug.LogError("Failed to start SpatialOS locally.");
                return;
            }

            process.OutputDataReceived += OnReceived;
            process.ErrorDataReceived += OnReceived;

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) =>
            {
                process.Dispose();
                process = null;
            };
        }
    }
}
