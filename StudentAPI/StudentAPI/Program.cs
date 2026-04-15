using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text;
using StudentAPI.Data;
using StudentAPI.Models;
using StudentAPI.Auth;
using StudentAPI.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("BasicAuth", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Basic Authentication"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "BasicAuth"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Auth
builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();

//app.UseHttpsRedirection();


//var students = new List<Student>(); // In-memory list to store students for demo purposes






// --------------------
// POST /students
// --------------------
app.MapPost("/students", async (AppDbContext context, Student student) =>
{
    context.Students.Add(student);
    await context.SaveChangesAsync();

    return Results.Created($"/students/{student.Id}", student);
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("CreateStudent")
.WithOpenApi();

// --------------------
// GET /students
// --------------------
app.MapGet("/students", async (AppDbContext context) =>
{
    var students = await context.Students.ToListAsync();
    return Results.Ok(students);
})
.RequireAuthorization()
.WithName("GetAllStudents")
.WithOpenApi();

// --------------------
// GET /students/{id}
// --------------------
app.MapGet("/students/{id:int}", async (AppDbContext context, int id) =>
{
    var student = await context.Students.FindAsync(id);
    return student is not null ? Results.Ok(student) : Results.NotFound();
})
.RequireAuthorization()
.WithName("GetStudentById")
.WithOpenApi();

// Temporary — remove after use
//app.MapGet("/hash/{password}", (string password) =>
   // Results.Ok(StudentAPI.Helpers.PasswordHasher.Hash(password)));

app.Run();

