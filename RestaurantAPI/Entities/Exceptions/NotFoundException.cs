namespace RestaurantAPI.Entities.Exceptions
{
    public class NotFoundException(string message) : Exception(message)
    {
    }
}
