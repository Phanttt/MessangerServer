using MessangerServer.Data;
using MessangerServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> GetTest(int currUserId)
        {
            //var chatsq = await context.AttachmentUserChats.Where(e => e.UserId == currUserId).Include(e => e.Chat).Include(e=>e.User).ToListAsync();
            var currUser = await context.Users.Where(e=>e.Id==currUserId).FirstOrDefaultAsync();
            var chats2 = await context.Chats.Include(e => e.Users).Where(e=>e.Users.Contains(currUser)).ToListAsync();
            //var users = await context.Users.ToListAsync();
            //await hub.Clients.All.SendAsync("ReceiveMessage", chatMessage.Content);
            return Ok();
        }
    }
}
