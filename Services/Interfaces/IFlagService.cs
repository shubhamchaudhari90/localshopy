using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface IFlagService
    {
        Task<List<Flag>> GetAllFlags();
        Task<Flag?> GetFlagByKey(string key);
        Task<bool> SaveFlag(Flag flag);
        Task<bool> DeleteFlag(string key);
    }
}
