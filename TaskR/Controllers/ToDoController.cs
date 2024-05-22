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
        public async Task<IActionResult> CreateTask(int id)
        {
            var userId = await _accountService.GetAppUserIdByNameAsync(this.User.Identity.Name);
            var tdlSelectList = await _toDoListService.GetTDLSelectListByUserIdAsync(userId);
            var priorities = _toDoListService.GetPrioritySelectList();

            var globaltags = await _toDoListService.GetGlobalTagsAsync();
            var usertags = await _toDoListService.GetUserTagsAsync(userId);
            var vmtags = globaltags.Concat(usertags).ToList();

            var vm = new CreateTaskVm
            {
                ToDoListId = id,
                MSL_Tags = new MultiSelectList(vmtags, "Id", "Name"),
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
                Tasks = list.TaskItems
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
        [HttpGet]
        public async Task<IActionResult> TaskDetails(int id)
        {
            var result = await _toDoListService.GetTaskByIdAsync(id);
            if (result == null)
                return RedirectToAction(nameof(Index));

            var userId = result.ToDoList.AppUserId;
            var tdlSelectList = await _toDoListService.GetTDLSelectListByUserIdAsync(userId);
            var vm = new CreateTaskVm
            {
                Id = id,
                Descripton = result.Description,
                ToDoListId = result.ToDoListId,
                SelectListItems_ToDoList = tdlSelectList,
                IsCompleted = result.IsCompleted,
                CreatedOn = result.CreatedOn,
                CompletedOn = result.CompletedOn,
                Deadline = result.Deadline,
                DeadlineInputString = result.Deadline.ToInputString(),
                Priority = result.Priority,


            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> TaskDetails(CreateTaskVm vm)
        {
            if (ModelState.IsValid)
            {
                TaskItem dbTask = await _toDoListService.GetTaskByIdAsync(vm.Id);
                if (dbTask == null)
                {
                    // Todo Fehlermedlung
                    return RedirectToAction(nameof(Index));
                }

                if (vm.IsCompletedString == "on")
                {
                    vm.IsCompleted = true;
                    if (!dbTask.IsCompleted)
                    {
                        dbTask.IsCompleted = true;
                        dbTask.CompletedOn = DateTime.Now;
                    }
                }
                //Todo Update Entry
                dbTask.Id = vm.Id;
                dbTask.ToDoListId = vm.ToDoListId;
                dbTask.Description = vm.Descripton;
                dbTask.IsCompleted = false;
                dbTask.CreatedOn = vm.CreatedOn;
                dbTask.CompletedOn = vm.CompletedOn;
                dbTask.Deadline = vm.Deadline;
                dbTask.Priority = vm.Priority;
                //dbTask.Tags = selectedTags // TODO
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var result = await _toDoListService.GetTaskByIdAsync(vm.Id);
                if (result == null)
                    return RedirectToAction(nameof(Index));

                var userId = result.ToDoList.AppUserId;
                var tdlSelectList = await _toDoListService.GetTDLSelectListByUserIdAsync(userId);
                var errorvm = new CreateTaskVm
                {
                    Id = vm.Id,
                    Descripton = result.Description,
                    ToDoListId = result.ToDoListId,
                    SelectListItems_ToDoList = tdlSelectList,
                    IsCompleted = result.IsCompleted,
                    CreatedOn = result.CreatedOn,
                    CompletedOn = result.CompletedOn,
                    Deadline = result.Deadline,
                    DeadlineInputString = result.Deadline.ToInputString(),
                    Priority = result.Priority,
                };
                return View(errorvm);
                //TempData["ErrorMessage"] = 
                ModelState.AddModelError("Description", "blah");
                return RedirectToAction(nameof(TaskDetails), routeValues: new { id = vm.Id }); // validation geht so nicht
                //Todo Selectlisten
                //return View(nameof(TaskDetails), new {id=vm.Id});
            }
        }

    }
}
