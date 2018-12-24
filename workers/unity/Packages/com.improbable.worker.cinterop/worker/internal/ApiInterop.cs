using System.Collections.Generic;
using System.Text;

namespace Improbable.Worker.CInterop.Internal
{
    internal static class ApiInterop
    {
        private static int Utf8CstrLen(string s, bool includeNullTerminator)
        {
            return (s == null ? 0 : Encoding.UTF8.GetByteCount(s)) + (includeNullTerminator ? 1 : 0);
        }

        private static void ToUtf8Cstr(string s, byte[] buffer, int bufferIndex, bool includeNullTerminator)
        {
            if (s == null)
            {
                return;
            }

            Encoding.UTF8.GetBytes(s, 0, s.Length, buffer, bufferIndex);
            if (includeNullTerminator)
            {
                buffer[bufferIndex + s.Length] = 0;
            }
        }

        public static byte[] ToUtf8Cstr(string s, bool includeNullTerminator = true)
        {
            var buffer = new byte[Utf8CstrLen(s, includeNullTerminator)];
            ToUtf8Cstr(s, buffer, 0, includeNullTerminator);
            return buffer;
        }

        public static byte[] ToPackedUtf8Cstr(
            IList<string> s,
            out IList<int> indexes,
            bool includeNullTerminator = true)
        {
            var totalLength = 0;
            for (var i = 0; i < s.Count; ++i)
            {
                totalLength += Utf8CstrLen(s[i], includeNullTerminator);
            }

            indexes = new int[s.Count];
            var buffer = new byte[totalLength];
            var bufferIndex = 0;
            for (var i = 0; i < s.Count; ++i)
            {
                ToUtf8Cstr(s[i], buffer, bufferIndex, includeNullTerminator);
                indexes[i] = bufferIndex;
                bufferIndex += Utf8CstrLen(s[i], includeNullTerminator);
            }

            return buffer;
        }

        public static unsafe string FromUtf8Cstr(byte* buffer, uint length)
        {
            if (buffer == null)
            {
                return "";
            }

            var managedBuffer = new byte[length];
            for (var i = 0; i < length; ++i)
            {
                managedBuffer[i] = buffer[i];
            }

            return Encoding.UTF8.GetString(managedBuffer);
        }

        // Assuming that the byte buffer is null terminated.
        public static unsafe string FromUtf8Cstr(byte* buffer)
        {
            if (buffer == null)
            {
                return "";
            }

            var length = 0;
            while (buffer[length] != 0)
            {
                ++length;
            }

            var managedBuffer = new byte[length];
            for (var i = 0; i < length; ++i)
            {
                managedBuffer[i] = buffer[i];
            }

            return Encoding.UTF8.GetString(managedBuffer);
        }
    }
}
