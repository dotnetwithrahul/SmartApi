using FirebaseApiMain.Application.Interfaces;
using FirebaseApiMain.Application.Services;
using FirebaseApiMain.Infrastructure.Auth.Interface;
using FirebaseApiMain.Infrastructure.Auth.Models;
using FirebaseApiMain.Infrastructure.Auth.Services;
using FirebaseApiMain.Infrastructure.Interface;
using FirebaseApiMain.Infrastructure.Services;
using FirebaseApiMain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();




builder.Services.Configure<JwtSection>(builder.Configuration.GetSection("JwtSection"));

var jwtSection = builder.Configuration.GetSection(nameof(JwtSection)).Get<JwtSection>();
Console.WriteLine(jwtSection);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSection.Issuer,
        ValidAudience = jwtSection.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection.Key)),
        ClockSkew = TimeSpan.Zero
    };
});








builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://smartecomreact.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HMS API",
        Version = "v1"
    });

    // XML comments path for Swagger


    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});



builder.Services.AddMemoryCache();


builder.Services.AddScoped<FirebaseService>();



builder.Services.AddHttpClient<IProductRepository, ProductRepository>();


builder.Services.AddScoped<ICustomerProductService, CustomerProductServicecs>();

builder.Services.AddTransient<IFileService, FileService>();


builder.Services.AddTransient<IAuthInterface, AuthService>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication(); // Add this before UseAuthorization
app.UseAuthorization();


app.MapControllers();

app.Run();
