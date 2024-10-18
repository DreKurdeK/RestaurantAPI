using Microsoft.AspNetCore.Authorization;
using RestaurantAPI.Entities;
using System.Security.Claims;

namespace RestaurantAPI.Authorization
{
    public class CreatedMultipleRestaurantsRequirement(int minimumRestaurantCreated) : IAuthorizationRequirement
    {
        public int MinimumRestaurantCreated { get; } = minimumRestaurantCreated;
    }

    public class CreatedMultipleRestaurantsRequirementHandler(RestaurantDbContext dbContext) : AuthorizationHandler<CreatedMultipleRestaurantsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CreatedMultipleRestaurantsRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Task.CompletedTask;
            }

            var userId = int.Parse(userIdClaim.Value);
            var restaurantCount = dbContext.Restaurants.Count(r => r.CreatedById == userId);

            if (restaurantCount >= requirement.MinimumRestaurantCreated)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
