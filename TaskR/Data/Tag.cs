using System;
using System.Collections.Generic;

namespace TaskR.Data;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string HexColor { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
