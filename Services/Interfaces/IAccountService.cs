using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface IAccountService
    {
        string AdminLoggedIn(string username, string password);
        string AdminLoggedInFromGoogle(string userEmailId);
        Task AddLoggedInUser(string emailId, string loggedInType, string role);
        Task<List<LoggedInUser>> GetLoggedInUsersAsync();
        Task<bool> DeleteLoggedInUserAsync(Guid id);
    }
}
