﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using VLSU.ScheduleTelegramBot.Domain.Options;

namespace VLSU.ScheduleTelegramBot.API;

[ApiController]
[Route("")]
public class BotController : ControllerBase
{
	private readonly IOptions<BotOptions> _options;

    public BotController(IOptions<BotOptions> options)
    {
        _options = options;
    }

    [HttpPost]
	public async Task<IActionResult> HandleUpdate([FromBody] Update update, [FromServices] ITelegramBotClient bot, [FromServices] UpdateHandler handleUpdateService, CancellationToken ct)
	{
		if (Request.Headers["X-Telegram-Bot-Api-Secret-Token"] != _options.Value.SecretToken)
			return Forbid();

		try
		{
			await handleUpdateService.HandleUpdateAsync(bot, update, ct);
		}
		catch (Exception exception)
		{
			await handleUpdateService.HandleErrorAsync(bot, exception, Telegram.Bot.Polling.HandleErrorSource.HandleUpdateError, ct);
		}

		return Ok();
	}
}
