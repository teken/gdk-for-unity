using System;
using System.Reflection;

namespace Improbable.Worker.CInterop.Internal
{
    /// <summary>
    /// Class providing methods similar to System.Diagnostics.Contract in .NET 4.0.
    /// </summary>
    internal static class Contract
    {
        /// <summary>
        /// Throws the exception <typeparamref name="TException"/> with the parameter paramName
        /// that caused the exception and the exception message passed to the method if
        /// <paramref name="condition"/> is false.
        /// </summary>
        internal static void Requires<TException>(bool condition, string paramName,
                                                  string exceptionMessage = null) where TException : Exception
        {
            if (!condition)
            {
                ConstructorInfo ci = typeof(TException).GetConstructor(new[] {typeof(string), typeof(string)});
                if (ci != null)
                {
                    throw ci.Invoke(new[] {paramName, exceptionMessage}) as TException;
                }
                ci = typeof(TException).GetConstructor(new[] {typeof(string)});
                if (ci != null)
                {
                    throw ci.Invoke(new[] {exceptionMessage}) as TException;
                }
                throw Activator.CreateInstance<TException>();
            }
        }
    }
}