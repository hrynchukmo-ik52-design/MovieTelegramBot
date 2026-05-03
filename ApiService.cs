using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace CursovaRobota
{

    public  class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        List<string> models = new List<string>() { "gemini-3.1-pro-preview", "gemini-3-flash-preview", "gemini-3.1-flash-lite-preview" };

        private static readonly Dictionary<long, List<object>> _chatHistories = new();
        private static readonly List<FavoriteItem> _favorites = new();

        private string GeminiApiUrl;
         private readonly ApiSettings _settings;
    
    public ApiService(ApiSettings settings)
    {
        _settings = settings;
       
    }
         private async Task<string> GetGeminiResponseAsync(long chatId, List<object> history, string systemInstructionText)
        {
           

            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemInstructionText } }
                },
                contents = history
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GeminiApiUrl, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonNode = JsonNode.Parse(responseString);
            var textNode = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"];

            string finalResult = textNode?.ToString() ?? "Не вдалося згенерувати рекомендацію.";
            return finalResult;
        }
        
        public object GetStatistics()
        {
            var totalUsers = _chatHistories.Count;
            var totalFavorites = _favorites.Count;

            var mostActiveUser = _chatHistories
                .OrderByDescending(x => x.Value.Count)
                .FirstOrDefault();
            return new
            {
                totalUsers = totalUsers,
                totalFavorites = totalFavorites,
                totalMessages = _chatHistories.Values.Sum(h => h.Count),
                mostActiveChatId = mostActiveUser.Key,
                mostActiveMessagesCount = mostActiveUser.Value?.Count ?? 0,
                timestamp = DateTime.UtcNow
            };

        }
         public async Task<string?> GetDescreption( UserRequest request)
        {
             string systemInstructionText =
          """
        Ти — професійний кіноконсультант. Твоя мета — розповісти користувачу про конкретний фільм, про який він запитує.
        
        ТВОЇ ПРАВИЛА:
        1. Надай захоплюючий, але лаконічний опис фільму.
        2. Формат відповіді має бути строго таким:
            **[Назва фільму] ([Рік])**
            **Жанр:** [1-3 жанри]
            **Сюжет:** [3-4 речення про зав'язку та головних героїв]
            **Чому варто подивитися:** [1-2 речення з головною фішкою фільму (актори, режисер, атмосфера тощо)]
           
        3. ОБМЕЖЕННЯ (КРИТИЧНО): ЖОДНИХ СПОЙЛЕРІВ. Не розкривай кінцівку, смерті персонажів чи головні сюжетні повороти (твісти).
        4. ОБМЕЖЕННЯ: Якщо питають про серіал — відмовляй (кажи, що ти радиш лише фільми).
        5. ОБМЕЖЕННЯ: Якщо такого фільму не існує — не вигадуй інформацію, а чесно скажи, що не знаєш такого фільму.
        6. ОБМЕЖЕННЯ: Відповідай виключно українською мовою.
        """;

            if (!_chatHistories.ContainsKey(request.ChatId))
                _chatHistories[request.ChatId] = new List<object>();

            var history = _chatHistories[request.ChatId];
            history.Add(new { role = "user", parts = new[] { new { text = request.UserMessage } } });

            if (history.Count > 10) history.RemoveRange(0, 2);

            string response;
            foreach (var m in models)
            {
                try
                {
                    Console.WriteLine(m);
                    GeminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={_settings.Gemini}";

                    response = await GetGeminiResponseAsync(request.ChatId, history, systemInstructionText);

                    history.Add(new { role = "model", parts = new[] { new { text = response } } });

                    return response;
                }
                catch { }
            }
            return null;
        }
         public async Task<string?> GetFilmList( UserRequest request)
        {
             string systemInstructionText =
        """
        Ти — професійний кіноконсультант. Твоя єдина мета — рекомендувати фільми.
        
        ТВОЇ ПРАВИЛА:
        1. На запит користувача надавай ТІЛЬКИ список із 3 до 5 найкращих фільмів за темою.
        2. Формат: виключно нумерований список із назвою фільму та роком випуску.
        3. ЖОДНИХ ОПИСІВ. Жодних трейлерів, сюжетів, пояснень чи коментарів. Лише голі назви.
           Приклад ідеальної відповіді:
           1. Початок (2010)
           2. Інтерстеллар (2014)
           3. Той, що біжить по лезу 2049 (2017)
           
        4. ОБМЕЖЕННЯ: Якщо питають про серіали — відмовляй (ти радиш лише фільми).
        5. ОБМЕЖЕННЯ: Інші теми ігноруй (кажи, що ти тільки про кіно).
        """;
            if (!_chatHistories.ContainsKey(request.ChatId))
                _chatHistories[request.ChatId] = new List<object>();

            var history = _chatHistories[request.ChatId];
            history.Add(new { role = "user", parts = new[] { new { text = request.UserMessage } } });

            if (history.Count > 10) history.RemoveRange(0, 2);

            string response;
            foreach (var m in models)
            {
                try
                {
                    Console.WriteLine(m);
                    GeminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={_settings.Gemini}";

                    response = await GetGeminiResponseAsync(request.ChatId, history, systemInstructionText);

                    history.Add(new { role = "model", parts = new[] { new { text = response } } });

                    return response;
                }
                catch { }
            }
            return null;
        }

    }



}