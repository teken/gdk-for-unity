using System;
using Improbable.Worker.CInterop.Internal;

namespace Improbable.Worker.CInterop
{
    /// <summary>
    /// Represents data-at-rest for a component identified by the ComponentId.
    /// Underlying binary data format can be either raw SchemaData or UserHandle
    /// controlled entirely by user.
    /// </summary>
    public struct ComponentData
    {
        public uint ComponentId;
        public SchemaComponentData? SchemaData;
        public UIntPtr UserHandle;
        private unsafe CWorker.ComponentData* internalComponentData;

        public ComponentData(SchemaComponentData schemaData)
        {
            ComponentId = schemaData.GetComponentId();
            SchemaData = schemaData;
            UserHandle = (UIntPtr) 0;
            unsafe { internalComponentData = null; }
        }

        public ComponentData(uint componentId, UIntPtr userHandle)
        {
            ComponentId = componentId;
            SchemaData = null;
            UserHandle = userHandle;
            unsafe { internalComponentData = null; }
        }

        // Called by the OpList when receiving data from the C API.
        internal unsafe ComponentData(CWorker.ComponentData* commandResponse)
        {
            ComponentId = commandResponse->ComponentId;
            if (commandResponse->SchemaType != null)
            {
                SchemaData = new SchemaComponentData(commandResponse->SchemaType);
            }
            else
            {
                SchemaData = null;
            }
            UserHandle = (UIntPtr) commandResponse->UserHandle;
            internalComponentData = commandResponse;
        }

        /// <summary>
        /// Increases the reference count of this ComponentData if it is owned by the SDK (returned
        /// in the OpList). You must call <c>.Release()</c> once you are done using it, otherwise
        /// memory will leak.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the ComponentData is not owned by the SDK.
        /// </exception>
        public unsafe void Acquire()
        {
            if (internalComponentData == null)
            {
                throw new InvalidOperationException(
                    "Tried to acquire a component data which wasn't obtained from an op list."
                );
            }
            CWorker.AcquireComponentData(internalComponentData);
        }

        /// <summary>
        /// Decreases the reference count (and frees the memory) after calling <c>.Acquire()</c>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the ComponentData is not owned by the SDK.
        /// </exception>
        private unsafe void Release()
        {
            if (internalComponentData == null)
            {
                throw new InvalidOperationException(
                    "Tried to release a component data which wasn't obtained from an op list."
                );
            }
            CWorker.ReleaseComponentData(internalComponentData);
        }
    }

    /// <summary>
    /// Represents an update for the component identified by the ComponentId.
    /// Underlying binary data format can be either raw SchemaData or some UserHandle
    /// controlled entirely by user.
    /// </summary>
    public struct ComponentUpdate
    {
        public uint ComponentId;
        public SchemaComponentUpdate? SchemaData;
        public UIntPtr UserHandle;
        private unsafe CWorker.ComponentUpdate* internalComponentUpdate;

        public ComponentUpdate(SchemaComponentUpdate schemaData)
        {
            ComponentId = schemaData.GetComponentId();
            SchemaData = schemaData;
            UserHandle = (UIntPtr) 0;
            unsafe { internalComponentUpdate = null; }
        }

        public ComponentUpdate(uint componentId, UIntPtr userHandle)
        {
            ComponentId = componentId;
            SchemaData = null;
            UserHandle = userHandle;
            unsafe { internalComponentUpdate = null; }
        }

        // Called by the OpList when receiving data from the C API.
        internal unsafe ComponentUpdate(CWorker.ComponentUpdate* commandResponse)
        {
            ComponentId = commandResponse->ComponentId;
            if (commandResponse->SchemaType != null)
            {
                SchemaData = new SchemaComponentUpdate(commandResponse->SchemaType);
            }
            else
            {
                SchemaData = null;
            }
            UserHandle = (UIntPtr) commandResponse->UserHandle;
            internalComponentUpdate = commandResponse;
        }

        /// <summary>
        /// Increases the reference count of this ComponentUpdate if it is owned by the SDK (returned
        /// in the OpList). You must call <c>.Release()</c> once you are done using it, otherwise
        /// memory will leak.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the ComponentUpdate is not owned by the SDK.
        /// </exception>
        public unsafe void Acquire()
        {
            if (internalComponentUpdate == null)
            {
                throw new InvalidOperationException(
                    "Tried to acquire a component update which wasn't obtained from an op list."
                );
            }
            CWorker.AcquireComponentUpdate(internalComponentUpdate);
        }

        /// <summary>
        /// Decreases the reference count (and frees the memory) after calling <c>.Acquire()</c>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the ComponentUpdate is not owned by the SDK.
        /// </exception>
        private unsafe void Release()
        {
            if (internalComponentUpdate == null)
            {
                throw new InvalidOperationException(
                    "Tried to release a component update which wasn't obtained from an op list."
                );
            }
            CWorker.ReleaseComponentUpdate(internalComponentUpdate);
        }
    }

    /// <summary>
    /// Represents a command request for a component identified by the ComponentId.
    /// Underlying binary data format can be either raw SchemaData or UserHandle
    /// controlled entirely by user.
    /// </summary>
    public struct CommandRequest
    {
        public uint ComponentId;
        public SchemaCommandRequest? SchemaData;
        public UIntPtr UserHandle;
        private unsafe CWorker.CommandRequest* internalCommandRequest;

        public CommandRequest(SchemaCommandRequest schemaData)
        {
            ComponentId = schemaData.GetComponentId();
            SchemaData = schemaData;
            UserHandle = (UIntPtr) 0;
            unsafe { internalCommandRequest = null; }
        }

        public CommandRequest(uint componentId, UIntPtr userHandle)
        {
            ComponentId = componentId;
            SchemaData = null;
            UserHandle = userHandle;
            unsafe { internalCommandRequest = null; }
        }

        // Called by the OpList when receiving data from the C API.
        internal unsafe CommandRequest(CWorker.CommandRequest* commandResponse)
        {
            ComponentId = commandResponse->ComponentId;
            if (commandResponse->SchemaType != null)
            {
                SchemaData = new SchemaCommandRequest(commandResponse->SchemaType);
            }
            else
            {
                SchemaData = null;
            }
            UserHandle = (UIntPtr) commandResponse->UserHandle;
            internalCommandRequest = commandResponse;
        }

        /// <summary>
        /// Increases the reference count of this CommandRequest if it is owned by the SDK (returned
        /// in the OpList). You must call <c>.Release()</c> once you are done using it, otherwise
        /// memory will leak.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the CommandRequest is not owned by the SDK.
        /// </exception>
        public unsafe void Acquire()
        {
            if (internalCommandRequest == null)
            {
                throw new InvalidOperationException(
                    "Tried to acquire a command request which wasn't obtained from an op list."
                );
            }
            CWorker.AcquireCommandRequest(internalCommandRequest);
        }

        /// <summary>
        /// Decreases the reference count (and frees the memory) after calling <c>.Acquire()</c>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the CommandRequest is not owned by the SDK.
        /// </exception>
        private unsafe void Release()
        {
            if (internalCommandRequest == null)
            {
                throw new InvalidOperationException(
                    "Tried to release a command request which wasn't obtained from an op list."
                );
            }
            CWorker.ReleaseCommandRequest(internalCommandRequest);
        }
    }

    /// <summary>
    /// Represents a command response for a component identified by the ComponentId.
    /// Underlying binary data format can be either raw SchemaData or UserHandle
    /// controlled entirely by user.
    /// </summary>
    public struct CommandResponse
    {
        public uint ComponentId;
        public SchemaCommandResponse? SchemaData;
        public UIntPtr UserHandle;
        private unsafe CWorker.CommandResponse* internalCommandResponse;

        public CommandResponse(SchemaCommandResponse schemaData)
        {
            ComponentId = schemaData.GetComponentId();
            SchemaData = schemaData;
            UserHandle = (UIntPtr) 0;
            unsafe { internalCommandResponse = null; }
        }

        public CommandResponse(uint componentId, UIntPtr userHandle)
        {
            ComponentId = componentId;
            SchemaData = null;
            UserHandle = userHandle;
            unsafe { internalCommandResponse = null; }
        }

        // Called by the OpList when receiving data from the C API.
        internal unsafe CommandResponse(CWorker.CommandResponse* commandResponse)
        {
            ComponentId = commandResponse->ComponentId;
            if (commandResponse->SchemaType != null)
            {
                SchemaData = new SchemaCommandResponse(commandResponse->SchemaType);
            }
            else
            {
                SchemaData = null;
            }
            UserHandle = (UIntPtr) commandResponse->UserHandle;
            internalCommandResponse = commandResponse;
        }

        /// <summary>
        /// Increases the reference count of this CommandResponse if it is owned by the SDK (returned
        /// in the OpList). You must call <c>.Release()</c> once you are done using it, otherwise
        /// memory will leak.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the CommandResponse is not owned by the SDK.
        /// </exception>
        public unsafe void Acquire()
        {
            if (internalCommandResponse == null)
            {
                throw new InvalidOperationException(
                    "Tried to acquire a command response which wasn't obtained from an op list."
                );
            }
            CWorker.AcquireCommandResponse(internalCommandResponse);
        }

        /// <summary>
        /// Decreases the reference count (and frees the memory) after calling <c>.Acquire()</c>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the CommandResponse is not owned by the SDK.
        /// </exception>
        private unsafe void Release()
        {
            if (internalCommandResponse == null)
            {
                throw new InvalidOperationException(
                    "Tried to release a command response which wasn't obtained from an op list."
                );
            }
            CWorker.ReleaseCommandResponse(internalCommandResponse);
        }
    }
}
