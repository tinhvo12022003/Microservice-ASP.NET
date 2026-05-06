using Microsoft.EntityFrameworkCore;
using UserMicroservice.Data;
using UserMicroservice.Repository;
using UserMicroservice.Config;
using UserMicroservice.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();




builder.Services.AddDbContext<UserdbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));




builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UnitOfWork>();




builder.Services.AddScoped<UserService>();




builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperConfig>());
builder.Services.AddScoped<HashingConfig>();



var app = builder.Build();




if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();

