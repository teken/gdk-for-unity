using System;
using Improbable.Worker.CInterop.Internal;
using FieldId = System.UInt32;

namespace Improbable.Worker.CInterop
{
    /// <summary>
    /// A wrapper over a raw Schema_ComponentData pointer. Ownership of the memory is transferred
    /// when this object is passed into any other API. If this object is never passed into an API,
    /// then <c>.Destroy()</c> must be called explicitly.
    /// </summary>
    public unsafe struct SchemaComponentData
    {
        internal CSchema.ComponentData* handle;

        /// <exception cref="ArgumentException">if the handle pointer is null.</exception>
        internal SchemaComponentData(CSchema.ComponentData* handle)
        {
            if (handle == null)
            {
                throw new ArgumentException("Handle pointer cannot be null");
            }
            this.handle = handle;
        }

        public SchemaComponentData(uint componentId)
        {
            handle = CSchema.CreateComponentData(componentId);
        }

        public void Destroy() => CSchema.DestroyComponentData(handle);

        public uint GetComponentId() => CSchema.GetComponentDataComponentId(handle);

        public SchemaObject GetFields() => new SchemaObject(CSchema.GetComponentDataFields(handle));
    }

    /// <summary>
    /// A wrapper over a raw Schema_ComponentUpdate pointer. Ownership of the memory is  transferred
    /// when this object is passed into any other API. If this object is never passed into an API,
    /// then <c>.Destroy()</c> must be called explicitly.
    /// </summary>
    public unsafe struct SchemaComponentUpdate
    {
        internal CSchema.ComponentUpdate* handle;

        /// <exception cref="ArgumentException">if the handle pointer is null.</exception>
        internal SchemaComponentUpdate(CSchema.ComponentUpdate* handle)
        {
            if (handle == null)
            {
                throw new ArgumentException("Handle pointer cannot be null");
            }
            this.handle = handle;
        }

        public SchemaComponentUpdate(uint componentId)
        {
            handle = CSchema.CreateComponentUpdate(componentId);
        }

        public void Destroy() => CSchema.DestroyComponentUpdate(handle);

        public uint GetComponentId() => CSchema.GetComponentUpdateComponentId(handle);

        public SchemaObject GetFields() => new SchemaObject(CSchema.GetComponentUpdateFields(handle));

        public SchemaObject GetEvents() => new SchemaObject(CSchema.GetComponentUpdateEvents(handle));

        public void ClearClearedFields() => CSchema.ClearComponentUpdateClearedFields(handle);

        public void AddClearedField(FieldId fieldId) => CSchema.AddComponentUpdateClearedField(handle, fieldId);

        public uint GetClearedFieldCount() => CSchema.GetComponentUpdateClearedFieldCount(handle);

        public FieldId IndexClearedField(uint index) => CSchema.IndexComponentUpdateClearedField(handle, index);

        public FieldId[] GetClearedFields()
        {
            FieldId[] outFields = new FieldId[GetClearedFieldCount()];
            fixed (FieldId* fixedFields = outFields)
            {
                CSchema.GetComponentUpdateClearedFieldList(handle, fixedFields);
            }

            return outFields;
        }
    }

    /// <summary>
    /// A wrapper over a raw Schema_CommandRequest pointer. Ownership of the memory is transferred
    /// when this object is passed into any other API. If this object is never passed into an API,
    /// then <c>.Destroy()</c> must be called explicitly.
    /// </summary>
    public unsafe struct SchemaCommandRequest
    {
        internal CSchema.CommandRequest* handle;

        /// <exception cref="ArgumentException">if the handle pointer is null.</exception>
        internal SchemaCommandRequest(CSchema.CommandRequest* handle)
        {
            if (handle == null)
            {
                throw new ArgumentException("Handle pointer cannot be null");
            }
            this.handle = handle;
        }

        public SchemaCommandRequest(uint componentId, uint commandIndex)
        {
            handle = CSchema.CreateCommandRequest(componentId, commandIndex);
        }

        public void Destroy() => CSchema.DestroyCommandRequest(handle);

        public uint GetComponentId() => CSchema.GetCommandRequestComponentId(handle);

        public uint GetCommandIndex() => CSchema.GetCommandRequestCommandIndex(handle);

        public SchemaObject GetObject() => new SchemaObject(CSchema.GetCommandRequestObject(handle));
    }

    /// <summary>
    /// A wrapper over a raw Schema_CommandResponse pointer. Ownership of the memory is transferred
    /// when this object is passed into any other API. If this object is never passed into an API,
    /// then <c>.Destroy()</c> must be called explicitly.
    /// </summary>
    public unsafe struct SchemaCommandResponse
    {
        internal CSchema.CommandResponse* handle;

        /// <exception cref="ArgumentException">if the handle pointer is null.</exception>
        internal SchemaCommandResponse(CSchema.CommandResponse* handle)
        {
            if (handle == null)
            {
                throw new ArgumentException("Handle pointer cannot be null");
            }
            this.handle = handle;
        }

        public SchemaCommandResponse(uint componentId, uint commandIndex)
        {
            handle = CSchema.CreateCommandResponse(componentId, commandIndex);
        }

        public void Destroy() => CSchema.DestroyCommandResponse(handle);

        public uint GetComponentId() => CSchema.GetCommandResponseComponentId(handle);

        public uint GetCommandIndex() => CSchema.GetCommandResponseCommandIndex(handle);

        public SchemaObject GetObject() => new SchemaObject(CSchema.GetCommandResponseObject(handle));
    }
}
