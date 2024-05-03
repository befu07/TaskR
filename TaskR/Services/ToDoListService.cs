using Microsoft.EntityFrameworkCore;
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
    }
}
