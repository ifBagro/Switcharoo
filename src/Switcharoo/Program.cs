using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Switcharoo;
using Switcharoo.Database;
using Switcharoo.Entities;
using Switcharoo.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();

builder.Services.AddDbContext<SwitcharooContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SwitcharooDb"));
});

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<SwitcharooContext>()
    .AddApiEndpoints();

// builder.Services.AddTransient<IDbConnection>(_ => new SqliteConnection(builder.Configuration.GetConnectionString("SwitcharooDb")));
builder.Services.AddScoped<IFeatureProvider, FeatureProvider>();
// builder.Services.AddScoped<IRepository, FeatureRepository>();
builder.Services.AddScoped<IRepository, EfRepository>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapIdentityApi<User>();

// app.VerifyDatabase(app.Services.GetRequiredService<IDbConnection>());

app.MapControllers();

app.Run();
