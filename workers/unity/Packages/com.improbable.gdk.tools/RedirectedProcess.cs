using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Improbable.Gdk.Tools.MiniJSON;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Improbable.Gdk.Tools
{
    /// <summary>
    ///     Runs a windowless process and waits for it to return.
    /// </summary>
    public static class RedirectedProcess
    {
        private static string AppDataPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        /// <summary>
        ///     Runs redirected process in Application root directory with its stdout/stderr redirected to the
        ///     Unity console as a single debug print at the end.
        /// </summary>
        /// <param name="command">The filename to run.</param>
        /// <param name="arguments">Parameters that will be passed to the command.</param>
        /// <returns>The exit code.</returns>
        public static int Run(string command, params string[] arguments)
        {
            return RunIn(AppDataPath, command, arguments);
        }

        /// <summary>
        ///     Runs redirected process in specified output directory with its stdout/stderr redirected to the
        ///     Unity console as a single debug print at the end.
        /// </summary>
        /// <param name="workingDirectory">The directory to run the filename from.</param>
        /// <param name="command">The filename to run.</param>
        /// <param name="arguments">Parameters that will be passed to the command.</param>
        /// <returns>The exit code.</returns>
        public static int RunIn(string workingDirectory, string command, params string[] arguments)
        {
            var processOutput = new StringBuilder();

            void ProcessOutput(string output)
            {
                lock (processOutput)
                {
                    processOutput.AppendLine(ProcessSpatialOutput(output));
                }
            }

            var exitCode = RunRedirectedProcess(workingDirectory, command, null, ProcessOutput, arguments);

            var trimmedOutput = processOutput.ToString().Trim();

            if (string.IsNullOrEmpty(trimmedOutput))
            {
                return exitCode;
            }

            if (exitCode == 0)
            {
                Debug.Log(trimmedOutput);
            }
            else
            {
                Debug.LogError(trimmedOutput);
            }

            return exitCode;
        }

        /// <summary>
        ///     Runs redirected process in Application root directory and extracts returns output in output string.
        /// </summary>
        /// <param name="command">The filename to run.</param>
        /// <param name="output">String containing output of the process.</param>
        /// <param name="arguments">Parameters that will be passed to the command.</param>
        /// <returns>The exit code.</returns>
        public static int RunExtractOutput(string command, out string output, params string[] arguments)
        {
            var processOutput = new StringBuilder();

            void ProcessOutput(string outputLine)
            {
                lock (processOutput)
                {
                    processOutput.AppendLine(outputLine);
                }
            }

            var exitCode = RunRedirectedProcess(AppDataPath, command, null, ProcessOutput, arguments);

            output = processOutput.ToString().Trim();
            return exitCode;
        }

        /// <summary>
        ///     Runs redirected process in Application root directory with its stdout/stderr redirected to the
        ///     Unity console immediately.
        /// </summary>
        /// <param name="command">The filename to run.</param>
        /// <param name="arguments">Parameters that will be passed to the command.</param>
        /// <returns>The exit code.</returns>
        public static int RunWithImmediateOutput(string command, params string[] arguments)
        {
            return RunWithEnvVars(command, null, arguments);
        }

        /// <summary>
        ///     Runs redirected process in Application root directory with given environment variables.
        ///     Stdout/stderr are redirected to the Unity console immediately.
        /// </summary>
        /// <param name="command">The filename to run.</param>
        /// <param name="envVars">Dictionary containing environment variables to be used.</param>
        /// <param name="arguments">Parameters that will be passed to the command.</param>
        /// <returns>The exit code.</returns>
        public static int RunWithEnvVars(string command, Dictionary<string, string> envVars,
            params string[] arguments)
        {
            void ProcessOutput(string output)
            {
                Debug.Log(output.Trim());
            }

            return RunRedirectedProcess(AppDataPath, command, envVars, ProcessOutput, arguments);
        }

        /// <summary>
        ///     Runs the redirected process and waits for it to return.
        /// </summary>
        /// <param name="workingDirectory">The directory to run the filename from.</param>
        /// <param name="command">The filename to run.</param>
        /// <param name="envVars">Dictionary containing environment variables to be used.</param>
        /// <param name="outputProcessor">Action for processing output line by line.</param>
        /// <param name="arguments">Parameters that will be passed to the command.</param>
        /// <returns>The exit code.</returns>
        private static int RunRedirectedProcess(string workingDirectory, string command, Dictionary<string, string> envVars,
            Action<string> outputProcessor, params string[] arguments)
        {
            var info = new ProcessStartInfo(command, string.Join(" ", arguments))
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };

            foreach (var envVarEntry in envVars ?? new Dictionary<string, string>())
            {
                info.EnvironmentVariables.Add(envVarEntry.Key, envVarEntry.Value);
            }

            using (var process = Process.Start(info))
            {
                if (process == null)
                {
                    throw new Exception(
                        $"Failed to run {info.FileName} {info.Arguments}\nIs the .NET Core SDK installed?");
                }

                process.EnableRaisingEvents = true;

                void OnReceived(object sender, DataReceivedEventArgs args)
                {
                    if (string.IsNullOrEmpty(args.Data))
                    {
                        return;
                    }

                    outputProcessor(args.Data);
                }

                process.OutputDataReceived += OnReceived;
                process.ErrorDataReceived += OnReceived;

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return process.ExitCode;
            }
        }

        private static string ProcessSpatialOutput(string argsData)
        {
            if (!argsData.StartsWith("{") || !argsData.EndsWith("}"))
            {
                return argsData;
            }

            try
            {
                var logEvent = Json.Deserialize(argsData);
                if (logEvent.TryGetValue("msg", out var message))
                {
                    return (string) message;
                }
            }
            catch
            {
                return argsData;
            }

            return argsData;
        }
    }
}
