using TaskBoard.Infrastructure;
using TaskBoard.NotificationConsumer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddNotificationConsumer();

var app = builder.Build();
app.Run();
