using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign
{
    public interface ICreatorApplicationRepos
    {
        Task<object> ApplyForCreator(CreatorApplication creatorApplication);
    }
}
