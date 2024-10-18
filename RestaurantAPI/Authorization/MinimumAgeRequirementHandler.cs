using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RestaurantAPI.Authorization
{
    public class MinimumAgeRequirementHandler(ILogger<MinimumAgeRequirementHandler> logger) : AuthorizationHandler<MinimumAgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
            var dateOfBirthClaim = context.User.FindFirst(c => c.Type == "DateOfBirth");
            if (dateOfBirthClaim == null)
            {
                logger.LogInformation("Date of birth claim not found");
                return Task.CompletedTask;
            }

            var dateOfBirth = DateTime.Parse(dateOfBirthClaim.Value);

            var userEmail = context.User.FindFirst(c => c.Type == ClaimTypes.Name)?.Value ?? "Unknown user";

            logger.LogInformation("User: {UserEmail} with date of birth: [{DateOfBirth}]", userEmail, dateOfBirth);

            if (dateOfBirth.AddYears(requirement.MinimumAge) <= DateTime.Today)
            {
                logger.LogInformation("Authorization succeeded");
                context.Succeed(requirement);
            }
            else
            {
                logger.LogInformation("Authorization failed");
            }

            return Task.CompletedTask;
        }
    }
}
