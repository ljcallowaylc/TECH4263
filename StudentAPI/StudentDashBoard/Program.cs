using System.Net.Http.Json;
using System.Text.Json;
using StudentDashboard.Models;

// ── HttpClient setup ───────────────────────────────────────────────────────
// Update the port to match your StudentAPI (check launchSettings.json)
using var client = new HttpClient
{
    BaseAddress = new Uri("http://scaling-palm-tree-wrpg57x9vr4wc5g9g-5201.app.github.dev/")
};

var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// ── Main menu loop ─────────────────────────────────────────────────────────
while (true)
{
    PrintHeader();
    Console.WriteLine("1. List all students");
    Console.WriteLine("2. View student by ID");
    Console.WriteLine("3. Create new student");
    Console.WriteLine("0. Exit");
    Console.WriteLine();
    Console.Write("Select an option: ");

    string choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    switch (choice)
    {
        case "1": await ListStudentsAsync(); break;
        case "2": await ViewStudentAsync();  break;
        case "3": await CreateStudentAsync(); break;
        case "0":
            Console.WriteLine("Goodbye.");
            return;
        default:
            PrintError("Invalid option. Please enter 1, 2, 3, or 0.");
            break;
    }

    Console.WriteLine();
    Console.Write("Press Enter to continue...");
    Console.ReadLine();
}

// ── List all students ──────────────────────────────────────────────────────
async Task ListStudentsAsync()
{
    try
    {
        var response = await client.GetAsync("/students");

        if (!response.IsSuccessStatusCode)
        {
            PrintError($"API error: {response.StatusCode}");
            return;
        }

        string json = await response.Content.ReadAsStringAsync();
        var students = JsonSerializer.Deserialize<List<StudentResponseDto>>(json, options)
                       ?? new List<StudentResponseDto>();

        if (students.Count == 0)
        {
            Console.WriteLine("No students found.");
            return;
        }

        PrintSectionHeader("All Students");
        Console.WriteLine($"  {"ID",-6} {"Name",-25} {"Major"}");
        Console.WriteLine($"  {new string('-', 50)}");

        foreach (var s in students)
            Console.WriteLine($"  {s.Id,-6} {s.Name,-25} {s.Major}");
    }
    catch (HttpRequestException)
    {
        PrintError("Cannot connect to StudentAPI. Is it running?");
    }
}

// ── View student by ID ─────────────────────────────────────────────────────
async Task ViewStudentAsync()
{
    Console.Write("Enter student ID: ");
    string input = Console.ReadLine()?.Trim() ?? "";

    if (!int.TryParse(input, out int id) || id <= 0)
    {
        PrintError("ID must be a positive number.");
        return;
    }

    try
    {
        var response = await client.GetAsync($"/students/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            PrintError($"No student found with ID {id}.");
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            PrintError($"API error: {response.StatusCode}");
            return;
        }

        string json = await response.Content.ReadAsStringAsync();
        var student = JsonSerializer.Deserialize<StudentResponseDto>(json, options);

        if (student == null) { PrintError("Could not read student data."); return; }

        PrintSectionHeader("Student Details");
        Console.WriteLine($"  ID    : {student.Id}");
        Console.WriteLine($"  Name  : {student.Name}");
        Console.WriteLine($"  Major : {student.Major}");
    }
    catch (HttpRequestException)
    {
        PrintError("Cannot connect to StudentAPI. Is it running?");
    }
}

// ── Create new student ─────────────────────────────────────────────────────
async Task CreateStudentAsync()
{
    PrintSectionHeader("Create New Student");

    Console.Write("  Name  : ");
    string name = Console.ReadLine()?.Trim() ?? "";
    if (string.IsNullOrEmpty(name)) { PrintError("Name is required."); return; }

    Console.Write("  Age   : ");
    string ageInput = Console.ReadLine()?.Trim() ?? "";
    if (!int.TryParse(ageInput, out int age) || age <= 0)
    {
        PrintError("Age must be a positive number.");
        return;
    }

    Console.Write("  Major : ");
    string major = Console.ReadLine()?.Trim() ?? "";
    if (string.IsNullOrEmpty(major)) { PrintError("Major is required."); return; }

    var dto = new CreateStudentDto { Name = name, Age = age, Major = major };

    try
    {
        var response = await client.PostAsJsonAsync("/students", dto);

        if (response.IsSuccessStatusCode)
            PrintSuccess($"Student '{name}' created successfully.");
        else
            PrintError($"Failed to create student: {response.StatusCode}");
    }
    catch (HttpRequestException)
    {
        PrintError("Cannot connect to StudentAPI. Is it running?");
    }
}

// ── Helpers ────────────────────────────────────────────────────────────────
void PrintHeader()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("==============================");
    Console.WriteLine("  Student Dashboard (Console)");
    Console.WriteLine("==============================");
    Console.ResetColor();
    Console.WriteLine();
}

void PrintSectionHeader(string title)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"--- {title} ---");
    Console.ResetColor();
    Console.WriteLine();
}

void PrintError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  Error: {message}");
    Console.ResetColor();
}

void PrintSuccess(string message)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  {message}");
    Console.ResetColor();
}

 public class CreateStudentDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Major { get; set; } = string.Empty;
    }
     public class StudentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
    }