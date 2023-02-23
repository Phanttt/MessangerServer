
using MessangerServer.Models;
using Microsoft.AspNetCore.SignalR;
using MessangerServer.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity.Validation;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;

namespace MessangerServer.Controllers
{
    public class DataHub : Hub
    {
        private MessangerContext context;
        public DataHub(MessangerContext context)
        {
            this.context = context;
            ContextInitializer.Init(this.context);
        }
        public async Task SendMessage(ChatMessage message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined the group {groupName}.");
        }
        public async Task GetChats(string currUserId)
        {
            var currUser = await context.Users.Where(e => e.Id == Convert.ToInt32(currUserId)).FirstOrDefaultAsync();
            var chats = await context.Chats.Include(e => e.Users).Where(e => e.Users.Contains(currUser)).ToListAsync();
            await Clients.Caller.SendAsync("GetChats", chats);
        }
        public async Task GetCurrentChat(int currchatId)
        {
            var currchat = await context.Chats.Include(e => e.Users).Include(e => e.Messages).FirstOrDefaultAsync(e => e.Id == currchatId);
            //currchat.Users = currchat.Users.Where(e => e.Id! == userId).ToList();
            await Clients.Caller.SendAsync("CurrentChat", currchat);
        }
        public async Task Login(string login, string password)
        {
           
            var user = await context.Users.FirstOrDefaultAsync(e => e.Login == login && e.Password == password);
            if (user != null)
            {
                string encodedJwt = CreateToken(user);
                await Clients.Caller.SendAsync("JwtToken", encodedJwt);
            }
            else
            {
                await Clients.Caller.SendAsync("JwtToken", null);
            }
            
        }
        public async Task Register(string login, string name, string password)
        {
            User user = new User()
            {
                Login = login,
                Name = name,
                Password = password,
                Status = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            string encodedJwt = CreateToken(user);
            await Clients.Caller.SendAsync("JwtToken", encodedJwt);
        }
        private string CreateToken(User user)
        {
            var identity = GetIdentity(user);
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
               issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            notBefore: now,
            claims: identity.Claims,
            expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
               signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }
        private ClaimsIdentity GetIdentity(User user)
        {
            var claims = new List<Claim>
                {
                    new Claim("Id", Convert.ToString(user.Id)),
                    new Claim("Name", Convert.ToString(user.Name)),
                    new Claim("Login", Convert.ToString(user.Login))
                };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            return claimsIdentity;
        }   
    } 
}
