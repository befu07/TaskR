using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TaskR.Data;

namespace TaskR.Services
{
    public class AccountService
    {
        private readonly CryptoService256 _cryptoService;
        private readonly TaskRContext _ctx;

        public AccountService(CryptoService256 cryptoService, TaskRContext ctx)
        {
            _cryptoService = cryptoService;
            _ctx = ctx;
        }

        public async Task<bool> RegisterNewUserAsync(string username, string password, string email)
        {
            //Überprüfungen
            bool emailExists = await _ctx.AppUsers.Where(o => o.Email == email).AnyAsync();
            if (emailExists) { return false; }
            bool firstUserInDatabase = !(await _ctx.AppUsers.AnyAsync());
            //Salt erzeugen
            var salt = _cryptoService.GenerateSalt();

            //Salt an Passwort hängen
            var saltedPassword = _cryptoService.SaltString(password, salt, System.Text.Encoding.UTF8);

            //Gesaltetes Passwort Hashen
            var hash = _cryptoService.GetHash(saltedPassword);

            //Benutzer in Datenbank speichern
            var newUser = new AppUser
            {
                PasswordHash = hash,
                RegisteredOn = DateTime.Now,
                Salt = salt,
                Email = email,
                Username = username
            };
            if (firstUserInDatabase)
                newUser.AppRoleId = 1; // Wenn sonst keine Benutzer in DB, AdminRolle 

            _ctx.AppUsers.Add(newUser);

            await _ctx.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CanUserLogInAsync(string email, string loginPassword)
        {
            //Benutzer in DB suchen und laden
            var dbAppUser = await _ctx.AppUsers.Where(x => x.Email == email).FirstOrDefaultAsync();

            //Wenn Benutzer existiert...
            if (dbAppUser is null) return false;

            //Login-Passwort mit Salt aus DB salten
            var saltedLoginPassword = _cryptoService.SaltString(loginPassword, dbAppUser.Salt, System.Text.Encoding.UTF8);

            //Das gesaltete Login-PW hashen
            var hashedLoginPassword = _cryptoService.GetHash(saltedLoginPassword);

            //Den Login-PW-Hash mit dem Hash des PW aus der DB vergleichen
            //Wenn gleich, dann darf der Benutzer einloggen
            //Sonst nicht
            return hashedLoginPassword.SequenceEqual(dbAppUser.PasswordHash);

            //Achtung! Die beiden byte[]s können NICHT einfach mit == verglichen werden, da es sich um Verweis-Datentypen handelt
            //Bei Verweisdatentypen wird mit == immer die Verweise verglichen (und nicht der Inhalt) 
        }

        //internal async Task<AppRole> GetRoleByUserNameAsync(string username)
        //{
        //    var user = await _ctx.AppUsers.Include(o => o.AppRole).Where(x => x.Username == username).FirstOrDefaultAsync();
        //    return user.AppRole;
        //}
        internal async Task<int> GetAppUserIdByNameAsync(string username)
        {
            var user = await _ctx.AppUsers.Where(x => x.Username == username).FirstOrDefaultAsync();
            return user.Id;
        }

        internal async Task<List<AppUser>> GetAllUsersAsync()
        {
            return await _ctx.AppUsers.Include(o => o.AppRole).Include(o => o.Tags).Include(o => o.ToDoLists).ThenInclude(o => o.TaskItems).OrderBy(o => o.Email).ToListAsync();
        }

        internal async Task<Dictionary<int, string>> GetRolesDictAsync()
        {
            return await _ctx.AppRoles.ToDictionaryAsync(o => o.Id, o => o.RoleName);
        }

        internal async Task<int> UpdateUserRoleAsync(int id, int appRoleId)
        {
            var user = await _ctx.AppUsers.Where(o => o.Id == id).Include(o => o.ToDoLists).FirstOrDefaultAsync();
            if (user is null) { return -1; }

            #region LastAdmin 
            if (user.IsAdmin & appRoleId != 1)
            {
                int AdminCount = await _ctx.AppUsers.CountAsync(o => o.AppRoleId == 1);
                if (AdminCount <= 1)
                {
                    return -1;
                }
            }
            #endregion

            #region delete Lists
            if (appRoleId == 1)
            {
                var userlists = await _ctx.ToDoLists.Where(o => o.AppUserId == user.Id).ToListAsync();
                _ctx.ToDoLists.RemoveRange(userlists);
            }
            #endregion

            user.AppRoleId = appRoleId;
            return await _ctx.SaveChangesAsync();
        }

        internal async Task<AppUser> GetUserByEmail(string email)
        {
            var user = await _ctx.AppUsers.Include(o => o.AppRole).Where(x => x.Email == email).FirstOrDefaultAsync();
            return user;
        }
    }
}
