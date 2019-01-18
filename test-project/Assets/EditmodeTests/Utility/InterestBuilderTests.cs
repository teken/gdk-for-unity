using Improbable.Gdk.QueryBasedInterest;
using NUnit.Framework;

namespace Improbable.Gdk.EditmodeTests.Utility
{
    [TestFixture]
    public class InterestBuilderTests
    {
        private static ComponentInterest.Query BasicQuery => InterestQuery.Query(Constraint.RelativeSphere(10));

        private static InterestBuilder BasicInterest => InterestBuilder.Begin();

        [Test]
        public void AddQueries_can_be_called_after_AddQuery()
        {
            Assert.DoesNotThrow(() => BasicInterest
                .AddQuery<Position.Component>(BasicQuery)
                .AddQueries<Position.Component>(BasicQuery, BasicQuery));
        }

        [Test]
        public void AddQueries_can_be_called_after_AddQueries()
        {
            Assert.DoesNotThrow(() => BasicInterest
                .AddQueries<Position.Component>(BasicQuery, BasicQuery)
                .AddQueries<Position.Component>(BasicQuery, BasicQuery));
        }

        [Test]
        public void AddQuery_can_be_called_after_AddQueries()
        {
            Assert.DoesNotThrow(() => BasicInterest
                .AddQueries<Position.Component>(BasicQuery, BasicQuery)
                .AddQuery<Position.Component>(BasicQuery));
        }

        [Test]
        public void AddQuery_can_be_called_after_AddQuery()
        {
            Assert.DoesNotThrow(() => BasicInterest
                .AddQuery<Position.Component>(BasicQuery)
                .AddQuery<Position.Component>(BasicQuery));
        }

        [Test]
        public void AddQueries_should_not_change_dictionary_if_no_queries_given()
        {
            var interest = BasicInterest.AddQueries<Position.Component>().Build();
            Assert.AreEqual(0, interest.ComponentInterest.Count);
        }

        [Test]
        public void Build_should_not_return_snapshot_with_null_interest()
        {
            var interest = BasicInterest.Build();
            Assert.IsNotNull(interest.ComponentInterest);
        }
    }
}
