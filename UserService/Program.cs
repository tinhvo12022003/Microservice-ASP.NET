using Microsoft.EntityFrameworkCore;
using UserMicroservice.Data;
using UserMicroservice.Repository;
using UserMicroservice.Config;
using UserMicroservice.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add DbContext
builder.Services.AddDbContext<UserdbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<UserRepository>();

// Add UnitOfWork
builder.Services.AddScoped<UnitOfWork>();

// Add Services
builder.Services.AddScoped<UserService>();

// Add AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

