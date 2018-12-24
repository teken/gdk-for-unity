using Improbable.Worker.CInterop.Internal;
using System;

namespace Improbable.Worker.CInterop
{
    /// <summary>
    /// A class representing the standard future concept. It can be used for both synchronous
    /// and asynchronous interaction.
    /// </summary>
    /// <typeparam name="T">The type of object the future returns.</typeparam>
    public class Future<T> : IDisposable
    {
        private readonly CptrHandle handle;
        private readonly Func<uint?, T> get;
        private T valueResult;

        internal Future(CptrHandle handle, Func<uint?, T> get)
        {
            this.handle = handle;
            this.get = get;
        }

        /// <inheritdoc cref="IDisposable"/>
        public virtual void Dispose()
        {
            handle.Dispose();
        }

        /// <summary>
        /// Waits until the result becomes available, and returns it. If the result was already
        /// obtained by a previous call to Get() or Get(timeoutMillis), this function returns it
        /// immediately.
        /// </summary>
        /// <returns>The result.</returns>
        public T Get()
        {
            Contract.Requires<ObjectDisposedException>(!handle.IsClosed, GetType().Name);

            if (valueResult != null)
            {
                return valueResult;
            }
            valueResult = get(/* timeoutMillis */ null);
            return valueResult;
        }

        /// <summary>
        /// Checks if the result is available. If the result is available, this function returns
        /// true and the result will be stored in the out parameter. Otherwise, the function
        /// returns false.
        /// </summary>
        /// <param name="result">The result of the future if it has finished.</param>
        /// <param name="timeoutMillis">
        /// An optional time to wait for the result to become available.
        /// </param>
        /// <returns>True if the result is available, false otherwise.</returns>
        public bool TryGet(out T result, uint timeoutMillis = 0)
        {
            Contract.Requires<ObjectDisposedException>(!handle.IsClosed, GetType().Name);

            valueResult = get(timeoutMillis);
            if (valueResult != null)
            {
                result = valueResult;
                return true;
            }

            result = default(T);
            return false;
        }
    }
}
