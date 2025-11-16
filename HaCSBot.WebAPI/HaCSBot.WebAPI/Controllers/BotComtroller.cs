using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Telegram.Bot.Types;

namespace HaCSBot.WebAPI.Controllers
{
    [Controller]
    [Route("/")]
    public class BotComtroller : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post(Update update) 
        {
            return Ok();
        }
    }
}
