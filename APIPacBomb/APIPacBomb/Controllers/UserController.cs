using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIPacBomb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private Interfaces.IUserDatabaseService _userDatabaseService;

        public UserController(Interfaces.IUserDatabaseService userDatabaseService)
        {
            _userDatabaseService = userDatabaseService;
        }

        // GET: api/User
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            Model.User user = _userDatabaseService.GetUser(Classes.Util.GetUsernameFromToken(HttpContext));

            if (!user.IsAdmin)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/User/5
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Model.User requestedUser = _userDatabaseService.GetUser(id);

            if (!_IsAccessAllowed(requestedUser, HttpContext))
            {
                return NotFound();
            }            

            return Ok(requestedUser);
        }

        // GET: api/User/5/Picture
        [Authorize]
        [HttpGet("{id}/Picture")]
        public IActionResult GetPicture(int id)
        {
            if (!_IsAccessAllowed(id, HttpContext))
            {
                return NotFound();
            }

            return Ok(_userDatabaseService.GetUserPicture(id));
        }

        private bool _IsAccessAllowed(int requestedUserId, HttpContext context)
        {
            return _IsAccessAllowed(_userDatabaseService.GetUser(requestedUserId), context);
        }

        private bool _IsAccessAllowed(Model.User requestUser, HttpContext context)
        {
            Model.User LoggedOnUser = _userDatabaseService.GetUser(Classes.Util.GetUsernameFromToken(context));

            if (requestUser.Id != LoggedOnUser.Id && !LoggedOnUser.IsAdmin)
            {
                return false;
            }

            return true;
        }

    }
}
