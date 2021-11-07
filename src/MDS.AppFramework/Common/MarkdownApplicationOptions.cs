using MDS.AspnetServices.Common;
using Microsoft.Extensions.Options;

namespace MDS.AppFramework.Common
{
    public class MarkdownApplicationOptions : IOptions<MarkdownApplicationConfiguration>
    {
        public MarkdownApplicationOptions(IServiceProvider services)
        {
            Services = services;
            Current = this;
        }

        public MarkdownApplicationOptions(
            IServiceProvider services,
            MarkdownApplicationConfiguration? config) : this(services)
        {
            if (config is not null)
            {
                Value = config;
            }
        }

        public MarkdownApplicationOptions(
            IServiceProvider services,
            string json) : this(services)
        {
            if (json is null or "")
            {
                throw new AbandonedMutexException(nameof(json));
            }

            var config = json.FromJson<MarkdownApplicationConfiguration>();

            if (config is not null)
            {
                Value = config;
            }
        }
        public MarkdownApplicationConfiguration Value { get; } = new();

        public static MarkdownApplicationOptions? Current { get; private set; }
        private string? _serverRoot;

        public string? ServerRoot
        {
            get => _serverRoot;
            set => _serverRoot = value;
        }

        public IServiceProvider Services { get; set; }
    }
}