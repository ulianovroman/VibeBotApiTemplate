using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace BotApiTemplate.Service
{
    public sealed class GptService : IGptService
    {
        private readonly GptOptions _options;
        private readonly ChatClient? _chatClient;

        public GptService(IOptions<GptOptions> options)
        {
            _options = options.Value;

            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _chatClient = new ChatClient(_options.Model ?? "gpt-4o-mini", _options.ApiKey);
            }
        }

        public async Task<string?> GenerateReplyAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (_chatClient is null)
            {
                return null;
            }

            var systemPrompt = """
                You are an assistant inside a Telegram bot backend.
                Keep answers concise, useful, and safe.
                Replace this system prompt with your own business-specific instructions.
                """;

            var userPrompt = $"""
                User request:
                {prompt}

                Replace this user prompt template with your own format and context fields.
                """;

            var completion = await _chatClient.CompleteChatAsync(
                [
                    ChatMessage.CreateSystemMessage(systemPrompt),
                    ChatMessage.CreateUserMessage(userPrompt)
                ],
                cancellationToken: cancellationToken);

            return completion.Value.Content.FirstOrDefault()?.Text;
        }
    }
}
