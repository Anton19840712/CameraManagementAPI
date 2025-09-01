using CameraManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Camera Management API", Version = "v1" });
});

builder.Services.AddSingleton<CameraService>();
builder.Services.AddHttpClient<EventSubscriptionService>();
builder.Services.AddScoped<EventSubscriptionService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

// Remove HTTPS redirection for simplicity
// app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Configure to listen on port 8080
//app.Urls.Add("http://0.0.0.0:8080");

//Console.WriteLine("Camera Management API starting on http://localhost:8080");
//Console.WriteLine("Swagger available at http://localhost:8080/swagger");

app.Run();
