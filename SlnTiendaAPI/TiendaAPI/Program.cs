using Microsoft.EntityFrameworkCore;
using TiendaAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// recuperar la cadena de conexion
var cadena = builder.Configuration.GetConnectionString("cn1");
// asignar la cadena de conexion al contexto del EF Core, el
// cual se está registrando dentro de la Aplicacion
builder.Services.AddDbContext<BdtiendatelasContext>(
    x => x.UseSqlServer(cadena));
//

// Add services to the container.

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
