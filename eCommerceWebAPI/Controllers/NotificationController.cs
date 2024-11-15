using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        ECOMMERCE dbc;

        public NotificationController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Nofications/ListByUserId")]
        public IActionResult GetNoficationnByUserId(int userId)
        {
            List<Notification> notifications = dbc.Notifications.Where(r => r.UserId == userId).ToList();

            return Ok(new { dataa = notifications });
        }
    }
}
