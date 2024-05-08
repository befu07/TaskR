using TaskR.Data;
using Task = TaskR.Data.Task;

namespace TaskR.Models;
public class ToDoDetailsVm
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual IEnumerable<Task> Tasks { get; set; } = new List<Task>();
}