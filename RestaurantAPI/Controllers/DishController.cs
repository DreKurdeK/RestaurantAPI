using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.Models;
using RestaurantAPI.Services;

namespace RestaurantAPI.Controllers
{
    [Route("api/{restaurantId}/dish")]
    [ApiController]
    public class DishController(IDishService dishSerivce) : ControllerBase
    {
        [HttpPost]
        public ActionResult Post([FromRoute]int restaurantId, CreateDishDto dto)
        {
            var newDishId = dishSerivce.Create(restaurantId, dto);
            return Created("api/{restaurantId}/dish/{newDishId}", null);
        }
    }
}
