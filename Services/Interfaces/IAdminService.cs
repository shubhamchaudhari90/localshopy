namespace localshopyNew.Services.Interfaces
{
    public interface IAdminService
    {
        string AdminLoggedIn(string username, string password);
        string AdminLoggedInFromGoogle(string userEmailId);
    }
}
