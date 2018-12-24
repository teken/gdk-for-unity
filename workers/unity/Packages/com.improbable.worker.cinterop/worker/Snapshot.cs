using Improbable.Worker.CInterop.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EntityId = System.Int64;

namespace Improbable.Worker.CInterop
{
    public unsafe class SnapshotInputStream : IDisposable
    {
        private SnapshotInputStreamHandle inputStream;
        private List<WrappedGcHandle> componentVtableHandles;

        public SnapshotInputStream(string path, SnapshotParameters snapshotParams)
        {
            ParameterConversion.ConvertSnapshotParameters(snapshotParams, (parameters, handles) =>
            {
                componentVtableHandles = handles;
                fixed (byte* pathBytes = ApiInterop.ToUtf8Cstr(path))
                {
                    inputStream = CWorker.SnapshotInputStream_Create(pathBytes, parameters);
                }
            });
        }

        public void Dispose()
        {
            inputStream.Dispose();
            ParameterConversion.FreeVtableHandles(componentVtableHandles);
        }

        /// <exception cref="InvalidDataException">if any error is encountered</exception>
        public void ReadEntity(out EntityId entityId, out Entity entity)
        {
            entity = new Entity();

            var snapshotEntity = CWorker.SnapshotInputStream_ReadEntity(inputStream);
            var snapshotError = CWorker.SnapshotInputStream_GetError(inputStream);
            if (snapshotError != null)
            {
                entityId = new EntityId();
                throw new InvalidDataException(ApiInterop.FromUtf8Cstr(snapshotError));
            }

            // Load each component.
            for (int i = 0; i < snapshotEntity->ComponentCount; i++)
            {
                var component = snapshotEntity->Components[i];
                entity.components.Add(component.ComponentId, new ComponentData(&component));
            }

            entityId = snapshotEntity->EntityId;
        }

        public bool HasNext()
        {
            return CWorker.SnapshotInputStream_HasNext(inputStream) != 0;
        }
    }

    public unsafe class SnapshotOutputStream : IDisposable
    {
        private SnapshotOutputStreamHandle outputStream;
        private List<WrappedGcHandle> componentVtableHandles;

        public SnapshotOutputStream(string path, SnapshotParameters snapshotParams)
        {
            ParameterConversion.ConvertSnapshotParameters(snapshotParams, (parameters, handles) =>
            {
                componentVtableHandles = handles;
                fixed (byte* pathBytes = ApiInterop.ToUtf8Cstr(path))
                {
                    outputStream = CWorker.SnapshotOutputStream_Create(pathBytes, parameters);
                }
            });
        }

        public void Dispose()
        {
            outputStream.Dispose();
            ParameterConversion.FreeVtableHandles(componentVtableHandles);
        }

        /// <exception cref="InvalidDataException">if any error is encountered</exception>
        public void WriteEntity(EntityId entityId, Entity entity)
        {
            var snapshotEntity = new CWorker.Entity
            {
                EntityId = entityId,
                ComponentCount = (uint) entity.components.Count
            };

            var componentCount = entity.components.Count;
            var componentStorage = new CWorker.ComponentData[componentCount];

            // snapshotEntity.Component is set below, from componentStorage.
            var componentIndex = 0;
            foreach (var entry in entity.components)
            {
                componentStorage[componentIndex].ComponentId = entry.Key;
                componentStorage[componentIndex].UserHandle = (void*) entry.Value.UserHandle;
                if (entry.Value.SchemaData != null)
                {
                    componentStorage[componentIndex].SchemaType = entry.Value.SchemaData.Value.handle;
                }

                ++componentIndex;
            }

            var errors = new StringBuilder();

            fixed (CWorker.ComponentData* componentBuffer = componentStorage)
            {
                // Set up all the C pointers to the storage allocated above.
                snapshotEntity.Components = componentBuffer;
                CWorker.SnapshotOutputStream_WriteEntity(outputStream, &snapshotEntity);
                var error = CWorker.SnapshotOutputStream_GetError(outputStream);
                if (error != null)
                {
                    errors.Append(ApiInterop.FromUtf8Cstr(error));
                }
            }

            if (errors.Length > 0)
            {
                throw new InvalidDataException(errors.ToString());
            }
        }
    }
}
