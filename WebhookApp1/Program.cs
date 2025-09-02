using WebhookApp1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ISubscriptionManagerService, SubscriptionManagerService>();
builder.Services.AddHostedService<WebhookRegistrationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Logger.LogInformation("WebhookApp1 starting on port 8081");
app.Run("http://localhost:7081");