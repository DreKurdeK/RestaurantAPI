using RestaurantAPI.Entities;

namespace RestaurantAPI
{
    public class RestaurantSeeder
    {
        private readonly RestaurantDbContext _dbContext;

        // Constructor
        public RestaurantSeeder(RestaurantDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Seed method to populate the database
        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {
                if (!_dbContext.Restaurants.Any())
                {
                    var restaurants = GetRestaurants();
                    _dbContext.Restaurants.AddRange(restaurants);
                    _dbContext.SaveChanges();
                }
            }
        }

        // Method to create a list of restaurant data
        private IEnumerable<Restaurant> GetRestaurants()
        {
            var restaurants = new List<Restaurant>()
            {
                new()
                {
                    Name = "KFC",
                    Category = "Fast Food",
                    Description =
                    "KFC (short for Kentucky Fried Chicken) is an American fast food restaurant chain headquartered in the US.",
                    ContactEmail = "contact@kfc.com",
                    ContactNumber = "987-654-321",
                    HasDelivery = true,
                    Dishes = new List<Dish>()
                    {
                        new Dish()
                        {
                            Name = "Nashville Hot Chicken",
                            Price = 10.30M,
                        },
                        new Dish()
                        {
                            Name = "Chicken Nuggets",
                            Price = 5.30M,
                        },
                    },
                    Address = new Address()
                    {
                        City = "Kraków",
                        Street = "Długa 5",
                        PostalCode = "30-001"
                    }
                },
                new()
                {
                    Name = "McDonalds",
                    Category = "Fast Food",
                    Description =
                    "McDonalds is the world's biggest fast food chain with its presence in over 100 countries.",
                    ContactEmail = "contact@mcdonalds.com",
                    ContactNumber = "987-654-321",
                    HasDelivery = true,
                    Dishes = new List<Dish>()
                    {
                        new Dish()
                        {
                            Name = "Big Mac",
                            Price = 12.00M,
                        },
                        new Dish()
                        {
                            Name = "French Fries",
                            Price = 3.00M,
                        },
                    },
                    Address = new Address()
                    {
                        City = "Kraków",
                        Street = "Floriańska 10",
                        PostalCode = "31-001"
                    }
                },
            };

            return restaurants;
        }
    }
}
