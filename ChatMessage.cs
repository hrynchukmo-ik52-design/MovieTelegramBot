using System.ComponentModel.DataAnnotations;

namespace CursovaRobota
{
    public class ChatMessage
    {
        [Key] // Це первинний ключ (ID)
        public int Id { get; set; }
        
        public long ChatId { get; set; } // Кому належить повідомлення
        
        public string Role { get; set; } // "user" або "model" (ШІ)
        
        public string Text { get; set; } // Текст повідомлення
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Час створення
    }
}