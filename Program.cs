using Microsoft.EntityFrameworkCore;
using CursovaRobota;

var builder = WebApplication.CreateBuilder(args);

// 1. ПІДКЛЮЧЕННЯ БАЗИ ДАНИХ (PostgreSQL)
// Програма автоматично шукає змінну ConnectionStrings:DefaultConnection 
// у файлі appsettings.json або у вкладці Variables на Railway
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. НАЛАШТУВАННЯ КЛЮЧІВ (ApiKeys)
// Зчитуємо секцію ApiKeys і реєструємо її як Singleton
var apiSettings = builder.Configuration.GetSection("ApiKeys").Get<ApiSettings>() ?? new ApiSettings();
builder.Services.AddSingleton(apiSettings);

// 3. РЕЄСТРАЦІЯ СЕРВІСУ ApiService
// ВАЖЛИВО: Змінено з AddSingleton на AddScoped.
// Це необхідно, бо ApiService тепер використовує базу даних (AppDbContext), 
// яка не може бути всередині Singleton-сервісу.
builder.Services.AddScoped<ApiService>();

// 4. СТАНДАРТНІ НАЛАШТУВАННЯ API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. НАЛАШТУВАННЯ SWAGGER
// Ми дозволяємо Swagger і в Development, і на Railway, щоб ти міг перевірити API через браузер
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie API V1");
    c.RoutePrefix = string.Empty; // Робимо Swagger головною сторінкою
});

app.UseHttpsRedirection();
app.UseAuthorization();

// 6. МАПІНГ КОНТРОЛЕРІВ
app.MapControllers();

app.Run();