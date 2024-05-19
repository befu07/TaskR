
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TaskR.Helper;

namespace TaskR.Models;

public class TaskDetailsVm
{
    public int Id { get; set; }
    [Required] public int ToDoListId { get; set; }
    public List<SelectListItem>? SelectListItems_ToDoList { get; set; } = new();
    [Required]
    [MaxLength(100)]
    public string Descripton { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public string IsCompletedString { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
    [FutureDate]
    public DateTime? Deadline { get; set; }
    public string DeadlineInputString { get; set; } = "";
    public int? Priority { get; set; }

}
