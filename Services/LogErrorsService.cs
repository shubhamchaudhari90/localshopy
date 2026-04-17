using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class LogErrorsService : ILogErrorsService
    {
        private readonly AppDBContext _context;

        public LogErrorsService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<ErrorLog>> ErrorList()
        {
            var data = await _context.ErrorLogs.ToListAsync();
            return data;
        }

        public async Task<bool> Delete(int id)
        {
            var log = await _context.ErrorLogs.FindAsync(id);

            if (log == null)
                return false;

            _context.ErrorLogs.Remove(log);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}