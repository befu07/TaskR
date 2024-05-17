using TaskR.Data;

namespace TaskR.Models;

public class ToDoIndexVm
{
    public string Name { get; set; }
    public List<ToDoList> ToDoLists { get; set; } = new();
    public int MaxLists { get; set; }
    public bool IsFreeUser { get; set; }
    public bool IsPremiumUser { get; set; }
    public int RemainingLists => MaxLists-ToDoLists.Count;
}
