using System;
using System.Threading;

namespace Hydra.Core
{
    public static class HydraContext
    {
        private static readonly AsyncLocal<Guid?> _correlationId = new();
        private static readonly AsyncLocal<Guid?> _platformId = new();

        public static Guid? CorrelationId
        {
            get => _correlationId.Value;
            set => _correlationId.Value = value;
        }

        public static Guid? PlatformId
        {
            get => _platformId.Value;
            set => _platformId.Value = value;
        }

        public static void Set(Guid? correlationId, Guid? platformId)
        {
            CorrelationId = correlationId;
            PlatformId = platformId;
        }

        public static void Clear()
        {
            CorrelationId = null;
            PlatformId = null;
        }
    }
}
