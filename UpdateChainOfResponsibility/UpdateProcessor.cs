using Telegram.Bot.Types;

namespace VibeBotApi.UpdateChainOfResponsibility
{
    public class UpdateProcessor
    {
        private readonly IUpdateHandler[] _handlers;

        public UpdateProcessor(IEnumerable<IUpdateHandler> handlers)
        {
            _handlers = handlers.ToArray();
        }

        public Task ProcessAsync(Update update, CancellationToken ct)
        {
            var context = new UpdateContext();
            var index = 0;

            Task Next()
            {
                if (index >= _handlers.Length)
                    return Task.CompletedTask;

                var handler = _handlers[index++];
                return handler.HandleAsync(update, context, Next, ct);
            }

            return Next();
        }
    }
}
