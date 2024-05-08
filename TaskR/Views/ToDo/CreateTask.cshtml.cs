using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TaskR.Models;
public class CreateTaskVm
{
    [Required]
    [MaxLength(100)]
    public string Descripton { get; set; } = null!;
    [Required] public int ToDoListId { get; set; }
    [Required] public bool IsCompleted { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime CompletedOn { get; set; }
    public DateTime Deadline { get; set; }

    public int Priority { get; set; } = 3;

    #region UI Properties
    public List<SelectListItem> Priorities { get; set; }
    public List<SelectListItem> SelectList_ToDoList { get; set; }
    public SelectList SelectList_Tags { get; set; }
    #endregion
}
