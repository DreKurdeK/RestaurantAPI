using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;

namespace RestaurantAPI.Services
{
    public interface IRestaurantService
    {
        RestaurantDto? GetById(int id);
        List<RestaurantDto> GetAll();
        int Create(CreateRestaurantDto dto);
        void Delete(int id);
        void Update(int id, UpdateRestaurantDto dto);
    }

    public class RestaurantService(RestaurantDbContext dbContext, IMapper mapper, ILogger<RestaurantService> logger) : IRestaurantService
    {
        public void Update(int id, UpdateRestaurantDto dto)
        {
            var restaurant = dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == id);
            if (restaurant is null)
                throw new DirectoryNotFoundException("Restaurant not found");

            restaurant.Name = dto.Name;
            restaurant.Description = dto.Description;
            restaurant.HasDelivery = (bool)dto.HasDelivery;

            dbContext.SaveChanges();
        }
        public void Delete(int id)
        {
            logger.LogError($"Restaurant with id: {id} DELETE action invoked");
            var restaurant = dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == id);
            if (restaurant is null) 
                throw new DirectoryNotFoundException("Restaurant not found");

            dbContext.Restaurants.Remove(restaurant);
            dbContext.SaveChanges();
        }
        public RestaurantDto? GetById(int id)
        {
            var restaurant = dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .FirstOrDefault(r => r.Id == id);

            if (restaurant == null) 
                throw new DirectoryNotFoundException("Restaurant not found");

            var result = mapper.Map<RestaurantDto>(restaurant);
            return result;
        }

        public List<RestaurantDto> GetAll()
        {
            var restaurants = dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .ToList();

            var restaurantsDtos = mapper.Map<List<RestaurantDto>>(restaurants);
            return restaurantsDtos;
        }

        public int Create(CreateRestaurantDto dto)
        {
            var restaurant = mapper.Map<Restaurant>(dto);
            dbContext.Restaurants.Add(restaurant);
            dbContext.SaveChanges();

            return restaurant.Id;
        }
    }
}
