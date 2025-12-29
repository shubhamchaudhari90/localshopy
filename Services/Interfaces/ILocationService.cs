using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface ILocationService
    {
        Task<bool> IsLocationNameExists(string name);
        Task<Location?> GetLocationById(Guid id);
        Task<List<Location>> GetActiveLocations();
        Task<List<Location>> GetInActiveLocations();
        Task<bool> AddLocation(Location Location);
        Task<bool> UpdateLocation(Location Location);
        Task<bool> DeleteLocation(Guid id);
    }
}
