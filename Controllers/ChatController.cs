using MessangerServer.Data;
using MessangerServer.Models;
using MessangerServer.Models.FromClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MessangerServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private IHubContext<DataHub> hub; 
        public MessangerContext context;
        public ChatController(IHubContext<DataHub> hub, MessangerContext context)
        {
            this.hub = hub;
            this.context = context;
        }
        //[HttpGet]
        //[Route("/chat")]
        //public async Task<IActionResult> GetTest([FromQuery]ChatMessage chatMessage)
        //{
        //    await hub.Clients.All.SendAsync("ReceiveMessage", chatMessage.Content);
        //    return Ok();
        //}
        [HttpPost]
        public async Task<IActionResult> GetTest()
        {
            await hub.Clients.Group("Group1").SendAsync("ReceiveMessage", "privet");
            var currUser = await context.Users.Where(e => e.Id == Convert.ToInt32(2)).FirstOrDefaultAsync();
            var chats = await context.Chats.Include(e => e.Users).Where(e => e.Users.Contains(currUser)).ToListAsync();
            foreach (var item in chats)
            {
                await context.Messages.Where(e => e.ChatId == item.Id).OrderByDescending(e => e.DispatchTime).FirstOrDefaultAsync();
            }
            
            return Ok();
        }
      

    }
}
