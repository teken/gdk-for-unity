namespace Improbable.Worker.CInterop.Internal
{
    /// <summary>
    /// This attribute is valid on static functions and it is used by Mono's
    /// Ahead of Time Compiler (AOT) to generate the code necessary to support
    /// native calls back into managed code.
    /// </summary>
    /// <remarks>
    /// Implemented here as a custom attribute as we do not include Xamarin's
    /// Mono library within the C# Worker SDK layer itself. Based on the official Mono
    /// implementation.
    /// See: https://github.com/mono/mono/blob/master/mcs/class/System/Mono.Util/MonoPInvokeCallbackAttribute.cs
    /// See: https://developer.xamarin.com/api/type/MonoTouch.MonoPInvokeCallbackAttribute/
    /// </remarks>
    public sealed class MonoPInvokeCallbackAttribute : System.Attribute
    {
        public MonoPInvokeCallbackAttribute(System.Type delegateType) {}
    }
}
