using System.Runtime.InteropServices;
using FieldId = System.UInt32;
using EntityId = System.Int64;
using Uint64 = System.UInt64;
using Uint32 = System.UInt32;
using Uint8 = System.Byte;
using Int64 = System.Int64;
using Int32 = System.Int32;
using Char = System.Byte;

// This file must match the C API (worker_sdk/c/include/improbable/c_schema.h) *exactly*!
namespace Improbable.Worker.CInterop.Internal
{
    internal unsafe class CSchema
    {
        public struct Object {}
        public struct CommandRequest {}
        public struct CommandResponse {}
        public struct ComponentData {}
        public struct ComponentUpdate {}

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_CreateCommandRequest")]
        public static extern CommandRequest* CreateCommandRequest(FieldId component_id, FieldId command_index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_DestroyCommandRequest")]
        public static extern void DestroyCommandRequest(CommandRequest* request);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetCommandRequestComponentId")]
        public static extern FieldId GetCommandRequestComponentId(CommandRequest* request);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetCommandRequestCommandIndex")]
        public static extern FieldId GetCommandRequestCommandIndex(CommandRequest* request);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetCommandRequestObject")]
        public static extern Object* GetCommandRequestObject(CommandRequest* request);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_CreateCommandResponse")]
        public static extern CommandResponse* CreateCommandResponse(FieldId component_id, FieldId command_index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_DestroyCommandResponse")]
        public static extern void DestroyCommandResponse(CommandResponse* response);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetCommandResponseComponentId")]
        public static extern FieldId GetCommandResponseComponentId(CommandResponse* response);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetCommandResponseCommandIndex")]
        public static extern FieldId GetCommandResponseCommandIndex(CommandResponse* response);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetCommandResponseObject")]
        public static extern Object* GetCommandResponseObject(CommandResponse* response);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_CreateComponentData")]
        public static extern ComponentData* CreateComponentData(FieldId component_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_DestroyComponentData")]
        public static extern void DestroyComponentData(ComponentData* data);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetComponentDataComponentId")]
        public static extern FieldId GetComponentDataComponentId(ComponentData* data);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetComponentDataFields")]
        public static extern Object* GetComponentDataFields(ComponentData* data);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_CreateComponentUpdate")]
        public static extern ComponentUpdate* CreateComponentUpdate(FieldId component_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_DestroyComponentUpdate")]
        public static extern void DestroyComponentUpdate(ComponentUpdate* update);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetComponentUpdateComponentId")]
        public static extern FieldId GetComponentUpdateComponentId(ComponentUpdate* update);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetComponentUpdateFields")]
        public static extern Object* GetComponentUpdateFields(ComponentUpdate* update);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetComponentUpdateEvents")]
        public static extern Object* GetComponentUpdateEvents(ComponentUpdate* update);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_ClearComponentUpdateClearedFields")]
        public static extern void ClearComponentUpdateClearedFields(ComponentUpdate* update);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddComponentUpdateClearedField")]
        public static extern void AddComponentUpdateClearedField(ComponentUpdate* update, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetComponentUpdateClearedFieldCount")]
        public static extern Uint32 GetComponentUpdateClearedFieldCount(ComponentUpdate* update);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexComponentUpdateClearedField")]
        public static extern FieldId IndexComponentUpdateClearedField(ComponentUpdate* update, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetComponentUpdateClearedFieldList")]
        public static extern void GetComponentUpdateClearedFieldList(ComponentUpdate* update, FieldId* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetError")]
        public static extern Char* GetError(Object* obj);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_Clear")]
        public static extern void Clear(Object* obj);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_ClearField")]
        public static extern void ClearField(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_ShallowCopy")]
        public static extern void ShallowCopy(Object* src, Object* dst);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_ShallowCopyField")]
        public static extern void ShallowCopyField(Object* src, Object* dst, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AllocateObject")]
        public static extern Object* AllocateObject(Object* obj);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AllocateBuffer")]
        public static extern Uint8* AllocateBuffer(Object* obj, Uint32 length);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_MergeFromBuffer")]
        public static extern Uint8 MergeFromBuffer(Object* obj, Uint8* buffer, Uint32 length);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetWriteBufferLength")]
        public static extern Uint32 GetWriteBufferLength(Object* obj);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_WriteToBuffer")]
        public static extern Uint8 WriteToBuffer(Object* obj, Uint8* buffer);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_SerializeToBuffer")]
        public static extern Uint8 SerializeToBuffer(Object* obj, Uint8* buffer, Uint32 length);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetUniqueFieldIdCount")]
        public static extern Uint32 GetUniqueFieldIdCount(Object* obj);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetUniqueFieldIds")]
        public static extern void GetUniqueFieldIds(Object* obj, Uint32* buffer);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddFloat")]
        public static extern void AddFloat(Object* obj, FieldId field_id, float value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddDouble")]
        public static extern void AddDouble(Object* obj, FieldId field_id, double value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddBool")]
        public static extern void AddBool(Object* obj, FieldId field_id, Uint8 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddInt32")]
        public static extern void AddInt32(Object* obj, FieldId field_id, Int32 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddInt64")]
        public static extern void AddInt64(Object* obj, FieldId field_id, Int64 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddUint32")]
        public static extern void AddUint32(Object* obj, FieldId field_id, Uint32 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddUint64")]
        public static extern void AddUint64(Object* obj, FieldId field_id, Uint64 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddSint32")]
        public static extern void AddSint32(Object* obj, FieldId field_id, Int32 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddSint64")]
        public static extern void AddSint64(Object* obj, FieldId field_id, Int64 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddFixed32")]
        public static extern void AddFixed32(Object* obj, FieldId field_id, Uint32 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddFixed64")]
        public static extern void AddFixed64(Object* obj, FieldId field_id, Uint64 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddSfixed32")]
        public static extern void AddSfixed32(Object* obj, FieldId field_id, Int32 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddSfixed64")]
        public static extern void AddSfixed64(Object* obj, FieldId field_id, Int64 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddEntityId")]
        public static extern void AddEntityId(Object* obj, FieldId field_id, EntityId value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddEnum")]
        public static extern void AddEnum(Object* obj, FieldId field_id, Uint32 value);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddBytes")]
        public static extern void AddBytes(Object* obj, FieldId field_id, Uint8* buffer, Uint32 length);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddObject")]
        public static extern Object* AddObject(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddFloatList")]
        public static extern void AddFloatList(Object* obj, FieldId field_id, float* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddDoubleList")]
        public static extern void AddDoubleList(Object* obj, FieldId field_id, double* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddBoolList")]
        public static extern void AddBoolList(Object* obj, FieldId field_id, Uint8* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddInt32List")]
        public static extern void AddInt32List(Object* obj, FieldId field_id, Int32* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddInt64List")]
        public static extern void AddInt64List(Object* obj, FieldId field_id, Int64* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddUint32List")]
        public static extern void AddUint32List(Object* obj, FieldId field_id, Uint32* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddUint64List")]
        public static extern void AddUint64List(Object* obj, FieldId field_id, Uint64* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddSint32List")]
        public static extern void AddSint32List(Object* obj, FieldId field_id, Int32* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddSint64List")]
        public static extern void AddSint64List(Object* obj, FieldId field_id, Int64* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddFixed32List")]
        public static extern void AddFixed32List(Object* obj, FieldId field_id, Uint32* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddFixed64List")]
        public static extern void AddFixed64List(Object* obj, FieldId field_id, Uint64* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddSfixed32List")]
        public static extern void AddSfixed32List(Object* obj, FieldId field_id, Int32* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddSfixed64List")]
        public static extern void AddSfixed64List(Object* obj, FieldId field_id, Int64* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddEntityIdList")]
        public static extern void AddEntityIdList(Object* obj, FieldId field_id, EntityId* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_AddEnumList")]
        public static extern void AddEnumList(Object* obj, FieldId field_id, Uint32* values, Uint32 count);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetFloatCount")]
        public static extern Uint32 GetFloatCount(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetDoubleCount")]
        public static extern Uint32 GetDoubleCount(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetBoolCount")]
        public static extern Uint32 GetBoolCount(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetInt32Count")]
        public static extern Uint32 GetInt32Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetInt64Count")]
        public static extern Uint32 GetInt64Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetUint32Count")]
        public static extern Uint32 GetUint32Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetUint64Count")]
        public static extern Uint32 GetUint64Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSint32Count")]
        public static extern Uint32 GetSint32Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSint64Count")]
        public static extern Uint32 GetSint64Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetFixed32Count")]
        public static extern Uint32 GetFixed32Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetFixed64Count")]
        public static extern Uint32 GetFixed64Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSfixed32Count")]
        public static extern Uint32 GetSfixed32Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSfixed64Count")]
        public static extern Uint32 GetSfixed64Count(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetEntityIdCount")]
        public static extern Uint32 GetEntityIdCount(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetEnumCount")]
        public static extern Uint32 GetEnumCount(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetBytesCount")]
        public static extern Uint32 GetBytesCount(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetObjectCount")]
        public static extern Uint32 GetObjectCount(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetFloat")]
        public static extern float GetFloat(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetDouble")]
        public static extern double GetDouble(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetBool")]
        public static extern Uint8 GetBool(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetInt32")]
        public static extern Int32 GetInt32(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetInt64")]
        public static extern Int64 GetInt64(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetUint32")]
        public static extern Uint32 GetUint32(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetUint64")]
        public static extern Uint64 GetUint64(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSint32")]
        public static extern Int32 GetSint32(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSint64")]
        public static extern Int64 GetSint64(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetFixed32")]
        public static extern Uint32 GetFixed32(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetFixed64")]
        public static extern Uint64 GetFixed64(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSfixed32")]
        public static extern Int32 GetSfixed32(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSfixed64")]
        public static extern Int64 GetSfixed64(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetEntityId")]
        public static extern EntityId GetEntityId(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetEnum")]
        public static extern Uint32 GetEnum(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetBytesLength")]
        public static extern Uint32 GetBytesLength(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetBytes")]
        public static extern Uint8* GetBytes(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetObject")]
        public static extern Object* GetObject(Object* obj, FieldId field_id);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexFloat")]
        public static extern float IndexFloat(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexDouble")]
        public static extern double IndexDouble(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexBool")]
        public static extern Uint8 IndexBool(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexInt32")]
        public static extern Int32 IndexInt32(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexInt64")]
        public static extern Int64 IndexInt64(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexUint32")]
        public static extern Uint32 IndexUint32(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexUint64")]
        public static extern Uint64 IndexUint64(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexSint32")]
        public static extern Int32 IndexSint32(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexSint64")]
        public static extern Int64 IndexSint64(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexFixed32")]
        public static extern Uint32 IndexFixed32(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexFixed64")]
        public static extern Uint64 IndexFixed64(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexSfixed32")]
        public static extern Int32 IndexSfixed32(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexSfixed64")]
        public static extern Int64 IndexSfixed64(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexEntityId")]
        public static extern EntityId IndexEntityId(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexEnum")]
        public static extern Uint32 IndexEnum(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexBytesLength")]
        public static extern Uint32 IndexBytesLength(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexBytes")]
        public static extern Uint8* IndexBytes(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_IndexObject")]
        public static extern Object* IndexObject(Object* obj, FieldId field_id, Uint32 index);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetFloatList")]
        public static extern void GetFloatList(Object* obj, FieldId field_id, float* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetDoubleList")]
        public static extern void GetDoubleList(Object* obj, FieldId field_id, double* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetBoolList")]
        public static extern void GetBoolList(Object* obj, FieldId field_id, Uint8* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetInt32List")]
        public static extern void GetInt32List(Object* obj, FieldId field_id, Int32* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetInt64List")]
        public static extern void GetInt64List(Object* obj, FieldId field_id, Int64* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetUint32List")]
        public static extern void GetUint32List(Object* obj, FieldId field_id, Uint32* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetUint64List")]
        public static extern void GetUint64List(Object* obj, FieldId field_id, Uint64* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSint32List")]
        public static extern void GetSint32List(Object* obj, FieldId field_id, Int32* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSint64List")]
        public static extern void GetSint64List(Object* obj, FieldId field_id, Int64* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetFixed32List")]
        public static extern void GetFixed32List(Object* obj, FieldId field_id, Uint32* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetFixed64List")]
        public static extern void GetFixed64List(Object* obj, FieldId field_id, Uint64* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSfixed32List")]
        public static extern void GetSfixed32List(Object* obj, FieldId field_id, Int32* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetSfixed64List")]
        public static extern void GetSfixed64List(Object* obj, FieldId field_id, Int64* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetEntityIdList")]
        public static extern void GetEntityIdList(Object* obj, FieldId field_id, EntityId* output_array);

        [DllImport(Constants.WorkerLibrary, CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "Schema_GetEnumList")]
        public static extern void GetEnumList(Object* obj, FieldId field_id, Uint32* output_array);
    }
}
