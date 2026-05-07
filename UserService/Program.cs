using Microsoft.EntityFrameworkCore;
using UserMicroservice.Data;
using UserMicroservice.Repository;
using UserMicroservice.Config;
using UserMicroservice.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddOpenApi();

var jwtSettings = builder.Configuration.GetSection("Jwt"); // lấy section "Jwt" từ application.json
var jwtKey = jwtSettings["Key"];
builder.Services.AddSingleton(jwtSettings); // Toàn bộ app dùng chung 1 instance
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });


//  require authentication for all requests without [Authorize]
var requireAuthPolicy = new AuthorizationPolicyBuilder()
	.RequireAuthenticatedUser()
	.Build();

// builder.Services.AddAuthorizationBuilder()
// 	.SetFallbackPolicy(requireAuthPolicy);


builder.Services.AddDbContext<UserdbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));




builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UnitOfWork>();




builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();




builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperConfig>());
builder.Services.AddScoped<HashingConfig>();



var app = builder.Build();




if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


app.Run();

