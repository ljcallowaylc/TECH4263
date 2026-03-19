using EquipmentAPI.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();



List<Equipment> equipments = new();

app.MapPost("/equipments", (CreateEquipmentDto dto) =>
{
    var equipment = new Equipment(dto.Name, dto.Category, dto.Status, dto.Location);

    equipments.Add(equipment);

    return Results.Created($"/equipments/{equipment.Id}", new EquipmentResponseDto
    {
        Id = equipment.Id,
        Name = equipment.Name,
        Category = equipment.Category,
        Status = equipment.Status
    });
})
.WithName("CreateEquipment")
.WithOpenApi();

app.MapGet("/equipments", () =>
{
    var result = equipments.Select(e => new EquipmentResponseDto
    {
        Id = e.Id,
        Name = e.Name,
        Category = e.Category,
        Status = e.Status
    });

    return Results.Ok(result);
})
.WithName("GetEquipments")
.WithOpenApi();

app.MapGet("/equipments/{id:int:min(1)}", (int id) =>
{
    var equipment = equipments.FirstOrDefault(e => e.Id == id);

    if (equipment == null)
        return Results.NotFound();

    return Results.Ok(new EquipmentResponseDto
    {
        Id = equipment.Id,
        Name = equipment.Name,
        Category = equipment.Category,
        Status = equipment.Status
    });
})
.WithName("GetEquipmentById")
.WithOpenApi();



app.Run();

public class Equipment
{
    private static int _counter = 1;   // auto-increment counter

    public int Id { get; private set; }   // Server-assigned unique identifier
    public string Name { get; set; }      // Required
    public string Category { get; set; }
    public string Status { get; set; }
    public string Location { get; set; }

    public Equipment(string name, string category, string status, string location)
    {
        Id = _counter++;
        Name = name;
        Category = category;
        Status = status;
        Location = location;
    }
}