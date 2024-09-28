using Microsoft.EntityFrameworkCore;
using MediFinder_Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregamos la configuración para SQL
var connecionString = builder.Configuration.GetConnectionString("cadenaSQL");
builder.Services.AddDbContext<MedifinderContext>(options => options.UseSqlServer(connecionString));

//Definimos la nueva politica de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Activamos la nueva politca
app.UseCors("NuevaPolitica");
app.UseAuthorization();
app.MapControllers();

//var host = "http://192.168.100.171:5257";
//var hostCris = "http://192.168.100.37:5257";
//var host = "http://10.16.24.162:5257";

//app.Run(host);
app.Run();