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
        private readonly AccountService _accountService;

        public AdminController(AccountService accountService)
        {
            this._accountService = accountService;
        }
        public async Task<IActionResult> UserOverView()
        {
            UserOverViewVm vm = new();
            var users = await _accountService.GetAllUsersAsync();
            users.OrderBy(o => o.Email);
            vm.AppUsers = users;
            vm.AppRoleDict = await _accountService.GetRolesDictAsync();
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> UserUpdate(int Id, int AppRoleId)
        {
            //todo Update Approle

            return RedirectToAction(nameof(UserOverView));
        }
    }
}
