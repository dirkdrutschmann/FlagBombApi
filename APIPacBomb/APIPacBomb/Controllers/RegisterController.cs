using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APIPacBomb.Controllers
{
    /// <summary>
    ///   Controller zur Steuerung der Registrierung eines Nutzers
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private Interfaces.IUserDatabaseService _userDatabaseService;

        /// <summary>
        ///   Erzeugt eine Instanz des Registrierungscontrollers
        /// </summary>
        /// <param name="userDatabaseService">Datenbankservice</param>
        public RegisterController(Interfaces.IUserDatabaseService userDatabaseService)
        {
            _userDatabaseService = userDatabaseService;
        }

        /// <summary>
        ///   Prüft, ob eine E-Mailadresse bereits vorhanden ist
        /// </summary>
        /// <param name="mail">Mailadresse</param>
        /// <returns>true, wenn E-Mailadresse bereits im System hinterlegt ist, sonst false</returns>
        [AllowAnonymous]
        [HttpGet("mail/{mail}")]        
        public IActionResult GetMail(string mail)
        {
            return Ok(_userDatabaseService.ExistsMail(mail));
        }

        /// <summary>
        ///   Prüft, ob ein Nutzername bereits im System hinterlegt ist
        /// </summary>
        /// <param name="username">Nutzername</param>
        /// <returns>true, wenn Nutzername bereits vorahnden ist, sonst false</returns>
        [AllowAnonymous]
        [HttpGet("user/{username}")]
        public IActionResult Get(string username)
        {
            return Ok(_userDatabaseService.ExistsUsername(username));
        }

        /// <summary>
        ///   Registriert einen Nutzers
        /// </summary>
        /// <param name="user">Nutzerinformationen</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Post([FromBody] Model.User user)
        {
            IActionResult response = BadRequest();

            Model.User newUser = user;

            if (newUser != null)
            {
                if (_userDatabaseService.ExistsMail(newUser.Email) == true)
                {
                    return BadRequest("E-Mailadresse existiert bereits.");
                }

                if (_userDatabaseService.ExistsUsername(newUser.Username))
                {
                    return BadRequest("Nutzername existiert bereits");
                }

                if (string.IsNullOrEmpty(newUser.Password) || string.IsNullOrWhiteSpace(newUser.Password))
                {
                    return BadRequest("Kein Passwort übergeben.");
                }

                newUser.Secret = Guid.NewGuid().ToString();
                newUser.RegistrationOn = DateTime.Now;

                newUser.GeneratePasswordHash();

                newUser = _userDatabaseService.RegisterUser(newUser);

                response = Ok(newUser);
            }

            return response;
        }

    }
}
