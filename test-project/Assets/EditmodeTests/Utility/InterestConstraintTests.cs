using Improbable.Gdk.Core;
using Improbable.Gdk.QueryBasedInterest;
using NUnit.Framework;
using UnityEngine;

namespace Improbable.Gdk.EditmodeTests.Utility
{
    [TestFixture]
    public class InterestConstraintTests
    {
        private const double Distance = 10;
        private readonly Vector3 _center = Vector3.zero;

        private static ComponentInterest.QueryConstraint BasicConstraint()
        {
            return Constraint.Component<Position.Component>();
        }

        [Test]
        public void All_constraint_sets_OrConstraint_to_empty_list()
        {
            var constraint = Constraint.All(BasicConstraint());
            Assert.True(constraint.AndConstraint.Count > 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void All_constraint_with_no_params_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.All();
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void Any_constraint_sets_AndConstraint_to_empty_list()
        {
            var constraint = Constraint.Any(BasicConstraint());
            Assert.True(constraint.OrConstraint.Count > 0);
            Assert.True(constraint.AndConstraint.Count == 0);
        }

        [Test]
        public void Any_constraint_with_no_params_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.Any();
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void Sphere_constraint_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.Sphere(Distance, _center);
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void Cylinder_constraint_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.Cylinder(Distance, _center);
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void Box_constraint_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.Box(Distance, Distance, Distance, _center);
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void RelativeSphere_constraint_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.RelativeSphere(Distance);
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void RelativeCylinder_constraint_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.RelativeCylinder(Distance);
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void EntityId_constraint_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.EntityId(new EntityId(10));
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void Component_constraint_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.Component(Position.ComponentId);
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }

        [Test]
        public void Component_T__constraint_sets_AndOr_constraints_to_empty_list()
        {
            var constraint = Constraint.Component<Position.Component>();
            Assert.True(constraint.AndConstraint.Count == 0);
            Assert.True(constraint.OrConstraint.Count == 0);
        }
    }
}
