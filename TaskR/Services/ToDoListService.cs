using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TaskR.Data;

namespace TaskR.Services
{
    public class ToDoListService
    {
        private readonly TaskRContext _ctx;

        public ToDoListService(TaskRContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<bool> CreateNewListAsync(int userId, string name)
        {
            bool nameExists = await _ctx.ToDoLists.Where(o => o.Name == name & o.AppUserId == userId).AnyAsync();
            if (nameExists) { return false; }
            var newList = new ToDoList { AppUserId = userId, Name = name };
            _ctx.ToDoLists.Add(newList);
            await _ctx.SaveChangesAsync();
            return true;
        }
        internal async Task<List<ToDoList>> GetToDoListsByUserNameAsync(string username)
        {
            var user = await _ctx.AppUsers.Where(x => x.Username == username).FirstOrDefaultAsync();
            if (user is null) return new();
            return (await _ctx.ToDoLists.Where(x => x.AppUserId == user.Id).ToListAsync());
        }

        internal async Task<List<ToDoList>> GetAllToDoListsAsync()
        {
            return await _ctx.ToDoLists.ToListAsync();
        }

        internal async Task<List<ToDoList>> GetToDoListsByUserIdAsync(int userId)
        {
            return (await _ctx.ToDoLists.Include(o => o.Tasks).Where(x => x.AppUserId == userId).ToListAsync());
        }

        internal async Task<ToDoList?> GetToDoListByIdAsync(int id)
        {
            return await _ctx.ToDoLists.Where(x => x.Id == id).Include(o => o.Tasks).ThenInclude(o => o.Tags).FirstOrDefaultAsync();
            //return await _ctx.ToDoLists.FindAsync(id);
        }
        internal async Task<List<SelectListItem>> GetTDLSelectListByUserIdAsync(int id)
        {
            var items = await _ctx.ToDoLists
                .Where(x => x.AppUserId == id)
                .ToDictionaryAsync(o => o.Id);
            var selectlistitems = items.Select(
                o => new SelectListItem(o.Value.Name, o.Key.ToString())
                );
            var selectList = new List<SelectListItem>(selectlistitems);
            return selectList;
        }

        internal List<SelectListItem> GetPrioritySelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem("Höchste","1"),
                new SelectListItem("Hoch","2"),
                new SelectListItem("Normal","3"),
                new SelectListItem("Niedrig","4"),
                new SelectListItem("Keine","5")
            };
        }
        internal async Task<List<Tag>> GetAvailableTagsAsync()
        {
            return await _ctx.Tags.ToListAsync();
        }
        internal List<SelectListItem> GetTagsSelectList(List<Tag> tasks)
        {
            var list = new List<SelectListItem>();
            foreach (var task in tasks)
            {
                list.Add(new SelectListItem(task.Name, task.Id.ToString()));
            }
            return list;
        }

        internal async Task<List<Tag>> GetTagsByIntArrayAsync(int[] selectedTagIds)
        {
            return await _ctx.Tags.Where(o => selectedTagIds.Contains(o.Id)).ToListAsync();
        }

        internal async System.Threading.Tasks.Task CreateNewTaskItemAsync(Data.Task task)
        {
            await _ctx.Tasks.AddAsync(task);
            await _ctx.SaveChangesAsync();
        }

        internal async System.Threading.Tasks.Task<int> DeleteToDoListByIdAsync(int id)
        {
            var tdl = await GetToDoListByIdAsync(id);
            _ctx.ToDoLists.Remove(tdl);
            return await _ctx.SaveChangesAsync(true);
        }

        internal async Task<int> DeleteTaskByIdAsync(int id)
        {
            var task = await GetTaskByIdAsync(id) ?? new Data.Task();

            _ctx.Tasks.Remove(task);
            return await _ctx.SaveChangesAsync(true);
        }

        private async Task<Data.Task?> GetTaskByIdAsync(int id)
        {
            return await _ctx.Tasks.Where(o=>o.Id == id).Include(o=>o.Tags).SingleOrDefaultAsync();
        }

        internal async Task<int> GetListIdAsync(int userId, string name)
        {
            var list = await _ctx.ToDoLists.Where(o=>o.AppUserId==userId & o.Name==name).SingleOrDefaultAsync();

            return list?.Id ?? 0;
        }
    }
}
