using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotApiTemplate.UpdateChainOfResponsibility;

namespace BotApiTemplate.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramWebhookController : ControllerBase
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly UpdateProcessor _processor;

    public TelegramWebhookController(
        ITelegramBotClient telegramBotClient,
        UpdateProcessor processor)
    {
        _telegramBotClient = telegramBotClient;
        _processor = processor;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook(
        [FromBody] Update update,
        [FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string? headerSecret,
        CancellationToken cancellationToken)
    {
        var secret = Environment.GetEnvironmentVariable(EnvironmentVariables.TelegramWebhookSecret);

        if (headerSecret != secret)
        {
            return Unauthorized();
        }

        await _processor.ProcessAsync(update, cancellationToken);
        return Ok();
    }

    [HttpGet("webhook/settings")]
    public async Task<IActionResult> GetWebhookSettings(CancellationToken cancellationToken)
    {
        var result = await _telegramBotClient.GetWebhookInfo(cancellationToken);

        return Ok(result);
    }
}
