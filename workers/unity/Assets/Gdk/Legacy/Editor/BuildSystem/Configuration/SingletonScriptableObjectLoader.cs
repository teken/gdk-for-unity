using System;
using System.Collections.Generic;

namespace Improbable.Gdk.Legacy.BuildSystem.Configuration
{
    internal static class SingletonScriptableObjectLoader
    {
        internal static readonly HashSet<Type> LoadingInstances = new HashSet<Type>();
    }
}
