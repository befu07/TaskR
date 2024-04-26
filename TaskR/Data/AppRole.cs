﻿using System;
using System.Collections.Generic;

namespace TaskR.Data;

public partial class AppRole
{
    public int Id { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
}
