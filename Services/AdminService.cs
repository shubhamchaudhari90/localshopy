using localshopyNew.Services.Interfaces;

namespace localshopyNew.Services
{
    public class AdminService : IAdminService
    {
        private readonly IConfiguration _configuration;

        public AdminService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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
    }
}
