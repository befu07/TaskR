using Microsoft.EntityFrameworkCore;
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
            if(emailExists) { return false; }
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

        public async Task<bool> CanUserLogInAsync(string username, string loginPassword)
        {
            //Benutzer in DB suchen und laden
            var dbAppUser = await _ctx.AppUsers.Where(x => x.Username == username).FirstOrDefaultAsync();

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

        internal async Task<string> GetRoleByUserNameAsync(string username)
        {
            //var dbAppUserRolename = await _ctx.AppUsers
            //    .Where(x => x.Username == username)
            //    .Select(o => o.AppRole.RoleName)
            //    .FirstOrDefaultAsync();
            //return dbAppUserRolename;

            //var dbAppUser = _ctx.AppUsers.Include(o => o.AppRole).Where(x => x.Username == username).FirstOrDefault();
            //var dbAppUser = await _ctx.AppUsers.Include(o => o.AppRole).Where(x => x.Username == username).FirstOrDefault();
            //return dbAppUser.AppRole.RoleName;
            return (await _ctx.AppUsers.Include(o => o.AppRole).Where(x => x.Username == username).FirstOrDefaultAsync()).AppRole.RoleName;
        }

        internal async Task<List<AppUser>> GetAllUsers()
        {
            return await _ctx.AppUsers.Include(o => o.AppRole).ToListAsync();
        }
    }
}
