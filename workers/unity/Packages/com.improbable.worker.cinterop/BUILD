package(default_visibility = ["//visibility:private"])

load("//worker_sdk/csharp:assembly_version.bzl", "assembly_version")
load("//tools/build_rules:csharp.bzl", "cs_library")
load("//worker_sdk/common:defaults.bzl", "defaults_file")

defaults_file(
    name = "cs_defaults",
    out = "worker/Defaults.cs",
    template = "//worker_sdk/csharp_cinterop/sdk:worker/Defaults.tmpl.cs",
)

assembly_version(name = "SharedAssemblyInfo")

cs_library(
    name = "sdk",
    srcs = glob(
        include = ["**/*.cs"],
        exclude = ["**/*.tmpl.cs"],
    ) + [
        ":SharedAssemblyInfo.cs",
        ":cs_defaults",
    ],
    assembly_name = "Improbable.Worker.CInterop",
    visibility = ["//worker_sdk:packages"],
    runtime_deps = ["//worker_sdk/c:CoreSdkDll"],
)

cs_library(
    name = "sdk_static",
    srcs = glob(
        include = ["**/*.cs"],
        exclude = ["**/*.tmpl.cs"],
    ) + [
        ":SharedAssemblyInfo.cs",
        ":cs_defaults",
    ],
    assembly_name = "Improbable.Worker.CInteropStatic",
    define_constants = "DLL_IMPORT_STATIC",
    visibility = ["//worker_sdk:packages"],
    runtime_deps = ["//worker_sdk/c:CoreSdkStatic"],
)

# Depends on the combined shared library (CoreSdkDll), which includes the Core SDK and Shunt
# together, to avoid diamond problems (see WRK-361).
cs_library(
    name = "sdk_with_shunt",
    testonly = True,
    srcs = glob(
        include = ["**/*.cs"],
        exclude = ["**/*.tmpl.cs"],
    ) + [
        ":SharedAssemblyInfo.cs",
        ":cs_defaults",
    ],
    assembly_name = "Improbable.Worker.CInteropWithShunt",
    visibility = ["//worker_sdk:packages"],
    runtime_deps = ["//worker_sdk/csharp_cinterop/testing:CoreSdkDll"],
)
