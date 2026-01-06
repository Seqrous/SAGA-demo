using System.Threading.Channels;
using Npgsql;
using Orchestrator;

const int maxChannelSize = 1000;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var pgDbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");

await using var dataSource = NpgsqlDataSource.Create($"HOST={pgHost};Username={pgUser};Password={pgPassword};Database={pgDbName};Port={pgPort};");
builder.Services.AddSingleton(dataSource);
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