using Microsoft.EntityFrameworkCore;
using RecordManiaAPI.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<RecordManiaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RecordMania API v1");
    c.RoutePrefix = ""; // dostępne pod http://localhost:5000/
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();


app.MapGet("/", () => Results.Ok("Działa"));

app.Run();