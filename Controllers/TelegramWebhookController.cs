using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotApiTemplate.UpdateChainOfResponsibility;

namespace BotApiTemplate.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramWebhookController : ControllerBase
{
    private readonly ILogger<TelegramWebhookController> _logger;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly UpdateProcessor _processor;

    public TelegramWebhookController(
        ILogger<TelegramWebhookController> logger,
        ITelegramBotClient telegramBotClient,
        UpdateProcessor processor)
    {
        _logger = logger;
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

    [HttpGet("getWebhookSettings")]
    public async Task<IActionResult> GetWebhookSettings(CancellationToken cancellationToken)
    {      
        var result = await _telegramBotClient.GetWebhookInfo(cancellationToken);

        return Ok(result);
    }

    //[HttpDelete("deleteWebhookSettings")]
    //public async Task<IActionResult> DeleteWebhookSettings(CancellationToken cancellationToken)
    //{
    //    if (_telegramBotClient is null)
    //    {
    //        return Problem("TELEGRAM_BOT_TOKEN is not configured");
    //    }

    //    await _telegramBotClient.DeleteWebhook(cancellationToken: cancellationToken);
    //    return Ok();
    //}
}