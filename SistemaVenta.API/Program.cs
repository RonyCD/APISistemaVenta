
using SistemaVenta.IOC;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
    
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CONEXCIÓN BD
builder.Services.InyectarDependencias(builder.Configuration);


//ACTIVAR CORS 1/2
builder.Services.AddCors(options => {
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });

});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//ACTIVAR LA CONFIGURACION DE CORS 2/2
app.UseCors("NuevaPolitica");

app.UseAuthorization();

app.MapControllers();

app.Run();
