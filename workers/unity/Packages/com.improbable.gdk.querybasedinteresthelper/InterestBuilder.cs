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

        public InterestBuilder AddQueries<T>(params ComponentInterest.Query[] interestQueries)
            where T : ISpatialComponentData
        {
            if (interestQueries.Length == 0)
            {
                return this;
            }

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

        public InterestBuilder AddQuery<T>(ComponentInterest.Query interestQuery)
            where T : ISpatialComponentData
        {
            var componentId = Dynamic.GetComponentId<T>();
            if (!interest.ContainsKey(componentId))
            {
                interest.Add(componentId, new ComponentInterest
                {
                    Queries = new List<ComponentInterest.Query> { interestQuery }
                });
                return this;
            }

            interest[componentId].Queries.Add(interestQuery);
            return this;
        }

        public Interest.Snapshot Build()
        {
            return new Interest.Snapshot { ComponentInterest = interest };
        }
    }
}
