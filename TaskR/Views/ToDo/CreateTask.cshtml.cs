using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TaskR.Data;
using TaskR.Helper;

namespace TaskR.Models;
public class CreateTaskVm
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Descripton { get; set; } = null!;
    [Required] public int ToDoListId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedOn { get; set; }
    public DateTime CreatedOn { get; set; }

    //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    [FutureDate]
    public DateTime? Deadline { get; set; }
    public int[]? SelectedTagIds { get; set; }
    public int? Priority { get; set; } = 3;

    #region UI Properties
    public string IsCompletedString { get; set; } = "off";
    public string DeadlineInputString { get; set; } = "";
    public List<SelectListItem>? SelectListItems_Priorities { get; set; } = new();
    public List<SelectListItem>? SelectListItems_ToDoList { get; set; } = new();
    public MultiSelectList? MSL_Tags { get; set; }
    public Dictionary<int, Tag> TagsDict { get; set; } = new();
    #endregion
}
