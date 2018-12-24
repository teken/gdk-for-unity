using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Improbable.Worker.CInterop.Internal
{
    internal class WrappedGcHandle : CriticalFinalizerObject, IDisposable
    {
        private GCHandle handle;

        public WrappedGcHandle(object obj)
        {
            handle = GCHandle.Alloc(obj);
        }

        ~WrappedGcHandle()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }

        public void Dispose()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
            GC.SuppressFinalize(this);
        }

        public IntPtr Get()
        {
            return GCHandle.ToIntPtr(handle);
        }
    }

    public abstract class CptrHandle : SafeHandle
    {
        protected CptrHandle() : base(/* invalid value */ (IntPtr) 0, /* owns handle */ true) { }

        public override bool IsInvalid => handle == (IntPtr) 0;
    }

    internal class ConnectionHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
            CWorker.Connection_Destroy(handle);
            return true;
        }
    }

    internal class Alpha_LocatorHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
            CWorker.Alpha_Locator_Destroy(handle);
            return true;
        }
    }

    internal class LocatorHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
			CWorker.Locator_Destroy(handle);
            return true;
        }
    }

    public class SnapshotInputStreamHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
			CWorker.SnapshotInputStream_Destroy(handle);
            return true;
        }
    }

    public class SnapshotOutputStreamHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
			CWorker.SnapshotOutputStream_Destroy(handle);
            return true;
        }
    }

    internal class OpListHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
            CWorker.OpList_Destroy(handle);
            return true;
        }

        internal unsafe CWorker.OpList* GetUnderlying()
        {
            return (CWorker.OpList*)handle;
        }
    }

    internal class ConnectionFutureHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
			CWorker.ConnectionFuture_Destroy(handle);
            return true;
        }
    }

    internal class DeploymentListFutureHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
			CWorker.DeploymentListFuture_Destroy(handle);
            return true;
        }
    }

    internal class Alpha_PlayerIdentityTokenResponseFutureHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
            CWorker.Alpha_PlayerIdentityTokenResponseFuture_Destroy(handle);
            return true;
        }
    }

    internal class Alpha_LoginTokensResponseFutureHandle : CptrHandle
    {
        protected override bool ReleaseHandle()
        {
            CWorker.Alpha_LoginTokensResponseFuture_Destroy(handle);
            return true;
        }
    }

    public class GcHandlePool : CriticalFinalizerObject, IDisposable
    {
        private List<GCHandle> handles;

        public GcHandlePool()
        {
            handles = new List<GCHandle>();
        }

        ~GcHandlePool()
        {
            DisposeInternal();
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        public IntPtr Pin(object obj)
        {
            handles.Add(GCHandle.Alloc(obj, GCHandleType.Pinned));
            return handles[handles.Count - 1].AddrOfPinnedObject();
        }

        private void DisposeInternal()
        {
            for (int i = 0; i < handles.Count; ++i)
            {
                if (handles[i].IsAllocated) {
                    handles[i].Free();
                }
            }
        }
    }
}
