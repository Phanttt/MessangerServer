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
            await hub.Clients.User("qwer").SendAsync("ReceiveMessage", "qwer");
            return Ok();
        }
    }
}
