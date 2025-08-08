using Notification;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IHostedService, MessageListener>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
