using System.Threading.Channels;
using EasyNetQ;
using Npgsql;
using Orchestrator;

const int maxChannelSize = 1000;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

RegisterPostgresConnection();
RegisterRmqConnection();
builder.Services.AddScoped<ISagaRepository, PgSagaRepository>();
builder.Services.AddHostedService<SagaWorker>();
builder.Services.AddSingleton(TimeProvider.System);

var sagaChannel = Channel.CreateBounded<Guid>(
    new BoundedChannelOptions(maxChannelSize)
    {
        SingleReader = true,
        SingleWriter = true,
        AllowSynchronousContinuations = false,
        FullMode = BoundedChannelFullMode.DropWrite
    }
);

builder.Services.AddSingleton(sagaChannel.Reader);
builder.Services.AddSingleton(sagaChannel.Writer);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
return;

void RegisterPostgresConnection()
{
    var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
    var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
    var user = Environment.GetEnvironmentVariable("POSTGRES_USER");
    var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");

    var dataSource = NpgsqlDataSource.Create($"HOST={host};Username={user};Password={pass};Database={dbName};Port={port};");
    builder.Services.AddSingleton(dataSource);
}

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