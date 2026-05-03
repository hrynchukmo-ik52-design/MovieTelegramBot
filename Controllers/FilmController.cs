using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace CursovaRobota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilmController : ControllerBase
    {
        
        ApiService apiService;
        public FilmController(ApiService apiService)
        {
            this.apiService = apiService;
        }
        [HttpPost("List")]
        public async Task<IActionResult> GetFilmsList([FromBody] UserRequest request)
        {
            var response = await apiService.GetFilmList(request);
            if (response != null)
            {
                return Ok(new { reply = response });
            }
            else { return StatusCode(500, "Помилка серверу");}

        }
        [HttpPost("Description")]
        public async Task<IActionResult> GetFilmsDescription([FromBody] UserRequest request)
        {
            var response = await apiService.GetDescreption(request);
            if (response != null)
            {
                return Ok(new { reply = response });
            }
            else { return StatusCode(500, "Помилка серверу");}

        }
    }

}