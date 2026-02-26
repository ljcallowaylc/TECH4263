
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();


List<Equipment> equipments = new();


app.MapPost("/createequipment", (string name, string category, string status, string location) =>
{
    if (string.IsNullOrWhiteSpace(name))
        return Results.BadRequest("Name is required.");

    Equipment equipment = new Equipment(name, category, status, location);
    equipments.Add(equipment);

    return Results.Created($"/getequipment/{equipment.Id}", equipment);
}).WithName("CreateEquipment");

app.MapGet("/getequipments", () =>
{
    return Results.Ok(equipments);
}).WithName("GetEquipments");

app.MapGet("/getequipment/{id}", (int id) =>
{
    var equipment = equipments.FirstOrDefault(e => e.Id == id);

    if (equipment is null)
        return Results.NotFound($"Equipment with Id {id} not found.");

    return Results.Ok(equipment);
}).WithName("GetEquipmentById");



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