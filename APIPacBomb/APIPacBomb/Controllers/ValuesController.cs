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
            ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
            Model.User user = _userDatabaseService.GetUser(identity.Claims.Where(x => x.Type == "uname").FirstOrDefault().Value);
            
            return Ok(user);
        }
    }
}
