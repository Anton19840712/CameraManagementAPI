using WebhookApp2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDatabaseInitializationService, DatabaseInitializationService>();
builder.Services.AddSingleton<ILeaderElectionService, LeaderElectionService>();
builder.Services.AddHostedService<WebhookRegistrationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Logger.LogInformation("WebhookApp2 starting on port 8082");
app.Run("http://localhost:7082");