using System.Collections.Generic;
using System.Linq;
using Improbable.Gdk.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace Improbable.Gdk.QueryBasedInterest
{
    public static class Constraint
    {
        private static readonly List<ComponentInterest.QueryConstraint> EmptyList
            = new List<ComponentInterest.QueryConstraint>();

        private static ComponentInterest.QueryConstraint Default()
        {
            return new ComponentInterest.QueryConstraint
            {
                AndConstraint = EmptyList,
                OrConstraint = EmptyList
            };
        }

        public static ComponentInterest.QueryConstraint Sphere(double radius, Vector3 center)
        {
            var constraint = Default();
            constraint.SphereConstraint = new ComponentInterest.SphereConstraint
            {
                Center = new Coordinates(center.x, center.y, center.z),
                Radius = radius
            };
            return constraint;
        }

        public static ComponentInterest.QueryConstraint Cylinder(double radius, Vector3 center)
        {
            var constraint = Default();
            constraint.CylinderConstraint = new ComponentInterest.CylinderConstraint()
            {
                Center = new Coordinates(center.x, center.y, center.z),
                Radius = radius
            };
            return constraint;
        }

        public static ComponentInterest.QueryConstraint Box(double xLength, double yLength, double zLength, Vector3 center)
        {
            var constraint = Default();
            constraint.BoxConstraint = new ComponentInterest.BoxConstraint
            {
                Center = new Coordinates(center.x, center.y, center.z),
                EdgeLength = new EdgeLength(xLength, yLength, zLength)
            };
            return constraint;
        }

        public static ComponentInterest.QueryConstraint RelativeSphere(double radius)
        {
            var constraint = Default();
            constraint.RelativeSphereConstraint = new ComponentInterest.RelativeSphereConstraint
            {
                Radius = radius
            };
            return constraint;
        }

        public static ComponentInterest.QueryConstraint RelativeCylinder(double radius)
        {
            var constraint = Default();
            constraint.RelativeCylinderConstraint = new ComponentInterest.RelativeCylinderConstraint
            {
                Radius = radius
            };
            return constraint;
        }

        public static ComponentInterest.QueryConstraint RelativeBox(double xLength, double yLength, double zLength)
        {
            var constraint = Default();
            constraint.RelativeBoxConstraint = new ComponentInterest.RelativeBoxConstraint
            {
                EdgeLength = new EdgeLength(xLength, yLength, zLength)
            };
            return constraint;
        }

        public static ComponentInterest.QueryConstraint EntityId(EntityId entityId)
        {
            var constraint = Default();
            constraint.EntityIdConstraint = entityId.Id;
            return constraint;
        }

        public static ComponentInterest.QueryConstraint Component<T>() where T : ISpatialComponentData
        {
            var constraint = Default();
            constraint.ComponentConstraint = Dynamic.GetComponentId<T>();
            return constraint;
        }

        public static ComponentInterest.QueryConstraint Component(uint componentId)
        {
            var constraint = Default();
            constraint.ComponentConstraint = componentId;
            return constraint;
        }

        public static ComponentInterest.QueryConstraint All(ComponentInterest.QueryConstraint constraint,
            params ComponentInterest.QueryConstraint[] constraints)
        {
            var andConstraints = new List<ComponentInterest.QueryConstraint>(constraints.Length + 1) {constraint};
            andConstraints.AddRange(constraints);

            return new ComponentInterest.QueryConstraint
            {
                AndConstraint = andConstraints,
                OrConstraint = EmptyList
            };
        }

        public static ComponentInterest.QueryConstraint Any(ComponentInterest.QueryConstraint constraint,
            params ComponentInterest.QueryConstraint[] constraints)
        {
            var orConstraints = new List<ComponentInterest.QueryConstraint>(constraints.Length + 1) {constraint};
            orConstraints.AddRange(constraints);

            return new ComponentInterest.QueryConstraint
            {
                AndConstraint = EmptyList,
                OrConstraint = orConstraints
            };
        }
    }
}
