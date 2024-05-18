using TaskR.Data;
using static TaskR.Services.ToDoListService;
using Task = TaskR.Data.Task;

namespace TaskR.Models;
public class TDLDetailsVm
{
    public int Id { get; set; }
    public TaskFilter Filter { get; set; } = 0;
    public string Query { get; set; } = string.Empty;
    public string Name { get; set; } = null!;
    public virtual IEnumerable<Task> Tasks { get; set; } = new List<Task>();
}