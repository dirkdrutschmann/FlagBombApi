using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace APIPacBomb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        /// <summary>
        ///   Kofigurationsdaten
        /// </summary>
        private IConfiguration _config;

        private Interfaces.IUserDatabaseService _userDatabaseService;

        /// <summary>
        ///   Erstellt eine Instanz des Logincontrollers
        /// </summary>
        /// <param name="config">Dependency Injection für Konfigurationsdaten</param>
        public LoginController(IConfiguration config, Interfaces.IUserDatabaseService userDatabaseService)
        {
            _config = config;
            _userDatabaseService = userDatabaseService;
        }

        /// <summary>
        ///   Einloggen eines Nutzers
        ///   POST: api/Login
        /// </summary>
        /// <param name="login">Nutzerdaten</param>
        /// <returns>JWT</returns>
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] Model.User login)
        {
            IActionResult response = Unauthorized();
            var user = _userDatabaseService.Authenticate(login);

            if (user != null)
            {
                var tokenString = _GenerateJSONWebToken(user);
                user.LastLogon = DateTime.Now;
                _userDatabaseService.SetUser(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        /// <summary>
        ///   Erstellt den JWT
        /// </summary>
        /// <param name="user">Anzumeldender Nutzer</param>
        /// <returns>JWT</returns>
        private string _GenerateJSONWebToken(Model.User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
