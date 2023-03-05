using MessangerServer.Data;
using MessangerServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MessangerServer.Models.FromClient;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using MessangerServer.Models.Auth;

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
            byte[] avatar;
            var filePath = Directory.GetCurrentDirectory() + "\\wwwroot\\forest.jpg";

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                avatar = binaryReader.ReadBytes((int)fileStream.Length);
            }

            User user = new User()
            {
                Login = data.Login,
                Name = data.Name,
                Password = data.Password,
                Status = true,
                Avatar = avatar
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

        [HttpPost]
        [Route("AccountEdit")]
        public async Task<string> AccountEdit([FromForm] IFormFile file, [FromForm] string otherData)
        {
            var changedData = JsonConvert.DeserializeObject<ChangedData>(otherData);
            byte[] imageData = null;


            User user = await context.Users.FirstOrDefaultAsync(e => e.Id == Convert.ToInt32(changedData.Id));
            user.Name = changedData.Name;
            user.Login = changedData.Login;

            if(file.Length != 4)
            {
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)file.Length);
                }
                user.Avatar = imageData;
            }

            context.Update(user);
            await context.SaveChangesAsync();

            string encodedJwt = CreateToken(user);
            return encodedJwt;
        }
    }
}
