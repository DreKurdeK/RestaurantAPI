using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using System.Linq.Expressions;

namespace RestaurantAPI.Services
{
    public interface IRestaurantService
    {
        RestaurantDto? GetById(int id);
        PagedResult<RestaurantDto> GetAll(RestaurantQuery query);
        int Create(CreateRestaurantDto dto);
        void Delete(int id);
        void Update(int id, UpdateRestaurantDto dto);
    }

    public class RestaurantService(
        RestaurantDbContext dbContext, //Database
        IMapper mapper, //Mapping models
        ILogger<RestaurantService> logger, //Logging
        IAuthorizationService authorizationService, //User authorization
        IUserContextService userContextService) //Context of ac
        : IRestaurantService
    {
        public void Update(int id, UpdateRestaurantDto dto)
        {
            var restaurant = dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == id)
                ?? throw new DirectoryNotFoundException("Restaurant not found");

            var authResult = authorizationService.AuthorizeAsync(userContextService.User, restaurant,
                new ResourceOperationRequirement(ResourceOperation.Update)).Result;

            if (!authResult.Succeeded)
            {
                throw new ForbidException();
            }

            restaurant.Name = dto.Name;
            restaurant.HasDelivery = dto.HasDelivery ?? false;
            restaurant.Description = dto.Description;
            restaurant.HasDelivery = (bool)dto.HasDelivery;

            dbContext.SaveChanges();
        }
        public void Delete(int id)
        {
            logger.LogError("Restaurant with id: {RestaurantId} DELETE action invoked", id);
            var restaurant = dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == id)
                ?? throw new DirectoryNotFoundException("Restaurant not found");

            var authResult = authorizationService.AuthorizeAsync(userContextService.User, restaurant,
                new ResourceOperationRequirement(ResourceOperation.Update)).Result;

            if (!authResult.Succeeded)
            {
                throw new ForbidException();
            }

            dbContext.Restaurants.Remove(restaurant);
            dbContext.SaveChanges();
        }
        public RestaurantDto? GetById(int id)
        {
            var restaurant = dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .FirstOrDefault(r => r.Id == id)
                ?? throw new DirectoryNotFoundException("Restaurant not found");

            var result = mapper.Map<RestaurantDto>(restaurant);
            return result;
        }

        public PagedResult<RestaurantDto> GetAll(RestaurantQuery query)
        {
            var baseQuery = dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .Where(r => query.SearchPhrase == null ||
                (r.Name != null && r.Name.Contains(query.SearchPhrase, StringComparison.CurrentCultureIgnoreCase) ||
                 r.Description != null && r.Description.Contains(query.SearchPhrase, StringComparison.CurrentCultureIgnoreCase)));

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                var columnsSelector = new Dictionary<string, Expression<Func<Restaurant, object>>>
        {
            { nameof(Restaurant.Name), r => r.Name },
            { nameof(Restaurant.Description), r => r.Description },
            { nameof(Restaurant.Category), r => r.Category }
        };

                if (columnsSelector.TryGetValue(query.SortBy, out Expression<Func<Restaurant, object>>? value))
                {
                    baseQuery = query.SortDirection == SortDirection.ASC
                        ? baseQuery.OrderBy(value)
                        : baseQuery.OrderByDescending(value);
                }
            }

            var restaurants = baseQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .ToList();

            var totalItemsCount = baseQuery.Count();
            var restaurantsDtos = mapper.Map<List<RestaurantDto>>(restaurants);

            var result = new PagedResult<RestaurantDto>(restaurantsDtos, totalItemsCount, query.PageSize, query.PageNumber);

            return result;
        }


        public int Create(CreateRestaurantDto dto)
        {
            var restaurant = mapper.Map<Restaurant>(dto);
            restaurant.CreatedById = userContextService.GetUserId;
            dbContext.Restaurants.Add(restaurant);
            dbContext.SaveChanges();

            return restaurant.Id;
        }
    }
}
