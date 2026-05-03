public class UserRequest
    {
        public long ChatId { get; set; }
        public string UserMessage { get; set; }
    }

    public class FavoriteItem 
    { 
        public long ChatId { get; set; } 
        public string MovieTitle { get; set; } 

        public string? Note { get; set; } 
    }