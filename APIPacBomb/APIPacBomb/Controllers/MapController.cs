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
    public class MapController : ControllerBase
    {
        // GET: api/Map
        [Authorize]
        [HttpGet]
        public IActionResult Get([FromBody] Classes.Requests.MapRequest mapSettings)
        {
            Model.Map.Grid grid = new Model.Map.Grid(mapSettings.Width, mapSettings.Height, mapSettings.SquareFactor);
            grid.GenerateMap();

            return Ok(grid.Columns);
        }

    }
}
