using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CrawlScope.Application.Abstractions.Crawling.Services;

namespace CrawlScope.Infrastructure.BackgroundJobs
{
    public class CrawlJobChannel : ICrawlJobChannel
    {
        private readonly Channel<Guid> _channel;

        public CrawlJobChannel()
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<Guid>(options);
        }

        public async Task AddCrawlJobAsync(Guid crawlJobId, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(crawlJobId, cancellationToken);
        }

        public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
