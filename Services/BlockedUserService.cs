using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class BlockedUserService : IBlockedUserService
    {
        private readonly AppDBContext _context;

        public BlockedUserService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<BlockedUser>> GetBlockedUsers()
        {
            List<BlockedUser> blockedUsers = await _context.BlockedUsers.ToListAsync();
            return blockedUsers;
        }

        public async Task<bool> AddBlockedUser(BlockedUser blockedUser)
        {
            if (blockedUser == null)
                return false;

            await _context.BlockedUsers.AddAsync(blockedUser);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveBlockedUser(string emailId)
        {
            if (string.IsNullOrEmpty(emailId))
                return false;

            BlockedUser? blockedUser = await _context.BlockedUsers.FirstOrDefaultAsync(x => x.EmailId == emailId);

            if (blockedUser == null) return false;

            _context.BlockedUsers.Remove(blockedUser);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
