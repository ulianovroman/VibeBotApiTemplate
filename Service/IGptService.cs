using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using BotApiTemplate.Dto;

namespace BotApiTemplate.Service
{
    public interface IGptService
    {
        Task<string> AskAsync(string prompt);
        Task<CardDto> GetCardData(string nativeLang, string studyLang, string word);
    }

    public class GptService(OpenAIClient _client) : IGptService
    {
        public async Task<CardDto> GetCardData(string nativeLang, string studyLang, string word)
        {
            var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4.1-mini";
            var chat = _client.GetChatClient(model);
            var systemPrompt = @"
You generate bilingual flashcard data.

Rules:
- Detect language of Word. Word can be a single word or a short phrase.
- If Word is in NativeLang, set t to StudyLang translation.
- If Word is in StudyLang, set t to NativeLang translation.
- pos, m, forms and ex must always be in StudyLang.
- ex should contain 1-2 short StudyLang examples with Word or its translation in context.
- Output ONLY valid JSON without markdown fences or comments.
- Allowed keys only: t, pos, m, forms, ex.
- forms and ex must be arrays (empty if unknown).
- t, pos, m can be null when unknown.
";

            var userPrompt = $@"
'NativeLang': {nativeLang}
'StudyLang': {studyLang}
'Word': {word}
";

            var response = await chat.CompleteChatAsync(
                [
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                ]);

            var rawResponse = response.Value.Content[0].Text;
            var normalizedJson = TryExtractJsonObject(rawResponse);

            CardDto? card;
            try
            {
                card = JsonSerializer.Deserialize<CardDto>(normalizedJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse GPT card JSON. Raw response: {rawResponse}", ex);
            }

            if (card is null)
            {
                throw new InvalidOperationException($"OpenAI returned empty card payload. Raw response: {rawResponse}");
            }

            card.forms ??= [];
            card.ex ??= [];

            return card;
        }

        private static string TryExtractJsonObject(string rawResponse)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                return rawResponse;
            }

            var trimmed = rawResponse.Trim();

            if (trimmed.StartsWith("```") && trimmed.EndsWith("```"))
            {
                var firstNewLine = trimmed.IndexOf('\n');
                if (firstNewLine >= 0)
                {
                    trimmed = trimmed[(firstNewLine + 1)..];
                }

                var lastFence = trimmed.LastIndexOf("```");
                if (lastFence >= 0)
                {
                    trimmed = trimmed[..lastFence];
                }

                trimmed = trimmed.Trim();
            }

            var firstBrace = trimmed.IndexOf('{');
            var lastBrace = trimmed.LastIndexOf('}');

            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                return trimmed.Substring(firstBrace, lastBrace - firstBrace + 1);
            }

            return trimmed;
        }

        public async Task<string> AskAsync(string prompt)
        {
            var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4.1-mini";
            var chat = _client.GetChatClient(model);

            var response = await chat.CompleteChatAsync(
                new UserChatMessage(prompt)
            );

            return response.Value.Content[0].Text;
        }
    }
}
