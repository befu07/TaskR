using System;
using System.Collections.Generic;

namespace TaskR.Data;

public partial class TaskItem
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int ToDoListId { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public DateTime? Deadline { get; set; }

    public int? Priority { get; set; }

    public virtual ToDoList ToDoList { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
