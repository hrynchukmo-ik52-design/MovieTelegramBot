using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CursovaRobota;
using Microsoft.OpenApi;
using Microsoft.VisualBasic;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    ApiService apiService ;
    public UsersController(ApiService apiService)
    {
       this.apiService = apiService;
    }

   private static readonly List<FavoriteItem> _favorites = new();
    [HttpPost("add-favorite")]
    public IActionResult AddFavorite([FromBody] FavoriteItem item)
    {
        if (string.IsNullOrEmpty(item.MovieTitle)) return BadRequest();
        if (!_favorites.Any(f => f.ChatId == item.ChatId && f.MovieTitle == item.MovieTitle))
            _favorites.Add(item);
        return Ok(new { status = "success" });
    }

    
    [HttpGet("favorites/{chatId}")]
    public IActionResult GetFavorites(long chatId)
    {
        var list = _favorites.Where(f => f.ChatId == chatId).ToList();
        return Ok(new { favorites = list });
    }

    
    [HttpDelete("favorites/{chatId}/{movieTitle}")]
    public IActionResult DeleteFavorite(long chatId, string movieTitle)
    {
        _favorites.RemoveAll(f => f.ChatId == chatId && f.MovieTitle == movieTitle);
        return Ok(new { status = "success" });
    }


    [HttpPut("favorites/update")]
    public IActionResult UpdateFavorite([FromBody] FavoriteItem updateRequest)
    {
        var item = _favorites.FirstOrDefault(f => f.ChatId == updateRequest.ChatId && f.MovieTitle == updateRequest.MovieTitle);
        if (item != null)
        {
            item.Note = updateRequest.Note;
            return Ok(new { status = "success" });
        }
        return NotFound();
    }

   [HttpGet("stats")]
public IActionResult GetStatistics()
{
   return Ok(apiService.GetStatistics());
}

}