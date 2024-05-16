using System.ComponentModel.DataAnnotations;
using TaskR.Data;

namespace TaskR.Models;
public class ToDoCreateVm
{
    [Required]
    [MaxLength(30)]
    public string Name { get; set; } = null!;
}
