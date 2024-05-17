using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using TaskR.Models;
using TaskR.Services;

namespace TaskR.Controllers
{
    //[Authorize(Roles = "FreeUser")]
    //[Authorize(Roles = "PremiumUser")]
    [Authorize(Roles = "FreeUser,PremiumUser")]
    public class ToDoController : Controller
    {

        public static string Name = nameof(ToDoController).Replace("Controller", null);
        private readonly ToDoListService _toDoListService;
        private readonly AccountService _accountService;

        public ToDoController(ToDoListService toDoListService, AccountService accountService)
        {
            _toDoListService = toDoListService;
            _accountService = accountService;
        }
        public async Task<IActionResult> Index()
        {
            var userName = this.User.Identity.Name;
            var userId = await _accountService.GetAppUserIdByNameAsync(userName);
            var lists = await _toDoListService.GetToDoListsByUserIdAsync(userId);
            var vm = new ToDoIndexVm
            {
                Name = userName,
                ToDoLists = lists
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTask(int id)
        {
            var userId = await _accountService.GetAppUserIdByNameAsync(this.User.Identity.Name);
            var tdlSelectList = await _toDoListService.GetTDLSelectListByUserIdAsync(userId);
            var priorities = _toDoListService.GetPrioritySelectList();
            var tags = await _toDoListService.GetAvailableTagsAsync();
            var vm = new CreateTaskVm
            {
                MSL_Tags = new MultiSelectList(tags, "Id", "Name"),
                ToDoListId = id,
                AvailableTags = tags,
                SelectListItems_ToDoList = tdlSelectList,
                SelectListItems_Priorities = priorities,
                SelectListItems_Tags = _toDoListService.GetTagsSelectList(tags)
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskVm vm)
        {
            if (ModelState.IsValid)
            {
                var selectedTags = await _toDoListService.GetTagsByIntArrayAsync(vm.SelectedTagIds);
                TaskR.Data.Task task = new TaskR.Data.Task
                {
                    ToDoListId = vm.ToDoListId,
                    Descripton = vm.Descripton,
                    Deadline = vm.Deadline,
                    IsCompleted = false,
                    CreatedOn = DateTime.Now,
                    Priority = vm.Priority,
                    Tags = selectedTags
                };
                //TODO 
                await _toDoListService.CreateNewTaskItemAsync(task);
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = vm.ToDoListId });
            }
            else
            {
                var errormessages = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                var errorstring = string.Join(", ", errormessages);
                TempData["ErrorMessage"] = errorstring;
                return RedirectToAction(nameof(CreateTask), ToDoController.Name, routeValues: new { id = vm.ToDoListId });
                /*
                    return RedirectToAction(nameof(CreateTask), ToDoController.Name, fragment: vm.ToDoListId.ToString());
                    return View(nameof(CreateTask));
                    return View(nameof(CreateTask), model: new {id = vm.ToDoListId});
                    return RedirectToAction(nameof(Index), HomeController.Name);
                 */
            }
        }

        [HttpGet]
        public async Task<IActionResult> TDLDetails(int id)
        {
            var list = await _toDoListService.GetToDoListByIdAsync(id);
            if (list == null) { return RedirectToAction(nameof(Index)); }
            var vm = new TDLDetailsVm
            {
                Id = list.Id,
                Name = list.Name,
                Tasks = list.Tasks
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> TDLCreate(ToDoCreateVm vm)
        {
            if (ModelState.IsValid)
            {
                var userId = await _accountService.GetAppUserIdByNameAsync(this.User.Identity.Name);
                bool success = await _toDoListService.CreateNewListAsync(userId, vm.Name);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Name existiert bereits";
                    return View();
                }
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpGet]
        public IActionResult TDLCreate()
        {
            //Todo
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> TDLDelete(int id)
        {
            var result = await _toDoListService.DeleteToDoListByIdAsync(id);
            if (result == 1)
            {
                TempData["DeleteMessage"] = "Eintrag gelöscht!";
                return RedirectToAction(nameof(Index));
            }
            else if(result > 1)
            {
                TempData["DeleteMessage"] = $"Liste und verknüpfte Aufgaben gelöscht!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["DeleteError"] = "Löschen fehlgeschlagen!";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpGet]
        public async Task<IActionResult> TaskDelete(int id, int listID)
        {
            var result = await _toDoListService.DeleteTaskByIdAsync(id);
            if (result == 1)
            {
                TempData["DeleteMessage"] = "Aufgabe gelöscht!";
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = listID });
            }
            else
            {
                TempData["DeleteError"] = "Löschen fehlgeschlagen!";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
