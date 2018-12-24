using System;
using System.Runtime.InteropServices;
using Improbable.Worker.CInterop.Internal;
using EntityId = System.Int64;
using FieldId = System.UInt32;
using Uint64 = System.UInt64;
using Uint32 = System.UInt32;
using Uint8 = System.Byte;
using Int64 = System.Int64;
using Int32 = System.Int32;

namespace Improbable.Worker.CInterop
{
    /// <summary>
    /// A wrapper over a raw Schema_Object pointer.
    ///
    /// In general, if you try to obtain a value from a field when the field does not exist (or the
    /// type is incorrect), a default initialized value of that type will be returned. If you try to
    /// obtain an Object from a field that doesn't exist, it will return a _valid_ SchemaObject
    /// which is unreachable (so no way to obtain it by calling GetObject(...)). The GetXXCount
    /// functions can be used to detect this case.
    ///
    /// In addition to unsafe GetXXList and AddXXList functions that rely on the caller to provide
    /// an unmanaged buffer, there are safe wrappers that operate on C# arrays. Note that these are
    /// slightly less performant as they will make a copy of the data, but it avoids any potential
    /// lifetime issues.
    ///
    /// Note that it is advised not to use this data structure to keep around data in a persistent
    /// way due to the lack of type checking. Ideally, this should only be used to hold an
    /// intermediate "serialized" object.
    /// </summary>
    /// <remarks>
    /// SchemaObject is the main type for data manipulation, and roughly-speaking corresponds to an
    /// instance of a "type" as defined in schema. Each SchemaObject is owned by a "root" schema type
    /// instance, of which there are four: SchemaCommandRequest, SchemaCommandResponse,
    /// SchemaComponentData, and SchemaComponentUpdate.
    ///
    /// Each field defined in schema has a _field ID_, a  _type_ and an _arity_. For each type, there is
    /// a family of functions that can be used to read and write fields of that type for a particular
    /// field ID inside a SchemaObject. The mapping from schema type to function family is given below:
    ///
    ///      .schema type | function family
    /// ------------------|----------------
    ///             int32 | Int32
    ///             int64 | Int64
    ///            uint32 | Uint32
    ///            uint64 | Uint64
    ///            sint32 | Sint32
    ///            sint64 | Sint64
    ///           fixed32 | Fixed32
    ///           fixed64 | Fixed64
    ///          sfixed32 | Sfixed32
    ///          sfixed64 | Sfixed64
    ///              bool | Bool
    ///             float | Float
    ///            double | Double
    ///            string | Bytes / String
    ///          EntityId | EntityId (alias for Int64)
    ///             bytes | Bytes
    /// user-defined enum | Enum (alias for Uint32)
    /// user-defined type | Object
    ///
    /// The arity of a field is either singular, option, or list. The same function family can be used
    /// for manipulating fields of any arity: a singular field is simply a field whose ID occurs exactly
    /// once; an option field is a field whose ID occurs zero or one times; and a list field is a field
    /// whose ID occurs any number of times.
    ///
    /// Therefore, typically, where X is the function family, we use the GetX and AddX
    /// functions to read and write singular fields; the GetXCount, GetX and AddX
    /// functions to read and write option fields, and the GetXCount, IndexX and
    /// AddX functions to read and write list fields. However, these functions are all
    /// interopable: internally, GetX just retrieves the last occurence of the given field ID, for
    /// instance.
    ///
    /// Note that for maximum efficiency, fields should be accessed in increasing ID order. If there
    /// are multiple values in a single field ID (GetXXCount is greater than 1), then they should
    /// be accessed in increasing index order.
    ///
    /// Map fields are represented as lists of Object fields, where each object represents an entry in
    /// the map, and has the key under field ID 1 (SchemaMapKeyFieldId) and the value under field ID
    /// 2 (SchemaMapValueFieldId).
    ///
    /// It is the responsibility of the user to ensure that SchemaObjects are accessed and mutated in a
    /// way consistent with the schema definitions of their corresponding types. Typically, this is done
    /// by writing a custom code generator for the schema AST.
    ///
    /// </remarks>
    public unsafe struct SchemaObject
    {
        public const int SchemaMapKeyFieldId = 1;
        public const int SchemaMapValueFieldId = 2;

        internal CSchema.Object* handle;

        internal SchemaObject(CSchema.Object* handle)
        {
            this.handle = handle;
        }
        
        private string GetError()
        {
            var error = CSchema.GetError(handle);
            if (error != null)
            {
                return ApiInterop.FromUtf8Cstr(error);
            }
            else
            {
                return null;
            }
        }

        public void Clear() => CSchema.Clear(handle);
        
        public void ClearField(FieldId fieldId) => CSchema.ClearField(handle, fieldId);

        /// <remark>
        /// If `this == other`, or if the objects are not associated with the same root schema type
        /// instance (SchemaComponentData, etc), no operation is performed.
        /// </remark>
        public void ShallowCopy(SchemaObject other) => CSchema.ShallowCopy(other.handle, handle);

        /// <remark>
        /// If `this == other`, or if the objects are not associated with the same root schema type
        /// instance (SchemaComponentData, etc), no operation is performed.
        /// </remark>
        public void ShallowCopyField(SchemaObject other, FieldId fieldId) => CSchema.ShallowCopyField(other.handle, handle, fieldId);

        public SchemaObject AllocateObject() => new SchemaObject(CSchema.AllocateObject(handle));

        /// <exception cref="InvalidOperationException">if there's a failure deserializing the buffer.</exception>
        public void MergeFromBuffer(byte[] buffer)
        {
            Uint32 bufferLength = (Uint32) buffer.Length;
            fixed (byte* bufferPtr = buffer)
            {
                if (CSchema.MergeFromBuffer(handle, bufferPtr, bufferLength) == 0)
                {
                    throw new InvalidOperationException("Failed to deserialize schema object. Reason: " + GetError());
                }
            }
        }
        
        /// <exception cref="InvalidOperationException">if there's a failure serializing the object.</exception>
        public byte[] Serialize()
        {
            Uint32 bufferLength = CSchema.GetWriteBufferLength(handle);
            byte[] outBytes = new byte[bufferLength];
            fixed (byte* outBuffer = outBytes)
            {
                if (CSchema.SerializeToBuffer(handle, outBuffer, bufferLength) == 0)
                {
                    throw new InvalidOperationException("Failed to serialize schema object. Reason: " + GetError());
                }
            }

            return outBytes;
        }

        FieldId[] GetUniqueFieldIds()
        {
            Uint32 fieldIdCount = CSchema.GetUniqueFieldIdCount(handle);
            FieldId[] outFieldIds = new FieldId[fieldIdCount];
            fixed (FieldId* outFieldIdBuffer = outFieldIds)
            {
                CSchema.GetUniqueFieldIds(handle, outFieldIdBuffer);
            }
            return outFieldIds;
        }

        public void AddFloat(FieldId fieldId, float value) => CSchema.AddFloat(handle, fieldId, value);

        public void AddDouble(FieldId fieldId, double value) => CSchema.AddDouble(handle, fieldId, value);

        public void AddBool(FieldId fieldId, bool value) => CSchema.AddBool(handle, fieldId, Convert.ToByte(value));

        public void AddInt32(FieldId fieldId, Int32 value) => CSchema.AddInt32(handle, fieldId, value);

        public void AddInt64(FieldId fieldId, Int64 value) => CSchema.AddInt64(handle, fieldId, value);

        public void AddUint32(FieldId fieldId, Uint32 value) => CSchema.AddUint32(handle, fieldId, value);

        public void AddUint64(FieldId fieldId, Uint64 value) => CSchema.AddUint64(handle, fieldId, value);

        public void AddSint32(FieldId fieldId, Int32 value) => CSchema.AddSint32(handle, fieldId, value);

        public void AddSint64(FieldId fieldId, Int64 value) => CSchema.AddSint64(handle, fieldId, value);

        public void AddFixed32(FieldId fieldId, Uint32 value) => CSchema.AddFixed32(handle, fieldId, value);

        public void AddFixed64(FieldId fieldId, Uint64 value) => CSchema.AddFixed64(handle, fieldId, value);

        public void AddSfixed32(FieldId fieldId, Int32 value) => CSchema.AddSfixed32(handle, fieldId, value);

        public void AddSfixed64(FieldId fieldId, Int64 value) => CSchema.AddSfixed64(handle, fieldId, value);

        public void AddEntityId(FieldId fieldId, EntityId value) => CSchema.AddEntityId(handle, fieldId, value);

        public void AddEnum(FieldId fieldId, Uint32 value) => CSchema.AddEnum(handle, fieldId, value);

        public void AddBytes(FieldId fieldId, byte* buffer, Uint32 bufferLength)
        {
            CSchema.AddBytes(handle, fieldId, buffer, bufferLength);
        }

        public void AddBytes(FieldId fieldId, byte[] value)
        {
            if (value == null)
            {
                CSchema.AddBytes(handle, fieldId, null, 0);
                return;
            }

            Uint32 bufferLength = (Uint32) value.Length;
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (byte* valueBuffer = value)
            {
                CWorker.Memcpy(buffer, valueBuffer, (UIntPtr) bufferLength);
            }

            AddBytes(fieldId, buffer, bufferLength);
        }

        public SchemaObject AddObject(FieldId fieldId) => new SchemaObject(CSchema.AddObject(handle, fieldId));

        public void AddString(FieldId fieldId, string value)
        {
            AddBytes(fieldId, ApiInterop.ToUtf8Cstr(value, false));
        }

        public void AddFloatList(FieldId fieldId, float[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(float)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (float* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddFloatList(fieldId, (float*) buffer, values.Length);
        }

        public void AddFloatList(FieldId fieldId, float* buffer, int length)
        {
            CSchema.AddFloatList(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddDoubleList(FieldId fieldId, double[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(double)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (double* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddDoubleList(fieldId, (double*) buffer, values.Length);
        }

        public void AddDoubleList(FieldId fieldId, double* buffer, int length)
        {
            CSchema.AddDoubleList(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddBoolList(FieldId fieldId, bool[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Uint8)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            for (int i = 0; i < values.Length; ++i)
            {
                buffer[i] = Convert.ToByte(values[i]);
            }

            AddBoolList(fieldId, buffer, values.Length);
        }

        public void AddBoolList(FieldId fieldId, Uint8* buffer, int length)
        {
            CSchema.AddBoolList(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddInt32List(FieldId fieldId, Int32[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Int32)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Int32* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddInt32List(fieldId, (Int32*) buffer, values.Length);
        }

        public void AddInt32List(FieldId fieldId, Int32* buffer, int length)
        {
            CSchema.AddInt32List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddInt64List(FieldId fieldId, Int64[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Int64)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Int64* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddInt64List(fieldId, (Int64*) buffer, values.Length);
        }

        public void AddInt64List(FieldId fieldId, Int64* buffer, int length)
        {
            CSchema.AddInt64List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddUint32List(FieldId fieldId, Uint32[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Uint32)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Uint32* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddUint32List(fieldId, (Uint32*) buffer, values.Length);
        }

        public void AddUint32List(FieldId fieldId, Uint32* buffer, int length)
        {
            CSchema.AddUint32List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddUint64List(FieldId fieldId, Uint64[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Uint64)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Uint64* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddUint64List(fieldId, (Uint64*) buffer, values.Length);
        }

        public void AddUint64List(FieldId fieldId, Uint64* buffer, int length)
        {
            CSchema.AddUint64List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddSint32List(FieldId fieldId, Int32[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Int32)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Int32* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddSint32List(fieldId, (Int32*) buffer, values.Length);
        }

        public void AddSint32List(FieldId fieldId, Int32* buffer, int length)
        {
            CSchema.AddSint32List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddSint64List(FieldId fieldId, Int64[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Int64)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Int64* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddSint64List(fieldId, (Int64*) buffer, values.Length);
        }

        public void AddSint64List(FieldId fieldId, Int64* buffer, int length)
        {
            CSchema.AddSint64List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddFixed32List(FieldId fieldId, Uint32[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Uint32)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Uint32* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddFixed32List(fieldId, (Uint32*) buffer, values.Length);
        }

        public void AddFixed32List(FieldId fieldId, Uint32* buffer, int length)
        {
            CSchema.AddFixed32List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddFixed64List(FieldId fieldId, Uint64[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Uint64)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Uint64* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddFixed64List(fieldId, (Uint64*) buffer, values.Length);
        }

        public void AddFixed64List(FieldId fieldId, Uint64* buffer, int length)
        {
            CSchema.AddFixed64List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddSfixed32List(FieldId fieldId, Int32[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Int32)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Int32* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddSfixed32List(fieldId, (Int32*) buffer, values.Length);
        }

        public void AddSfixed32List(FieldId fieldId, Int32* buffer, int length)
        {
            CSchema.AddSfixed32List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddSfixed64List(FieldId fieldId, Int64[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Int64)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Int64* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddSfixed64List(fieldId, (Int64*) buffer, values.Length);
        }

        public void AddSfixed64List(FieldId fieldId, Int64* buffer, int length)
        {
            CSchema.AddSfixed64List(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddEntityIdList(FieldId fieldId, EntityId[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(EntityId)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (EntityId* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddEntityIdList(fieldId, (EntityId*) buffer, values.Length);
        }

        public void AddEntityIdList(FieldId fieldId, EntityId* buffer, int length)
        {
            CSchema.AddEntityIdList(handle, fieldId, buffer, (Uint32) length);
        }

        public void AddEnumList(FieldId fieldId, Uint32[] values)
        {
            Uint32 bufferLength = (Uint32) (values.Length * Marshal.SizeOf(typeof(Uint32)));
            Uint8* buffer = CSchema.AllocateBuffer(handle, bufferLength);
            fixed (Uint32* valueBuffer = values)
            {
                CWorker.Memcpy(buffer, (Uint8*) valueBuffer, (UIntPtr) bufferLength);
            }

            AddEnumList(fieldId, (Uint32*) buffer, values.Length);
        }

        public void AddEnumList(FieldId fieldId, Uint32* buffer, int length)
        {
            CSchema.AddEnumList(handle, fieldId, buffer, (Uint32) length);
        }

        public Uint32 GetFloatCount(FieldId fieldId) => CSchema.GetFloatCount(handle, fieldId);

        public Uint32 GetDoubleCount(FieldId fieldId) => CSchema.GetDoubleCount(handle, fieldId);

        public Uint32 GetBoolCount(FieldId fieldId) => CSchema.GetBoolCount(handle, fieldId);

        public Uint32 GetInt32Count(FieldId fieldId) => CSchema.GetInt32Count(handle, fieldId);

        public Uint32 GetInt64Count(FieldId fieldId) => CSchema.GetInt64Count(handle, fieldId);

        public Uint32 GetUint32Count(FieldId fieldId) => CSchema.GetUint32Count(handle, fieldId);

        public Uint32 GetUint64Count(FieldId fieldId) => CSchema.GetUint64Count(handle, fieldId);

        public Uint32 GetSint32Count(FieldId fieldId) => CSchema.GetSint32Count(handle, fieldId);

        public Uint32 GetSint64Count(FieldId fieldId) => CSchema.GetSint64Count(handle, fieldId);

        public Uint32 GetFixed32Count(FieldId fieldId) => CSchema.GetFixed32Count(handle, fieldId);

        public Uint32 GetFixed64Count(FieldId fieldId) => CSchema.GetFixed64Count(handle, fieldId);

        public Uint32 GetSfixed32Count(FieldId fieldId) => CSchema.GetSfixed32Count(handle, fieldId);

        public Uint32 GetSfixed64Count(FieldId fieldId) => CSchema.GetSfixed64Count(handle, fieldId);

        public Uint32 GetEntityIdCount(FieldId fieldId) => CSchema.GetEntityIdCount(handle, fieldId);

        public Uint32 GetEnumCount(FieldId fieldId) => CSchema.GetEnumCount(handle, fieldId);

        public Uint32 GetBytesCount(FieldId fieldId) => CSchema.GetBytesCount(handle, fieldId);

        public Uint32 GetObjectCount(FieldId fieldId) => CSchema.GetObjectCount(handle, fieldId);

        public Uint32 GetStringCount(FieldId fieldId) => GetBytesCount(fieldId);

        public float GetFloat(FieldId fieldId) => CSchema.GetFloat(handle, fieldId);

        public double GetDouble(FieldId fieldId) => CSchema.GetDouble(handle, fieldId);

        public bool GetBool(FieldId fieldId) => Convert.ToBoolean(CSchema.GetBool(handle, fieldId));

        public Int32 GetInt32(FieldId fieldId) => CSchema.GetInt32(handle, fieldId);

        public Int64 GetInt64(FieldId fieldId) => CSchema.GetInt64(handle, fieldId);

        public Uint32 GetUint32(FieldId fieldId) => CSchema.GetUint32(handle, fieldId);

        public Uint64 GetUint64(FieldId fieldId) => CSchema.GetUint64(handle, fieldId);

        public Int32 GetSint32(FieldId fieldId) => CSchema.GetSint32(handle, fieldId);

        public Int64 GetSint64(FieldId fieldId) => CSchema.GetSint64(handle, fieldId);

        public Uint32 GetFixed32(FieldId fieldId) => CSchema.GetFixed32(handle, fieldId);

        public Uint64 GetFixed64(FieldId fieldId) => CSchema.GetFixed64(handle, fieldId);

        public Int32 GetSfixed32(FieldId fieldId) => CSchema.GetSfixed32(handle, fieldId);

        public Int64 GetSfixed64(FieldId fieldId) => CSchema.GetSfixed64(handle, fieldId);

        public EntityId GetEntityId(FieldId fieldId) => CSchema.GetEntityId(handle, fieldId);

        public Uint32 GetEnum(FieldId fieldId) => CSchema.GetEnum(handle, fieldId);

        public byte[] GetBytes(FieldId fieldId)
        {
            Uint8* buffer = GetBytesBuffer(fieldId);
            int bufferLength = GetBytesLength(fieldId);
            byte[] outBytes = new byte[bufferLength];
            fixed (byte* outBuffer = outBytes)
            {
                CWorker.Memcpy(outBuffer, buffer, (UIntPtr) bufferLength);
            }

            return outBytes;
        }

        public int GetBytesLength(FieldId fieldId) => (int) CSchema.GetBytesLength(handle, fieldId);
        
        public Uint8* GetBytesBuffer(FieldId fieldId) => CSchema.GetBytes(handle, fieldId);

        /// <remark>
        /// Note that the schema library deserializes schema objects lazily, so calling `GetObject`
        /// can cause more of the buffer to be deserialized, triggering an error. For that reason,
        /// it is possible for an exception to be thrown.
        /// </remark>
        /// <exception cref="InvalidOperationException">if there's a failure deserializing the buffer.</exception>
        public SchemaObject GetObject(FieldId fieldId)
        {
            var schemaObject = new SchemaObject(CSchema.GetObject(handle, fieldId));
            var error = GetError();
            if (error != null)
            {
                throw new InvalidOperationException("Failed to deserialize schema object. Reason: " + error);
            }
            return schemaObject;
        }

        public string GetString(FieldId fieldId)
        {
            fixed (byte* buffer = GetBytes(fieldId))
            {
                Uint32 length = (Uint32) GetBytesLength(fieldId);
                return ApiInterop.FromUtf8Cstr(buffer, length);
            }
        }

        public float IndexFloat(FieldId fieldId, Uint32 index) => CSchema.IndexFloat(handle, fieldId, index);

        public double IndexDouble(FieldId fieldId, Uint32 index) => CSchema.IndexDouble(handle, fieldId, index);

        public bool IndexBool(FieldId fieldId, Uint32 index) =>
            Convert.ToBoolean(CSchema.IndexBool(handle, fieldId, index));

        public Int32 IndexInt32(FieldId fieldId, Uint32 index) => CSchema.IndexInt32(handle, fieldId, index);

        public Int64 IndexInt64(FieldId fieldId, Uint32 index) => CSchema.IndexInt64(handle, fieldId, index);

        public Uint32 IndexUint32(FieldId fieldId, Uint32 index) => CSchema.IndexUint32(handle, fieldId, index);

        public Uint64 IndexUint64(FieldId fieldId, Uint32 index) => CSchema.IndexUint64(handle, fieldId, index);

        public Int32 IndexSint32(FieldId fieldId, Uint32 index) => CSchema.IndexSint32(handle, fieldId, index);

        public Int64 IndexSint64(FieldId fieldId, Uint32 index) => CSchema.IndexSint64(handle, fieldId, index);

        public Uint32 IndexFixed32(FieldId fieldId, Uint32 index) => CSchema.IndexFixed32(handle, fieldId, index);

        public Uint64 IndexFixed64(FieldId fieldId, Uint32 index) => CSchema.IndexFixed64(handle, fieldId, index);

        public Int32 IndexSfixed32(FieldId fieldId, Uint32 index) => CSchema.IndexSfixed32(handle, fieldId, index);

        public Int64 IndexSfixed64(FieldId fieldId, Uint32 index) => CSchema.IndexSfixed64(handle, fieldId, index);

        public EntityId IndexEntityId(FieldId fieldId, Uint32 index) => CSchema.IndexEntityId(handle, fieldId, index);

        public Uint32 IndexEnum(FieldId fieldId, Uint32 index) => CSchema.IndexEnum(handle, fieldId, index);

        public byte[] IndexBytes(FieldId fieldId, Uint32 index)
        {
            Uint8* buffer = IndexBytesBuffer(fieldId, index);
            int bufferLength = IndexBytesLength(fieldId, index);
            byte[] outBytes = new byte[bufferLength];
            fixed (byte* outBuffer = outBytes)
            {
                CWorker.Memcpy(outBuffer, buffer, (UIntPtr) bufferLength);
            }

            return outBytes;
        }

        public int IndexBytesLength(FieldId fieldId, Uint32 index) => (int) CSchema.IndexBytesLength(handle, fieldId, index);
        
        public Uint8* IndexBytesBuffer(FieldId fieldId, Uint32 index) => CSchema.IndexBytes(handle, fieldId, index);

        public SchemaObject IndexObject(FieldId fieldId, Uint32 index) =>
            new SchemaObject(CSchema.IndexObject(handle, fieldId, index));

        public string IndexString(FieldId fieldId, Uint32 index)
        {
            fixed (byte* buffer = IndexBytes(fieldId, index))
            {
                Uint32 length = (Uint32) IndexBytesLength(fieldId, index);
                return ApiInterop.FromUtf8Cstr(buffer, length);
            }
        }

        public float[] GetFloatList(FieldId fieldId)
        {
            float[] values = new float[GetFloatCount(fieldId)];
            fixed (float* buffer = values)
            {
                GetFloatList(fieldId, buffer);
            }

            return values;
        }

        public void GetFloatList(FieldId fieldId, float* buffer) => CSchema.GetFloatList(handle, fieldId, buffer);

        public double[] GetDoubleList(FieldId fieldId)
        {
            double[] values = new double[GetDoubleCount(fieldId)];
            fixed (double* buffer = values)
            {
                GetDoubleList(fieldId, buffer);
            }

            return values;
        }

        public void GetDoubleList(FieldId fieldId, double* buffer) => CSchema.GetDoubleList(handle, fieldId, buffer);

        public bool[] GetBoolList(FieldId fieldId)
        {
            bool[] values = new bool[GetBoolCount(fieldId)];
            Uint8* tempBuffer = (Uint8*) Marshal.AllocHGlobal(values.Length);
            GetBoolList(fieldId, tempBuffer);

            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = Convert.ToBoolean(tempBuffer[i]);
            }

            Marshal.FreeHGlobal((IntPtr) tempBuffer);
            return values;
        }

        public void GetBoolList(FieldId fieldId, Uint8* buffer) => CSchema.GetBoolList(handle, fieldId, buffer);

        public Int32[] GetInt32List(FieldId fieldId)
        {
            Int32[] values = new Int32[GetInt32Count(fieldId)];
            fixed (Int32* buffer = values)
            {
                GetInt32List(fieldId, buffer);
            }

            return values;
        }

        public void GetInt32List(FieldId fieldId, Int32* buffer) => CSchema.GetInt32List(handle, fieldId, buffer);

        public Int64[] GetInt64List(FieldId fieldId)
        {
            Int64[] values = new Int64[GetInt64Count(fieldId)];
            fixed (Int64* buffer = values)
            {
                GetInt64List(fieldId, buffer);
            }

            return values;
        }

        public void GetInt64List(FieldId fieldId, Int64* buffer) => CSchema.GetInt64List(handle, fieldId, buffer);

        public Uint32[] GetUint32List(FieldId fieldId)
        {
            Uint32[] values = new Uint32[GetUint32Count(fieldId)];
            fixed (Uint32* buffer = values)
            {
                GetUint32List(fieldId, buffer);
            }

            return values;
        }

        public void GetUint32List(FieldId fieldId, Uint32* buffer) => CSchema.GetUint32List(handle, fieldId, buffer);

        public Uint64[] GetUint64List(FieldId fieldId)
        {
            Uint64[] values = new Uint64[GetUint64Count(fieldId)];
            fixed (Uint64* buffer = values)
            {
                GetUint64List(fieldId, buffer);
            }

            return values;
        }

        public void GetUint64List(FieldId fieldId, Uint64* buffer) => CSchema.GetUint64List(handle, fieldId, buffer);

        public Int32[] GetSint32List(FieldId fieldId)
        {
            Int32[] values = new Int32[GetSint32Count(fieldId)];
            fixed (Int32* buffer = values)
            {
                GetSint32List(fieldId, buffer);
            }

            return values;
        }

        public void GetSint32List(FieldId fieldId, Int32* buffer) => CSchema.GetSint32List(handle, fieldId, buffer);

        public Int64[] GetSint64List(FieldId fieldId)
        {
            Int64[] values = new Int64[GetSint64Count(fieldId)];
            fixed (Int64* buffer = values)
            {
                GetSint64List(fieldId, buffer);
            }

            return values;
        }

        public void GetSint64List(FieldId fieldId, Int64* buffer) => CSchema.GetSint64List(handle, fieldId, buffer);

        public Uint32[] GetFixed32List(FieldId fieldId)
        {
            Uint32[] values = new Uint32[GetFixed32Count(fieldId)];
            fixed (Uint32* buffer = values)
            {
                GetFixed32List(fieldId, buffer);
            }

            return values;
        }

        public void GetFixed32List(FieldId fieldId, Uint32* buffer) => CSchema.GetFixed32List(handle, fieldId, buffer);

        public Uint64[] GetFixed64List(FieldId fieldId)
        {
            Uint64[] values = new Uint64[GetFixed64Count(fieldId)];
            fixed (Uint64* buffer = values)
            {
                GetFixed64List(fieldId, buffer);
            }

            return values;
        }

        public void GetFixed64List(FieldId fieldId, Uint64* buffer) => CSchema.GetFixed64List(handle, fieldId, buffer);

        public Int32[] GetSfixed32List(FieldId fieldId)
        {
            Int32[] values = new Int32[GetSfixed32Count(fieldId)];
            fixed (Int32* buffer = values)
            {
                GetSfixed32List(fieldId, buffer);
            }

            return values;
        }

        public void GetSfixed32List(FieldId fieldId, Int32* buffer) => CSchema.GetSfixed32List(handle, fieldId, buffer);

        public Int64[] GetSfixed64List(FieldId fieldId)
        {
            Int64[] values = new Int64[GetSfixed64Count(fieldId)];
            fixed (Int64* buffer = values)
            {
                GetSfixed64List(fieldId, buffer);
            }

            return values;
        }

        public void GetSfixed64List(FieldId fieldId, Int64* buffer) => CSchema.GetSfixed64List(handle, fieldId, buffer);

        public EntityId[] GetEntityIdList(FieldId fieldId)
        {
            EntityId[] values = new EntityId[GetEntityIdCount(fieldId)];
            fixed (EntityId* buffer = values)
            {
                GetEntityIdList(fieldId, buffer);
            }

            return values;
        }

        public void GetEntityIdList(FieldId fieldId, EntityId* buffer) => CSchema.GetEntityIdList(handle, fieldId, buffer);

        public Uint32[] GetEnumList(FieldId fieldId)
        {
            Uint32[] values = new Uint32[GetEnumCount(fieldId)];
            fixed (Uint32* buffer = values)
            {
                GetEnumList(fieldId, buffer);
            }

            return values;
        }

        public void GetEnumList(FieldId fieldId, Uint32* buffer) => CSchema.GetEnumList(handle, fieldId, buffer);
    }
}
