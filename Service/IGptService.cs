namespace BotApiTemplate.Service
{
    public interface IGptService
    {
        Task<string?> GenerateReplyAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
