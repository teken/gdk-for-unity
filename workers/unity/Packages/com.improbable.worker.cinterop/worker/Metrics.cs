using System.Collections.Generic;

namespace Improbable.Worker.CInterop
{
    public class HistogramMetric
    {
        internal List<Bucket> buckets = new List<Bucket>();
        internal double sum;

        public List<Bucket> Buckets
        {
            get { return buckets; }
        }

        public double Sum
        {
            get { return sum; }
            set { sum = value; }
        }

        public struct Bucket
        {
            public double UpperBound;
            public uint Samples;
        }

        public HistogramMetric(List<double> upperBounds)
        {
            foreach (var bound in upperBounds)
            {
                Bucket bucket;
                bucket.UpperBound = bound;
                bucket.Samples = 0;
                buckets.Add(bucket);
            }
            Bucket infBucket;
            infBucket.UpperBound = double.PositiveInfinity;
            infBucket.Samples = 0;
            buckets.Add(infBucket);
        }

        public HistogramMetric() : this(new List<double>()) {}

        public void ClearObservations()
        {
            for (var i = 0; i < buckets.Count; ++i)
            {
                var b = buckets[i];
                b.Samples = 0;
                buckets[i] = b;
            }
            sum = 0;
        }

        public void RecordObservation(double value)
        {
            for (var i = 0; i < buckets.Count; ++i)
            {
                if (value <= buckets[i].UpperBound)
                {
                    var b = buckets[i];
                    ++b.Samples;
                    buckets[i] = b;
                }
            }
            sum += value;
        }
    }

    public class Metrics
    {
        public void Merge(Metrics metrics)
        {
            if (metrics.Load.HasValue)
            {
                Load = metrics.Load.Value;
            }
            foreach (var entry in metrics.GaugeMetrics)
            {
                GaugeMetrics[entry.Key] = entry.Value;
            }
            foreach (var entry in metrics.HistogramMetrics)
            {
                HistogramMetrics[entry.Key] = entry.Value;
            }
        }

        public double? Load { set; get; }
        public Dictionary<string, double> GaugeMetrics { get; } = new Dictionary<string, double>();
        public Dictionary<string, HistogramMetric> HistogramMetrics { get; } = new Dictionary<string, HistogramMetric>();
    }
}
