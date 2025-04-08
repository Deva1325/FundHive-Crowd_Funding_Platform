using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface ISidebarRepos
    {
        Task<List<SidebarModel>> GetTabsByRoleIdAsync(int roleId, string isAdmin);

    }
}
