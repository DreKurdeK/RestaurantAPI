using AutoMapper;
using RestaurantAPI.Entities;
using RestaurantAPI.Entities.Exceptions;
using RestaurantAPI.Models;

namespace RestaurantAPI.Services
{
    public interface IDishService
    {
        int Create(int restaurantId, CreateDishDto dto);
    }

    public class DishService(RestaurantDbContext context, IMapper mapper) : IDishService
    {
        public int Create(int restaurantId, CreateDishDto dto)
        {
            var restaurant = context.Restaurants.FirstOrDefault(r => r.Id == restaurantId);
            if (restaurant == null)
                throw new NotFoundException("Restaurant not found");

            var dishEntity = mapper.Map<Dish>(dto);
            context.Dishes.Add(dishEntity);
            context.SaveChanges();
            return dishEntity.Id;
        }
    }
}
