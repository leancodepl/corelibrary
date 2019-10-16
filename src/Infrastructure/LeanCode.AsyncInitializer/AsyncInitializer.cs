using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.AsyncInitializer
{
    public sealed class AsyncInitializer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<AsyncInitializer>();

        private readonly IEnumerable<IAsyncInitializable> allInits;

        public AsyncInitializer(IEnumerable<IAsyncInitializable> allInits)
        {
            this.allInits = allInits.OrderBy(i => i.Order).ToList();
        }

        public async ValueTask InitializeAsync(CancellationToken token)
        {
            logger.Information("Initializing async modules");

            foreach (var i in allInits)
            {
                if (token.IsCancellationRequested)
                {
                    logger.Information("Cancellation requested, skipping initialization");

                    break;
                }

                logger.Debug("Initializing {Type}", i.GetType());

                await i.InitializeAsync();
            }
        }

        public async ValueTask DeinitializeAsync(CancellationToken token)
        {
            logger.Information("Disposing async modules");

            foreach (var i in allInits.Reverse())
            {
                if (token.IsCancellationRequested)
                {
                    logger.Warning("Cancellation required, skipping rest of the disposals");

                    break;
                }

                logger.Debug("Disposing {Type}", i.GetType());

                await i.DeinitializeAsync();
            }
        }
    }
}
