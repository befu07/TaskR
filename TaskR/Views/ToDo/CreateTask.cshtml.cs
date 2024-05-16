using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TaskR.Data;

namespace TaskR.Models;
public class CreateTaskVm
{
    [Required]
    [MaxLength(100)]
    public string Descripton { get; set; } = null!;
    [Required] public int ToDoListId { get; set; }
    public bool IsCompleted { get; set; }

    public DateTime CreatedOn { get; set; }
    //public DateTime CompletedOn { get; set; }
    public DateTime? Deadline { get; set; }

    public int Priority { get; set; } = 3;

    #region UI Properties
    public List<SelectListItem>? SelectListItems_Priorities { get; set; } = new();
    public List<SelectListItem>? SelectListItems_ToDoList { get; set; } = new();
    public List<SelectListItem>? SelectListItems_Tags { get; set; } = new();
    public List<Tag> AvailableTags { get; set; } = new();
    public List<Tag> SelectedTags { get; set; } = new();
    public int[]? SelectedTagIds { get; set; }
    public MultiSelectList? MSL_Tags { get; set; }
    #endregion
}
