using localshopyNew.Models;

namespace localshopyNew.Services.Interfaces
{
    public interface IBlockedUserService
    {
        Task<List<BlockedUser>> GetBlockedUsers();
        Task<bool> AddBlockedUser(BlockedUser blockedUser);
        Task<bool> RemoveBlockedUser(string emailId);
    }
}
