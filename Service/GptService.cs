using Microsoft.Extensions.Options;

namespace BotApiTemplate.Service
{
    public sealed class GptService : IGptService
    {
        private readonly GptOptions _options;

        public GptService(IOptions<GptOptions> options)
        {
            _options = options.Value;
        }

        public Task<string?> GenerateReplyAsync(string prompt, CancellationToken cancellationToken = default)
        {
            // Template: the service is already connected to DI and configuration,
            // the model call will be placed here after adding business logic.
            _ = prompt;
            _ = cancellationToken;
            _ = _options;

            return Task.FromResult<string?>(null);
        }
    }
}
