using Telegram.Bot.Types;

namespace BotApiTemplate.UpdateChainOfResponsibility
{
    public interface IUpdateHandler
    {
        Task HandleAsync(Update update, UpdateContext context, Func<Task> next, CancellationToken ct);
    }
}
