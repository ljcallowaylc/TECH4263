using System.ComponentModel.DataAnnotations.Schema;

namespace StudentAPI.Models;

public class Student
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Major { get; set; } = string.Empty;
}