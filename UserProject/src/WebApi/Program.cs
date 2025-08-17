using Application.Features.Users.Commands.Create;
using Application.Interfaces;
using Domain.Interfaces;
using Infrastracture.Caching;
using Infrastracture.Persistence;
using Infrastracture.Repositories;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var redisConnection = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection"));
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICachingService, RedisCachingService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
