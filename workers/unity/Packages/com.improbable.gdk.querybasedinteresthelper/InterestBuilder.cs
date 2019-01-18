using System.Collections.Generic;
using Improbable.Gdk.Core;

namespace Improbable.Gdk.QueryBasedInterest
{

    public class InterestBuilder
    {
        private readonly Dictionary<uint, ComponentInterest> interest;

        private InterestBuilder()
        {
            interest = new Dictionary<uint, ComponentInterest>();
        }

        /// <summary>
        ///     Creates a new InterestBuilder object.
        /// </summary>
        /// <returns>
        ///     A new InterestBuilder object.
        /// </returns>
        public static InterestBuilder Begin()
        {
            return new InterestBuilder();
        }

        /// <summary>
        ///     Add queries to the Interest component.
        /// </summary>
        /// <param name="query">
        ///     First query to add for a given authoritative component.
        /// </param>
        /// <param name="queries">
        ///     Further queries to add for a given authoritative component.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the authoritative component to add the queries to.
        /// </typeparam>
        /// <remarks>
        ///     At least one query must be provided to update the Interest component.
        /// </remarks>
        /// <returns>
        ///     An InterestBuilder object.
        /// </returns>
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

        /// <summary>
        ///     Builds the Interest snapshot.
        /// </summary>
        /// <returns>
        ///     A Interest.Snapshot object.
        /// </returns>
        public Interest.Snapshot Build()
        {
            return new Interest.Snapshot { ComponentInterest = interest };
        }
    }
}
