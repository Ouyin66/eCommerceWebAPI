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
        [Route("/Nofication/ListByUserId")]
        public IActionResult GetNoficationnByUserId(int userId)
        {
            List<Notification> notifications = dbc.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.DateCreated).ToList();

            return Ok(new { dataa = notifications });
        }

        [HttpPut]
        [Route("/Nofication/IsRead")]
        public IActionResult UpdateIsRead(int notificationId)
        {
            Notification existingNotification = dbc.Notifications.FirstOrDefault(n => n.Id == notificationId);

            if (existingNotification == null)
            {
                return NotFound(new { message = "Không tìm thấy thông báo này" });
            }

            existingNotification.IsRead = true;
            dbc.Notifications.Update(existingNotification);
            dbc.SaveChanges();
            return Ok(new { existingNotification, message = "Cập nhật thành công" });
        }
    }
}
