using System.Collections.Generic;
using EntityId = System.Int64;

namespace Improbable.Worker.CInterop.Query
{
    /// <summary>Base class for entity query constraints.</summary>
    public interface IConstraint {}

    public class EntityIdConstraint : IConstraint
    {
        public EntityIdConstraint(EntityId entityId)
        {
            EntityId = entityId;
        }
        public EntityId EntityId;
    }

    public class ComponentConstraint : IConstraint
    {
        public ComponentConstraint(uint componentId)
        {
            ComponentId = componentId;
        }
        public uint ComponentId;
    }

    public class SphereConstraint : IConstraint
    {
        public SphereConstraint(double x, double y, double z, double radius)
        {
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
        }
        public double X;
        public double Y;
        public double Z;
        public double Radius;
    }

    public class AndConstraint : List<IConstraint>, IConstraint
    {
        public AndConstraint() {}
        public AndConstraint(IEnumerable<IConstraint> enumerator) : base(enumerator) {}
    }

    public class OrConstraint : List<IConstraint>, IConstraint
    {
        public OrConstraint() {}
        public OrConstraint(IEnumerable<IConstraint> enumerator) : base(enumerator) {}
    }

    public class NotConstraint : IConstraint
    {
        public NotConstraint() {}
        public NotConstraint(IConstraint constraint)
        {
            Constraint = constraint;
        }
        public IConstraint Constraint;
    }

    public interface IResultType {}

    public class CountResultType : IResultType {}

    public class SnapshotResultType : IResultType
    {
        public SnapshotResultType() {}
        public SnapshotResultType(List<uint> componentIds)
        {
            ComponentIds = componentIds;
        }
        /// <summary>
        /// If nonempty, filters the components returned in the snapshot for each entity.
        /// </summary>
        public List<uint> ComponentIds;
    }

    public struct EntityQuery
    {
        public IConstraint Constraint;
        public IResultType ResultType;
    }
}
