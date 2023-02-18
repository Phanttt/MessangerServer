using MessangerServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MessangerServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private IHubContext<DataHub> hub;
        public ChatController(IHubContext<DataHub> hub)
        {
            this.hub = hub;
        }
        //[HttpGet]
        //[Route("/chat")]
        //public async Task<IActionResult> GetTest([FromQuery]ChatMessage chatMessage)
        //{
        //    await hub.Clients.All.SendAsync("ReceiveMessage", chatMessage.Content);
        //    return Ok();
        //}
        public async Task<IActionResult> GetTest(ChatMessage chatMessage)
        {
            await hub.Clients.All.SendAsync("ReceiveMessage", chatMessage.Content);
            return Ok();
        }
    }
}
