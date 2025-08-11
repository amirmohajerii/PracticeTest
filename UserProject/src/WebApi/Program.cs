using Application.Features.Users.Commands.Create;
using Application.Interfaces;
using Domain.Interfaces;
using Infrastracture.Caching;
using Infrastracture.Persistence;
using Infrastracture.Repositories;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Redis
// We use a singleton for the connection multiplexer as it is designed to be a long-lived object.
var redisConnection = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection"));
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

// Register repositories and services
// Scoped services are created once per client request.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICachingService, RedisCachingService>();

// Register MediatR for CQRS
// This will scan the specified assembly for all MediatR command and query handlers.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
