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
            // Шаблон: сервис уже подключён к DI и конфигурации,
            // здесь будет вызов модели после наполнения бизнес-логикой.
            _ = prompt;
            _ = cancellationToken;
            _ = _options;

            return Task.FromResult<string?>(null);
        }
    }
}
