using System.Collections.Generic;
using Improbable.Gdk.Core;
using Improbable.Worker.CInterop;

namespace Improbable.Gdk.QueryBasedInterest
{
    public class InterestBuilder
    {
        private readonly Dictionary<uint, ComponentInterest> interest;

        private InterestBuilder()
        {
            interest = new Dictionary<uint, ComponentInterest>();
        }

        public static InterestBuilder Begin()
        {
            return new InterestBuilder();
        }

        public InterestBuilder AddQueries<T>(ComponentInterest.Query query,
            params ComponentInterest.Query[] queries)
            where T : ISpatialComponentData
        {
            var interestQueries = new List<ComponentInterest.Query>(queries.Length + 1) {query};
            interestQueries.AddRange(queries);

            var componentId = Dynamic.GetComponentId<T>();
            if (!interest.ContainsKey(componentId))
            {
                interest.Add(componentId, new ComponentInterest
                {
                    Queries = new List<ComponentInterest.Query>(interestQueries)
                });
                return this;
            }

            interest[componentId].Queries.AddRange(interestQueries);
            return this;
        }

        public Interest.Snapshot Build()
        {
            return new Interest.Snapshot { ComponentInterest = interest };
        }
    }
}
