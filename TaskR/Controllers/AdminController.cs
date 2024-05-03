using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskR.Data;
using TaskR.Services;
using TaskR.Models;

namespace TaskR.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public static string Name = nameof(AdminController).Replace("Controller", null);
        private readonly AccountService accountService;

        public AdminController(AccountService accountService)
        {
            this.accountService = accountService;
        }
        public async Task<IActionResult> UserOverView()
        {
            UserOverViewVm vm = new();
            vm.AppUsers = await accountService.GetAllUsers();
            return View(vm);
        }
    }
}
