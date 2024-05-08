using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var vm = new CreateTaskVm
            {
                ToDoListId = id,
                SelectList_ToDoList = tdlSelectList,
                Priorities = priorities
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskVm vm)
        {
            //TODO 
            var testln = vm;


            return RedirectToAction(nameof(Index));
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
