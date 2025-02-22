using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class SidebarClassRepos : ISidebarRepos
    {
        private readonly DbMain_CFS _CFS;

        public SidebarClassRepos(DbMain_CFS CFS)
        {
            _CFS = CFS;
        }


        public async Task<List<SidebarModel>> GetTabsByRoleIdAsync(int roleId)
        {
            var tabs = await (from t in _CFS.TblTabs
                              join p in _CFS.Permissions on t.TabId equals p.Tabid
                              where p.Isadmin == true || p.Iscreatorapproved == true // Fetch only for admin or approved creator
                              orderby t.SortOrder
                              select new SidebarModel
                              {
                                  TabId = t.TabId,
                                  TabName = t.TabName,
                                  ParentId = t.ParentId,
                                  TabUrl = t.TabUrl,
                                  IconPath = t.IconPath
                              }).ToListAsync();

            // Group the tabs into a hierarchical structure (Parent → Child tabs)
            var tabHierarchy = tabs
                .Where(tab => tab.ParentId == null)
                .Select(tab => new SidebarModel
                {
                    TabId = tab.TabId,
                    TabName = tab.TabName,
                    ParentId = tab.ParentId,
                    TabUrl = tab.TabUrl,
                    IconPath = tab.IconPath,
                    SubTabs = tabs.Where(sub => sub.ParentId == tab.TabId).ToList()
                }).ToList();

            return tabHierarchy;


        }
    }
}
