using System.Collections.Generic;

namespace Improbable.Gdk.QueryBasedInterest
{
    public class InterestQuery
    {
        private static readonly List<uint> EmptyList = new List<uint>();

        private ComponentInterest.Query query;

        public static InterestQuery Query(ComponentInterest.QueryConstraint constraint)
        {
            var interest = new InterestQuery
            {
                query =
                {
                    Constraint = constraint,
                    FullSnapshotResult = true,
                    ResultComponentId = EmptyList
                }
            };
            return interest;
        }

        public InterestQuery WithMaxFrequencyHz(float frequency)
        {
            query.Frequency = frequency;
            return this;
        }

        public ComponentInterest.Query FilterResults(params uint[] resultComponentIds)
        {
            if (resultComponentIds.Length > 0)
            {
                query.FullSnapshotResult = null;
                query.ResultComponentId = new List<uint>(resultComponentIds);
            }

            return query;
        }

        public static implicit operator ComponentInterest.Query(InterestQuery interestQuery)
        {
            return interestQuery.query;
        }
    }
}
