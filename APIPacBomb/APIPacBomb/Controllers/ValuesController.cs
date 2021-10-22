using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIPacBomb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private Interfaces.IUserDatabaseService _userDatabaseService;

        public ValuesController(Interfaces.IUserDatabaseService userDatabaseService)
        {
            _userDatabaseService = userDatabaseService;
        }

        // GET: api/Values
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_userDatabaseService.GetUser(Classes.Util.GetUsernameFromToken(HttpContext)));
        }
    }
}
