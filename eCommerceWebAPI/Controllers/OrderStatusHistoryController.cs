using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceWebAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class OrderStatusHistoryController : ControllerBase
    {
        ECOMMERCE dbc;

        public OrderStatusHistoryController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/OrderStatusHistory/ListByReceiptId")]
        public IActionResult GetStatusOrderById(int receiptId)
        {
            List<OrderStatusHistory> orderStatusHistories = dbc.OrderStatusHistories.Where(r => r.ReceiptId == receiptId).ToList();

            return Ok(new { dataa = orderStatusHistories });
        }

        [HttpPost]
        [Route("/OrderStatusHistory/InsertStatus")]
        public IActionResult InsertStatus(int receiptId, int state, string notes)
        {

            OrderStatusHistory existingOrderStatus = dbc.OrderStatusHistories.FirstOrDefault(s => s.ReceiptId == receiptId && s.State == state && s.Notes == notes);

            if (existingOrderStatus != null)
            {
                return BadRequest(new { message = "Đã tồn tại trạng thái này" });
            }

            OrderStatusHistory orderStatusHistory = new OrderStatusHistory();
            orderStatusHistory.ReceiptId = receiptId;
            orderStatusHistory.State = state;
            orderStatusHistory.Notes = notes;
            orderStatusHistory.TimeStamp = DateTime.Now;

            dbc.OrderStatusHistories.Add(orderStatusHistory);
            dbc.SaveChanges();
            return Ok(new { orderStatusHistory });
        }

        [HttpPut]
        [Route("/OrderStatusHistory/UpdateStatus")]
        public IActionResult UpdateStatus(int id, int receiptId, int state, string notes)
        {
            OrderStatusHistory existingOrderStatus = dbc.OrderStatusHistories.FirstOrDefault(s => s.ReceiptId == receiptId && s.State == state && s.Notes == notes);

            if (existingOrderStatus != null)
            {
                return BadRequest(new { message = "Đã tồn tại trạng thái này" });
            }

            OrderStatusHistory orderStatusHistory = dbc.OrderStatusHistories.FirstOrDefault(s => s.Id == id);
            orderStatusHistory.ReceiptId = receiptId;
            orderStatusHistory.State = state;
            orderStatusHistory.Notes = notes;
            orderStatusHistory.TimeStamp = DateTime.Now;

            dbc.OrderStatusHistories.Update(orderStatusHistory);
            dbc.SaveChanges();
            return Ok(new { orderStatusHistory });          
        }
    }
}
