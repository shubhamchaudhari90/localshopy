using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class AccountService(IConfiguration configuration, AppDBContext context) : IAccountService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly AppDBContext _context = context;

        public string AdminLoggedIn(string username, string password)
        {
            string emailIds = _configuration["MyData:Email"] ?? "ShubhamLuclXyhrgFD0PhJX17ejQoDsRhcTWCNilima";
            string passwords = _configuration["MyData:Password"] ?? "Tanishkaha4EkloPy0VJQq4DpNlyFX3f8koL7USRC";

            string[] arrEmail = emailIds.Split(",");
            string[] arrPassword = passwords.Split(",");

            if (arrEmail.Contains(username) && arrPassword.Contains(password))
            {
                return username + ":" + password;
            }
            else
            {
                return string.Empty;
            }
        }

        public string AdminLoggedInFromGoogle(string userEmailId)
        {
            string emailIds = _configuration["MyData:Email"] ?? "ShubhamLuclXyhrgFD0PhJX17ejQoDsRhcTWCNilima";
            string passwords = _configuration["MyData:Password"] ?? "Tanishkaha4EkloPy0VJQq4DpNlyFX3f8koL7USRC";

            string[] arrEmail = emailIds.Split(",");
            string[] arrPassword = passwords.Split(",");

            foreach (var email in arrEmail)
            {
                if (userEmailId.Trim().Equals(email.Trim()))
                {
                    return userEmailId + ":" + arrPassword[0];
                }
            }
            return string.Empty;
        }

        public async Task AddLoggedInUser(string emailId, string loggedInType, string role)
        {
            LoggedInUsers loggedInUser = new()
            {
                EmailId = emailId,
                Id = Guid.NewGuid(),
                Role = role,
                LoggedInTime = DateTime.UtcNow,
                LoggedInType = loggedInType
            };
            await _context.LoggedInUsers.AddAsync(loggedInUser);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LoggedInUsers>> GetLoggedInUsersAsync()
        {
            return await _context.LoggedInUsers.OrderByDescending(x => x.LoggedInTime).ToListAsync();
        }

        public async Task<bool> DeleteLoggedInUserAsync(Guid id)
        {
            var user = await _context.LoggedInUsers.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return false;

            _context.LoggedInUsers.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
