using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShareBucket.DataAccessLayer.Data;
using ShareBucket.JwtMiddlewareClient;
using ShareBucket.JwtMiddlewareClient.Services;
using ShareBucket.UserMicroService.Services;

GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());


var builder = WebApplication.CreateBuilder(args);
var _configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJwtUtils, JwtUtils>();
builder.Services.AddScoped<IUserService,UserService>();

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder
    .WithOrigins("http://localhost:3000",
                 "https://localhost:3000",
                 "http://localhost:7000",
                 "https://localhost:7001")
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials()
    .AllowAnyMethod()
    .AllowAnyHeader();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<JwtMiddleware>();

app.UseHttpsRedirection();

app.UseCors("corsapp");

app.UseAuthorization();

app.MapControllers();

app.Run();
