using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Collections.Generic;
using System.Security.Claims;
using TaskR.Data;
using TaskR.Helper;
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
            var isPremiumUser = this.User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "PremiumUser");
            int maxLists;
            if (isPremiumUser)
            {
                maxLists = 100;
            }
            else
            {
                maxLists = 5;
            }

            var vm = new ToDoIndexVm
            {
                Name = userName,
                ToDoLists = lists,
                MaxLists = maxLists,
                IsPremiumUser = isPremiumUser,
                IsFreeUser = !isPremiumUser
            };
            return View(vm);
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
                Tasks = list.TaskItems.OrderByDescending(o => o.IsUrgent()).ThenBy(o => o.Priority).ToList()
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> TDLDetails(TDLDetailsVm form)
        {
            var toDoList = await _toDoListService.GetToDoListByIdAsync(form.Id);
            if (toDoList == null) { return RedirectToAction(nameof(Index)); }

            // Filter anwenden
            var filter = form.Filter;
            var textquery = form.Query;
            var filteredTasks = _toDoListService.GetFilteredToDoList(toDoList.TaskItems, filter, textquery);
            var vm = new TDLDetailsVm
            {
                Id = toDoList.Id,
                Name = toDoList.Name,
                Tasks = filteredTasks,
                Filter = form.Filter,
                Query = form.Query
            };
            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> TDLCreate()
        {
            //Todo schöner machen?
            return View();
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
                int id = await _toDoListService.GetListIdAsync(userId, vm.Name);
                if (id < 1)
                {
                    TempData["ErrorMessage"] = "Redirect failed";
                    return View();
                }
                else
                {
                    return RedirectToAction(nameof(TDLDetails), routeValues: new { id = id });
                }
                //return RedirectToAction(nameof(Index));
            }
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
            else if (result > 1)
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
        public async Task<IActionResult> CreateTask(int id)
        {
            var userId = await _accountService.GetAppUserIdByNameAsync(this.User.Identity.Name);
            var tdlSelectList = await _toDoListService.GetTDLSelectListByUserIdAsync(userId);
            var priorities = _toDoListService.GetPrioritySelectList();

            var globaltags = await _toDoListService.GetGlobalTagsAsync();
            var usertags = await _toDoListService.GetUserTagsAsync(userId);
            var vmtags = usertags.Concat(globaltags).ToList();

            var vm = new CreateTaskVm
            {
                ToDoListId = id,
                MSL_Tags = new MultiSelectList(vmtags, "Id", "Name"),
                TagsDict = vmtags.ToDictionary(o => o.Id),
                SelectListItems_ToDoList = tdlSelectList,
                SelectListItems_Priorities = priorities,
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskVm vm)
        {
            if (ModelState.IsValid)
            {
                var selectedTags = await _toDoListService.GetTagsByIntArrayAsync(vm.SelectedTagIds);
                TaskItem task = new TaskItem
                {
                    ToDoListId = vm.ToDoListId,
                    Description = vm.Descripton,
                    Deadline = vm.Deadline,
                    IsCompleted = false,
                    CreatedOn = DateTime.Now,
                    Priority = vm.Priority,
                    Tags = selectedTags
                };
                await _toDoListService.CreateNewTaskItemAsync(task);
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = vm.ToDoListId });
            }
            else
            {
                var errormessages = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                var errorstring = string.Join(", ", errormessages);
                TempData["ErrorMessage"] = errorstring;
                return RedirectToAction(nameof(CreateTask), ToDoController.Name, routeValues: new { id = vm.ToDoListId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> TaskDelete(int id, int listID)
        {
            var result = await _toDoListService.DeleteTaskByIdAsync(id);
            if (result >= 1)
            {
                TempData["SuccessMessage"] = "Aufgabe gelöscht!";
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = listID });
            }
            else
            {
                TempData["ErrorMessage"] = "Löschen fehlgeschlagen!";
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = listID });
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteCompleted(int id)
        {
            var result = await _toDoListService.DeleteCompletedTasksByListIdAsync(id);

            if (result >= 1)
            {
                TempData["SuccessMessage"] = "Fertige Aufgaben gelöscht!";
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = id });
            }
            else if (result == -2)
            {
                TempData["ErrorMessage"] = "Liste enthält keine fertigen Aufgaben!";
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = id });
            }
            else
            {
                TempData["ErrorMessage"] = "Löschen fehlgeschlagen!";
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = id });
            }
        }
        [HttpGet]
        public async Task<IActionResult> TaskComplete(int id, int listID)
        {
            var result = await _toDoListService.CompleteTaskByIdAsync(id); // Todo
            switch (result)
            {
                TempData["SuccessMessage"] = "Aufgabe abgeschlossen!";
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = listID });
            }
            else if (result == -2)
            {
                TempData["ErrorMessage"] = "Aufgabe bereits abgeschlossen!";
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = listID });
            }
            else
            {
                TempData["ErrorMessage"] = "Vorgang fehlgeschlagen!";
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = listID });
            }
        }
        [HttpGet]
        public async Task<IActionResult> TaskDetails(int id)
        {
            var result = await _toDoListService.GetTaskByIdAsync(id);
            if (result == null)
                return RedirectToAction(nameof(Index));

            var userId = result.ToDoList.AppUserId;
            var tdlSelectList = await _toDoListService.GetTDLSelectListByUserIdAsync(userId);
            var priorities = _toDoListService.GetPrioritySelectList();

            var globaltags = await _toDoListService.GetGlobalTagsAsync();
            var usertags = await _toDoListService.GetUserTagsAsync(userId);
            var vmtags = usertags.Concat(globaltags).ToList();

            var vm = new CreateTaskVm
            {
                Id = id,
                Descripton = result.Description,
                ToDoListId = result.ToDoListId,
                IsCompleted = result.IsCompleted,
                CreatedOn = result.CreatedOn,
                CompletedOn = result.CompletedOn,
                Deadline = result.Deadline,
                DeadlineInputString = result.Deadline.ToInputString(),
                Priority = result.Priority,
                SelectListItems_ToDoList = tdlSelectList,
                SelectListItems_Priorities = priorities,

                MSL_Tags = new MultiSelectList(result.Tags, "Id", "Name"), // Todo unnedig
                SelectedTagIds = result.Tags.Select(t => t.Id).ToArray(),
                TagsDict = vmtags.ToDictionary(o => o.Id)
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> TaskUpdate(CreateTaskVm vm)
        {
            if (ModelState.IsValid)
            {
                TaskItem task = new()
                {
                    Id = vm.Id,
                    ToDoListId = vm.ToDoListId,
                    Description = vm.Descripton,
                    Deadline = vm.Deadline,
                    Priority = vm.Priority,
                };
                if (vm.SelectedTagIds != null)
                {
                    var tags = await _toDoListService.GetTagsByIntArrayAsync(vm.SelectedTagIds);
                    task.Tags = tags;
                }
                if (vm.IsCompletedString == "on")
                {
                    task.IsCompleted = true;
                }

                int result = await _toDoListService.UpdateTaskAsync(task);

                if (result == 0)
                {
                    TempData["ErrorMessage"] = "no changes";
                }
                if (result < 1)
                {
                    TempData["ErrorMessage"] = "shit happened";
                }
                if (result == 1)
                {
                    TempData["SuccessMessage"] = "Aufgabe upgedated";
                }
                if (result > 1)
                {
                    TempData["SuccessMessage"] = "Aufgabe upgedated und nochwas is passiert ka was wahrsch wegen tags";
                }
                return RedirectToAction(nameof(TDLDetails), routeValues: new { id = vm.ToDoListId });
            }
            else
            {
                var errormessages = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                var errorstring = string.Join(", ", errormessages);
                TempData["ErrorMessage"] = errorstring;
                return RedirectToAction(nameof(TaskDetails), routeValues: new { id = vm.Id });
            }
        }
        [HttpPost]
        public async Task<IActionResult> TaskPriorityUpdate(int id, int priority, int tdlid)
        {
            int result = await _toDoListService.UpdateTaskPriorityAsync(id, priority);
            switch (result)
            {
                case 1:
                    TempData["SuccessMessage"] = "Aufgabe upgedated";
                    break;
                case -1:
                default:
                    TempData["ErrorMessage"] = "shit happened";
                    break;


            }
            return RedirectToAction(nameof(TDLDetails), routeValues: new { id = tdlid });
        }

    }
}
