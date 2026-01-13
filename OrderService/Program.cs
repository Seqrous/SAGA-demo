using EasyNetQ;
using OrderService;

var builder = WebApplication.CreateBuilder(args);

RegisterRmqConnection();
builder.Services.AddHostedService<RabbitMqAutoSubscriberHostedService>();
builder.Services.AddTransient<MessageConsumer>();
var app = builder.Build();
app.Run();
return;

void RegisterRmqConnection()
{
    var host = Environment.GetEnvironmentVariable("RMQ_HOST");
    var user = Environment.GetEnvironmentVariable("RMQ_USER");
    var pass = Environment.GetEnvironmentVariable("RMQ_PASSWORD");
    var port = Environment.GetEnvironmentVariable("RMQ_PORT");
    var vhost = Environment.GetEnvironmentVariable("RMQ_VHOST");

    builder.Services
        .AddEasyNetQ($"host={host};virtualHost={vhost};username={user};password={pass};port={port}")
        .UseSystemTextJson();
}