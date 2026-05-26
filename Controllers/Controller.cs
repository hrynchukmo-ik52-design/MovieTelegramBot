using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CursovaRobota;
using Microsoft.OpenApi;
using Microsoft.VisualBasic;
using System.Linq; // Додано для роботи з базою даних

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    ApiService apiService;
    AppDbContext _db; // Додано підключення до бази

    // Оновлено конструктор
    public UsersController(ApiService apiService, AppDbContext db)
    {
       this.apiService = apiService;
       _db = db;
    }

    // Видалено статичний список _favorites

    [HttpPost("add-favorite")]
    public IActionResult AddFavorite([FromBody] FavoriteItem item)
    {
        if (string.IsNullOrEmpty(item.MovieTitle)) return BadRequest();
        
        // Перевіряємо в базі даних
        if (!_db.Favorites.Any(f => f.ChatId == item.ChatId && f.MovieTitle == item.MovieTitle))
        {
            _db.Favorites.Add(item);
            _db.SaveChanges(); // Зберігаємо в БД
        }
        return Ok(new { status = "success" });
    }

    [HttpGet("favorites/{chatId}")]
    public IActionResult GetFavorites(long chatId)
    {
        // Читаємо з бази даних
        var list = _db.Favorites.Where(f => f.ChatId == chatId).ToList();
        return Ok(new { favorites = list });
    }

    [HttpDelete("favorites/{chatId}/{movieTitle}")]
    public IActionResult DeleteFavorite(long chatId, string movieTitle)
    {
        // Шукаємо і видаляємо з бази даних
        var itemsToDelete = _db.Favorites.Where(f => f.ChatId == chatId && f.MovieTitle == movieTitle).ToList();
        if (itemsToDelete.Any())
        {
            _db.Favorites.RemoveRange(itemsToDelete);
            _db.SaveChanges(); // Зберігаємо в БД
        }
        return Ok(new { status = "success" });
    }

    [HttpPut("favorites/update")]
    public IActionResult UpdateFavorite([FromBody] FavoriteItem updateRequest)
    {
        // Шукаємо в базі даних
        var item = _db.Favorites.FirstOrDefault(f => f.ChatId == updateRequest.ChatId && f.MovieTitle == updateRequest.MovieTitle);
        if (item != null)
        {
            item.Note = updateRequest.Note;
            _db.SaveChanges(); // Зберігаємо в БД
            return Ok(new { status = "success" });
        }
        return NotFound();
    }

    
}