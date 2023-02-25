using MessangerServer.Data;
using MessangerServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MessangerServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private MessangerContext context;
        public AccountController(MessangerContext context)
        {
            this.context = context;
        }
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromBody] LogData data)
        {

            var user = await context.Users.FirstOrDefaultAsync(e => e.Login == data.Login && e.Password == data.Password);
            if (user != null)
            {
                string encodedJwt = CreateToken(user);
                return encodedJwt;
            }
            else
            {
                return null;
            }           
        }
        [HttpPost("Register")]
        public async Task<ActionResult<string>> Register(User data)
        {
            User user = new User()
            {
                Login = data.Login,
                Name = data.Name,
                Password = data.Password,
                Status = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            string encodedJwt = CreateToken(user);
            return encodedJwt;
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
