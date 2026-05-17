using System.Threading.RateLimiting;

namespace FactionAPI.Services.Api.RateLimiting;

// Enforces multiple rate limiters with AND semantics: all must allow the request.
public sealed class ChainedRateLimiter(RateLimiter[] inner) : RateLimiter
{
    public override TimeSpan? IdleDuration => inner.Min(l => l.IdleDuration);

    public override RateLimiterStatistics? GetStatistics() => null;

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        var leases = new List<RateLimitLease>(inner.Length);
        foreach (var limiter in inner)
        {
            var lease = limiter.AttemptAcquire(permitCount);
            if (!lease.IsAcquired)
            {
                foreach (var held in leases) held.Dispose();
                return lease;
            }
            leases.Add(lease);
        }
        return new AggregatedLease(leases);
    }

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        var leases = new List<RateLimitLease>(inner.Length);
        foreach (var limiter in inner)
        {
            var lease = await limiter.AcquireAsync(permitCount, cancellationToken);
            if (!lease.IsAcquired)
            {
                foreach (var held in leases) held.Dispose();
                return lease;
            }
            leases.Add(lease);
        }
        return new AggregatedLease(leases);
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        foreach (var l in inner) l.Dispose();
    }

    protected override ValueTask DisposeAsyncCore()
    {
        Dispose(true);
        return ValueTask.CompletedTask;
    }

    private sealed class AggregatedLease(List<RateLimitLease> leases) : RateLimitLease
    {
        public override bool IsAcquired => true;

        public override IEnumerable<string> MetadataNames => leases.SelectMany(l => l.MetadataNames);

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            foreach (var lease in leases)
                if (lease.TryGetMetadata(metadataName, out metadata)) return true;
            metadata = null;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                foreach (var l in leases) l.Dispose();
        }
    }
}
