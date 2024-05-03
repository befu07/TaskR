using TaskR.Data;

namespace TaskR.Models;

public class ToDoIndexVm
{
    public string Name { get; set; }
    public List<ToDoList> ToDoLists { get; set; } = new();
}
