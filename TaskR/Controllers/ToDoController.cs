using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult Create()
        {
            //Todo
            return View();
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
                return RedirectToAction(nameof(Details), routeValues: new {id= vm.ToDoListId });
            //return RedirectToAction(nameof(Index), HomeController.Name);
                //return View(nameof(Index));
                //return RedirectToAction(nameof(Index));
            }
            else
            {

            var testln = vm;
            return RedirectToAction(nameof(Index), HomeController.Name);
            }


        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var list = await _toDoListService.GetToDoListByIdAsync(id);
            if (list == null) { return RedirectToAction(nameof(Index)); }
            var vm = new ToDoDetailsVm
            {
                Id = list.Id,
                Name = list.Name,
                Tasks = list.Tasks
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Create(ToDoCreateVm vm)
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

    }
}
