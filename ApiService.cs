using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore; 

namespace CursovaRobota
{
    public class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        List<string> models = new List<string>() { "gemini-3.1-pro-preview", "gemini-3.5-flash", "gemini-flash-lite-latest" };

        private static readonly List<FavoriteItem> _favorites = new();

        private string GeminiApiUrl;
        private readonly ApiSettings _settings;
        private readonly AppDbContext _db; // Додано поле БД

        // Оновлено конструктор
        public ApiService(ApiSettings settings, AppDbContext db)
        {
            _settings = settings;
            _db = db;
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
            // Отримуємо статистику прямо з бази даних
            var totalUsers = _db.ChatMessages.Select(m => m.ChatId).Distinct().Count();
            var totalMessages = _db.ChatMessages.Count();
            var totalFavorites = _favorites.Count;

            var mostActiveUser = _db.ChatMessages
                .GroupBy(m => m.ChatId)
                .OrderByDescending(g => g.Count())
                .Select(g => new { ChatId = g.Key, Count = g.Count() })
                .FirstOrDefault();

            return new
            {
                totalUsers = totalUsers,
                totalFavorites = totalFavorites,
                totalMessages = totalMessages,
                mostActiveChatId = mostActiveUser?.ChatId ?? 0,
                mostActiveMessagesCount = mostActiveUser?.Count ?? 0,
                timestamp = DateTime.UtcNow
            };
        }

        public async Task<string?> GetDescreption(UserRequest request)
        {
            string systemInstructionText =
         """
        Ти — професійний кіноконсультант. Твоя мета — розповісти користувачу про конкретний фільм, про який він запитує.    
    ТВОЇ ПРАВИЛА:
    1. Якщо користувач просить порекомендувати або загалом розповісти про фільм, надай його захоплюючий, але лаконічний опис.
    2. Формат базової відповіді (для загального опису) має бути строго таким:
        **[Назва фільму] ([Рік])**
        **Жанр:** [1-3 жанри]
        **Сюжет:** [3-4 речення про зав'язку та головних героїв]
        **Чому варто подивитися:** [1-2 речення з головною фішкою фільму (актори, режисер, атмосфера тощо)]
    
    3. ОБМЕЖЕННЯ (КРИТИЧНО): ЖОДНИХ СПОЙЛЕРІВ. Не розкривай кінцівку, смерті персонажів чи головні сюжетні повороти (твісти). Це стосується будь-яких твоїх відповідей.
    4. ОБМЕЖЕННЯ: Якщо питають про серіал — відмовляй (кажи, що ти радиш лише фільми).
    5. ОБМЕЖЕННЯ: Якщо такого фільму не існує — не вигадуй інформацію, а чесно скажи, що не знаєш такого фільму.
    6. ОБМЕЖЕННЯ: Відповідай виключно українською мовою.
    7. ДОЗВІЛ НА ДЕТАЛІ (ДІАЛОГ): Якщо користувач запитує про конкретні подробиці фільму (акторський склад, історію створення, цікаві факти зі зйомок, саундтрек, пояснення лору світу тощо), сміливо та детально відповідай на ці питання у вільній формі. Жорсткий формат з пункту 2 для таких відповідей використовувати не потрібно, але правило "ЖОДНИХ СПОЙЛЕРІВ" залишається активним.
    """;

            // 1. Зберігаємо повідомлення користувача в БД
            _db.ChatMessages.Add(new ChatMessage { ChatId = request.ChatId, Role = "user", Text = request.UserMessage });
            await _db.SaveChangesAsync();

            // 2. Отримуємо останні 10 повідомлень для контексту
            var dbHistory = _db.ChatMessages
                .Where(m => m.ChatId == request.ChatId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            var history = dbHistory.Select(m => new { role = m.Role, parts = new[] { new { text = m.Text } } }).Cast<object>().ToList();

            string response;
            foreach (var m in models)
            {
                try
                {
                    Console.WriteLine(m);
                    GeminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={_settings.Gemini}";

                    response = await GetGeminiResponseAsync(request.ChatId, history, systemInstructionText);

                    // 3. Зберігаємо відповідь моделі в БД
                    _db.ChatMessages.Add(new ChatMessage { ChatId = request.ChatId, Role = "model", Text = response });
                    await _db.SaveChangesAsync();

                    return response;
                }
                catch { }
            }
            return null;
        }

        public async Task<string?> GetFilmList(UserRequest request)
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

            // 1. Зберігаємо повідомлення користувача в БД
            _db.ChatMessages.Add(new ChatMessage { ChatId = request.ChatId, Role = "user", Text = request.UserMessage });
            await _db.SaveChangesAsync();

            // 2. Отримуємо останні 10 повідомлень для контексту
            var dbHistory = _db.ChatMessages
                .Where(m => m.ChatId == request.ChatId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            var history = dbHistory.Select(m => new { role = m.Role, parts = new[] { new { text = m.Text } } }).Cast<object>().ToList();

            string response;
            foreach (var m in models)
            {
                try
                {
                    Console.WriteLine(m);
                    GeminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={_settings.Gemini}";

                    response = await GetGeminiResponseAsync(request.ChatId, history, systemInstructionText);

                    // 3. Зберігаємо відповідь моделі в БД
                    _db.ChatMessages.Add(new ChatMessage { ChatId = request.ChatId, Role = "model", Text = response });
                    await _db.SaveChangesAsync();

                    return response;
                }
                catch { }
            }
            return null;
        }
    }
}