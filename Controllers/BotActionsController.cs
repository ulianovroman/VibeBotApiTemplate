using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using VibeBotApi.Dto;

namespace VibeBotApi.Controllers;

[ApiController]
[Route("api/bot/actions")]
public class BotActionsController : ControllerBase
{
    private readonly ITelegramBotClient _telegramBotClient;

    public BotActionsController(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendTextMessage(
        [FromBody] BotSendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var apiSecret = Environment.GetEnvironmentVariable(EnvironmentVariables.VibeBotApiSecret);

        if (request.VibeBotApiSecret != apiSecret)
        {
            return Unauthorized();
        }

        if (request.ChatId is null)
        {
            return BadRequest("ChatId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text is required.");
        }

        var sentMessage = await _telegramBotClient.SendMessage(
            chatId: request.ChatId.Value,
            text: request.Text,
            cancellationToken: cancellationToken);

        return Ok(new
        {
            sentMessage.Chat.Id,
            sentMessage.MessageId,
            sentMessage.Date
        });
    }
}
