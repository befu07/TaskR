using TaskR.Data;

//namespace TaskR.Views.Admin
namespace TaskR.Models
{
    public class UserOverViewVm
    {
        public List<AppUser> AppUsers { get; set; }
        public Dictionary<int,string> AppRoleDict { get; set; } = new();
    }
}
