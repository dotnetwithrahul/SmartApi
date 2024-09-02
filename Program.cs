using FirebaseApiMain.Application.Interfaces;
using FirebaseApiMain.Application.Services;
using FirebaseApiMain.Infrastructure.Interface;
using FirebaseApiMain.Infrastructure.Services;
using FirebaseApiMain.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();


builder.Services.AddScoped<FirebaseService>();



builder.Services.AddHttpClient<IProductRepository, ProductRepository>();


builder.Services.AddScoped<ICustomerProductService, CustomerProductServicecs>();

builder.Services.AddTransient<IFileService, FileService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseStaticFiles();   

app.UseAuthorization();

app.MapControllers();

app.Run();
