using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskR.Data;
using TaskR.Models;
using TaskR.Services;

namespace TaskR.Controllers
{
    [Authorize(Roles = "Admin, FreeUser, PremiumUser")]
    public class TagsController : Controller
    {

        public static string Name = nameof(TagsController).Replace("Controller", null);
        private readonly ToDoListService _toDoListService;
        private readonly AccountService _accountService;

        public TagsController(ToDoListService toDoListService, AccountService accountService)
        {
            _toDoListService = toDoListService;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = await _accountService.GetAppUserIdByNameAsync(this.User.Identity.Name);
            List<Data.Tag> tags = new();
            if (User.IsInRole("Admin"))
            {
                tags = await _toDoListService.GetGlobalTagsAsync();
            }
            else
            {
                tags = await _toDoListService.GetUserTagsAsync(userId);
            }
            TagIndexVm vm = new TagIndexVm()
            {
                Tags = tags.ToDictionary(o => o.Id),
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTag(TagIndexVm vm)
        {
            if (ModelState.IsValid)
            {
                var tag = new Tag
                {
                    HexColor = vm.HexColor.Replace("#", ""),
                    Name = vm.Name
                };
                if (!User.IsInRole("Admin"))
                {
                    tag.AppUserId = await _accountService.GetAppUserIdByNameAsync(this.User.Identity.Name);
                }
                int result;
                if (vm.Id == null)
                {
                    await Console.Out.WriteLineAsync("newtag" + vm.Id + " " + vm.Name + " " + vm.HexColor);
                    result = await _toDoListService.CreateTagAsync(tag);
                }
                else
                {
                    await Console.Out.WriteLineAsync("Id:" + vm.Id + " " + vm.Name + " " + vm.HexColor);
                    tag.Id = (int)vm.Id;
                    result = await _toDoListService.UpdateTagAsync(tag);
                }
                if (result == -1)
                {
                    TempData["ErrorMessage"] = "gehtned";
                }
                else if (result == 1)
                {
                    TempData["SuccessMessage"] = "Eintrag geUpdated";
                }
                else if (result == 0)
                {
                    TempData["ErrorMessage"] = "keine änderungen";
                }
                else
                {
                    TempData["ErrorMessage"] = "gehtned";
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errormessages = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                var errorstring = string.Join("\n ", errormessages);
                TempData["ErrorMessage"] = errorstring;
                return RedirectToAction(nameof(Index));
            }


        }

        public async Task<IActionResult> DeleteTag(int id)
        {
            int result = await _toDoListService.TryDeleteTagByIdAsync(id);
            if (result == -1)
            {
                TempData["ErrorMessage"] = "Vorgang fehlgeschlagen";
            }
            if (result == -2)
            {
                TempData["ErrorMessage"] = "Verwendete Tags können nicht gelöscht werden";
            }
            if(result >= 1)
            {
                TempData["SuccessMessage"] = "Eintrag gelöscht";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
