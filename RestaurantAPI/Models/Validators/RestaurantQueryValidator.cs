using FluentValidation;
using RestaurantAPI.Entities;

namespace RestaurantAPI.Models.Validators
{
    public class RestaurantQueryValidator : AbstractValidator<RestaurantQuery>
    {
        private readonly int[] allowedPageSizes = [5, 10, 15, 30, 50];
        private readonly string[] allowedSortByColumnNames = [nameof(Restaurant.Name), nameof(Restaurant.Description), nameof(Restaurant.Category)];

        public RestaurantQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).Custom((value, context) =>
            {
                if (!allowedPageSizes.Contains(value))
                {
                    context.AddFailure("PageSize", $"PageSize must be one of the following values: {string.Join(",", allowedPageSizes)}");
                }
            });

            RuleFor(x => x.SortBy)
                .Must(v => string.IsNullOrEmpty(v) || allowedSortByColumnNames.Contains(v))
                .WithMessage($"Sort by is optional, or must be one of the following values: {string.Join(",", allowedSortByColumnNames)}");
        }
    }
}
