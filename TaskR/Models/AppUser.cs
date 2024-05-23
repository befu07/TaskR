using System;
using System.Collections.Generic;

namespace TaskR.Data;

public partial class AppUser
{
    public bool IsAdmin => AppRoleId == 1;
}
