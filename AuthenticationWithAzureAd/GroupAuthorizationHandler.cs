using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;

namespace AuthenticationWithAzureAd;

public class GroupAuthorizationHandler : AuthorizationHandler<GroupAuthorizationRequirement>
{
    private readonly GraphServiceClient _graphServiceClient;

    public GroupAuthorizationHandler(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupAuthorizationRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            if (context.User.Claims.Any(x => x.Type == "groups" && x.Value == requirement.GroupId))
            {
                context.Succeed(requirement);
            }

            if (context.User.Claims.Any(x => x.Type == "hasgroups" && x.Value == "true"))
            {
                var groupResult = _graphServiceClient.Me.CheckMemberGroups(new List<string> { requirement.GroupId }).Request().PostAsync().Result;

                if (groupResult.Any(x => x == requirement.GroupId))
                {
                    context.Succeed(requirement);
                }
            }
        }
        
        return Task.CompletedTask;
    }
}
