using BotApiTemplate.Storage;
using Telegram.Bot.Types;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public sealed class LoggingHandler : IUpdateHandler
    {
        private readonly BotContext _context;

        public LoggingHandler(BotContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct)
        {
            Console.WriteLine($"Incoming tg update! UpdateId: {update.Id}, Type: {update.Type}");

            var messageText = update.Message?.Text;
            if (!string.IsNullOrWhiteSpace(messageText) && update.Message?.From is not null)
            {
                var textToStore = messageText.Length <= 256
                    ? messageText
                    : messageText[..256];

                _context.MessageLogs.Add(new MessageLog
                {
                    UpdateId = update.Id,
                    MessageId = update.Message.Id,
                    ChatId = update.Message.Chat.Id,
                    SenderUserId = update.Message.From.Id,
                    SenderUsername = update.Message.From.Username,
                    Text = textToStore,
                    Received = update.Message.Date.ToUniversalTime(),
                    LoggedAt = DateTime.UtcNow,
                });

                await _context.SaveChangesAsync(ct);
            }

            await next();
        }
    }
}
