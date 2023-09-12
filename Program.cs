using CreekRiver.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;


var builder = WebApplication.CreateBuilder(args);

/* 下面这三个是初始设置时要添加的 */

// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// [最重要] allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<CreekRiverDbContext>(builder.Configuration["CreekRiverDbConnectionString"]);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

/* 初始设置完成 */

// Add services to the container.
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

app.UseHttpsRedirection();

app.MapGet("/api/campsites", (CreekRiverDbContext db) =>
{
    return db.Campsites.ToList();
    /* 
     EF Core, is turning this method chain into a SQL query: SELECT Id, Nickname, ImageUrl, CampsiteTypeId FROM "Campsites"; 

    and then turning the tabular data that comes back from the database into .NET objects! 

    ASP.NET is serializing those .NET objects into JSON to send back to the client.*/

    /* dependency injection, where the framework sees a dependency that the handler requires, and passes in an instance of it as an arg so that the handler can use it. */
});

app.MapGet("/api/campsites/{id}", (CreekRiverDbContext db, int id) =>
{
    return db.Campsites.Include(c => c.CampsiteType).Single(c => c.Id == id);
});

/* Include() 等于 _expand */
app.Run();

