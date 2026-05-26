using System.ComponentModel.DataAnnotations; // Обов'язково додай цей using для [Key]

namespace CursovaRobota
{
    public class UserRequest
    {
        public long ChatId { get; set; }
        public string UserMessage { get; set; }
    }

    public class FavoriteItem
    {
        [Key] // Кажемо базі даних, що це унікальний номер запису
        public int Id { get; set; } 

        public long ChatId { get; set; }
        
        public string MovieTitle { get; set; }
        
        public string? Note { get; set; }

        // Додаємо час, щоб бот міг показувати найновіші збережені фільми зверху
        public DateTime AddedAt { get; set; } = DateTime.UtcNow; 
    }
}