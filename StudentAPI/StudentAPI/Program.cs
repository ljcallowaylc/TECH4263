using StudentAPI.Models;
using Microsoft.EntityFrameworkCore;
//using StudentAPI.Data;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


//var students = new List<Student>(); // In-memory list to store students for demo purposes






// --------------------
// POST /students
// --------------------
app.MapPost("/students", async (AppDbContext context, Student student) =>
{
    context.Students.Add(student);              // Add new student
    await context.SaveChangesAsync();           // Save to DB
    return Results.Created($"/students/{student.Id}", student);  // Return 201 with student info
})
.WithName("CreateStudent")
.WithOpenApi();

// --------------------
// GET /students
// --------------------
app.MapGet("/students", async (AppDbContext context) =>
{
    var students = await context.Students.ToListAsync(); // Get all students
    return Results.Ok(students);
})
.WithName("GetAllStudents")
.WithOpenApi();

// --------------------
// GET /students/{id}
// --------------------
app.MapGet("/students/{id:int}", async (AppDbContext context, int id) =>
{
    var student = await context.Students.FindAsync(id); // Find student by Id
    return student is not null ? Results.Ok(student) : Results.NotFound();
})
.WithName("GetStudentById")
.WithOpenApi();

app.Run();


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }
}

