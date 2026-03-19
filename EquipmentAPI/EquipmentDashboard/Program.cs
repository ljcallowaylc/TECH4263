using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

var client = new HttpClient
{
    BaseAddress = new Uri("http://scaling-palm-tree-wrpg57x9vr4wc5g9g-5280.app.github.dev/") // CHANGE THIS PORT
};

var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

while (true)
{
    Console.Clear();
    Console.WriteLine("==============================");
    Console.WriteLine("  Equipment Dashboard (Console)");
    Console.WriteLine("==============================");
    Console.WriteLine("1. List all equipment");
    Console.WriteLine("2. View equipment by ID");
    Console.WriteLine("3. Create new equipment");
    Console.WriteLine("0. Exit");
    Console.Write("\nSelect an option: ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await ListAllEquipment();
            break;

        case "2":
            await ViewEquipmentById();
            break;

        case "3":
            await CreateEquipment();
            break;

        case "0":
            return;

        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid option. Try again.");
            Console.ResetColor();
            Pause();
            break;
    }
}

async Task ListAllEquipment()
{
    try
    {
        var response = await client.GetAsync("/equipments");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to retrieve equipment.");
            Pause();
            return;
        }

        var json = await response.Content.ReadAsStringAsync();
        var items = JsonSerializer.Deserialize<List<EquipmentResponseDto>>(json, options);

        if (items == null || items.Count == 0)
        {
            Console.WriteLine("No equipment found.");
            Pause();
            return;
        }

        Console.WriteLine("\nID\tName\t\tStatus");
        Console.WriteLine("--------------------------------");

        foreach (var item in items)
        {
            Console.WriteLine($"{item.Id}\t{item.Name}\t\t{item.Status}");
        }
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("Error: Cannot reach Equipment API.");
    }

    Pause();
}

async Task ViewEquipmentById()
{
    Console.Write("Enter Equipment ID: ");
    var input = Console.ReadLine();

    if (!int.TryParse(input, out int id) || id <= 0)
    {
        Console.WriteLine("Invalid ID.");
        Pause();
        return;
    }

    try
    {
        var response = await client.GetAsync($"/equipments/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Console.WriteLine("Equipment not found.");
            Pause();
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Error retrieving equipment.");
            Pause();
            return;
        }

        var json = await response.Content.ReadAsStringAsync();
        var item = JsonSerializer.Deserialize<EquipmentResponseDto>(json, options);

        Console.WriteLine("\nEquipment Details:");
        Console.WriteLine("-------------------");
        Console.WriteLine($"ID: {item.Id}");
        Console.WriteLine($"Name: {item.Name}");
        Console.WriteLine($"Category: {item.Category}");
        Console.WriteLine($"Status: {item.Status}");
        Console.WriteLine($"Location: {item.Location}");
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("Error: Cannot reach Equipment API.");
    }

    Pause();
}

async Task CreateEquipment()
{
    Console.Write("Name: ");
    var name = Console.ReadLine();

    Console.Write("Category: ");
    var category = Console.ReadLine();

    Console.Write("Status: ");
    var status = Console.ReadLine();

    Console.Write("Location: ");
    var location = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(name) ||
        string.IsNullOrWhiteSpace(category) ||
        string.IsNullOrWhiteSpace(status) ||
        string.IsNullOrWhiteSpace(location))
    {
        Console.WriteLine("All fields are required.");
        Pause();
        return;
    }

    var dto = new CreateEquipmentDto
    {
        Name = name,
        Category = category,
        Status = status,
        Location = location
    };

    try
    {
        var response = await client.PostAsJsonAsync("/equipments", dto);

        if (response.IsSuccessStatusCode)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Equipment created successfully!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed to create equipment.");
        }

        Console.ResetColor();
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("Error: Cannot reach Equipment API.");
    }

    Pause();
}

void Pause()
{
    Console.WriteLine("\nPress Enter to continue...");
    Console.ReadLine();
}

    public class CreateEquipmentDto
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
    }
 public class EquipmentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
    }