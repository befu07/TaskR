using Microsoft.AspNetCore.Mvc;
using TaskR.Data;
using TaskR.Services;
using TaskR.Views.Admin;

namespace TaskR.Controllers
{
    public class AdminController : Controller
    {
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
