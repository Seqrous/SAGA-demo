using Npgsql;
using Orchestrator;

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();