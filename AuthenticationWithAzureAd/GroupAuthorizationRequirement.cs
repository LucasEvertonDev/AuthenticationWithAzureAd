using Microsoft.AspNetCore.Authorization;

namespace AuthenticationWithAzureAd
{
    public class GroupAuthorizationRequirement : IAuthorizationRequirement
    {
        public GroupAuthorizationRequirement(string groupId)
        {
            GroupId = groupId;
        }

        public string GroupId { get; private set; }
    }
}
