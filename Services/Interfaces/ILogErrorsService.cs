using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface ILogErrorsService
    {
        Task<List<ErrorLog>> ErrorList();
        Task<bool> Delete(int id);
    }
}
