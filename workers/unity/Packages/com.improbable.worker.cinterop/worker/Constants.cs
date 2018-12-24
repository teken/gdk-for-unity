namespace Improbable.Worker.CInterop
{
    /// <summary>
    /// A struct to contain constants that can be shared in Improbable.Worker.CInterop.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Constant used to alter between static/dynamic linking.
        /// </summary>
        #if DLL_IMPORT_STATIC
            public const string WorkerLibrary = "__Internal";
        #else
            public const string WorkerLibrary = "CoreSdkDll";
        #endif
    }
}
