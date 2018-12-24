using System;
using System.Collections.Generic;

namespace Improbable.Worker.CInterop
{
    public class Entity
    {
        internal readonly Dictionary<uint, ComponentData> components = new Dictionary<uint, ComponentData>();

        public ComponentData? Get(uint componentId)
        {
            ComponentData data;
            return components.TryGetValue(componentId, out data) ? data : (ComponentData?) null;
        }

        public void Add(ComponentData data)
        {
            if (!components.ContainsKey(data.ComponentId))
            {
                components.Add(data.ComponentId, data);
            }
        }

        public void Remove(uint componentId)
        {
            components.Remove(componentId);
        }

        public Dictionary<uint, ComponentData>.KeyCollection GetComponentIds()
        {
            return components.Keys;
        }
    }
}
