using CursovaRobota;

var builder = WebApplication.CreateBuilder(args);


var apiSettings = builder.Configuration.GetSection("ApiKeys").Get<ApiSettings>();
builder.Services.AddSingleton(apiSettings);

builder.Services.AddSingleton<ApiService>();
builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; 
    });
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();