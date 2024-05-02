using System;
using System.Collections.Generic;

namespace TaskR.Data;

public partial class ToDoList
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? AppUserId { get; set; }

    public virtual AppUser? AppUser { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
