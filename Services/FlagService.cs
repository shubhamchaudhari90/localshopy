using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class FlagService(AppDBContext context) : IFlagService
    {
        private readonly AppDBContext _context = context;

        public async Task<bool> SaveFlag(Flag flag)
        {
            var existing = await GetFlagByKey(flag.Key);
            if (existing == null)
            {
                flag.Id = Guid.NewGuid();
                await _context.Flags.AddAsync(flag);
            }
            else
            {
                existing.Value = flag.Value;
                _context.Flags.Update(existing);
            }
            int recordsChanged = await _context.SaveChangesAsync();
            return recordsChanged > 0;
        }

        public async Task<bool> DeleteFlag(string key)
        {
            var existing = await GetFlagByKey(key);

            if (existing == null)
                return false;

            _context.Flags.Remove(existing);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Flag>> GetAllFlags()
        {
            return await _context.Flags.OrderBy(x => x.Key).ToListAsync();
        }

        public async Task<Flag?> GetFlagByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            return await _context.Flags.FirstOrDefaultAsync(x => x.Key.ToLower() == key.ToLower());
        }
    }
}
