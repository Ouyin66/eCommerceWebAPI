using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace eCommerceWebAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class OrderStatusHistoryController : ControllerBase
    {
        ECOMMERCE dbc;
        private readonly IHubContext<NotificationHub> _hubContext;

        public OrderStatusHistoryController(ECOMMERCE db, IHubContext<NotificationHub> hubContext)
        {
            dbc = db;
            _hubContext = hubContext;
        }


        [HttpGet]
        [Route("/OrderStatusHistory/ListByReceiptId")]
        public IActionResult GetStatusOrderById(int receiptId)
        {
            List<OrderStatusHistory> orderStatusHistories = dbc.OrderStatusHistories.Where(s => s.ReceiptId == receiptId).OrderByDescending(s => s.TimeStamp).ToList();

            return Ok(new { dataa = orderStatusHistories });
        }

        [HttpPost]
        [Route("Cancel")]
        public async Task<IActionResult> CancelReceiptAsync(int receiptId, string notes)
        {
            using (var transaction = dbc.Database.BeginTransaction())
            {
                try
                {
                    // Kiểm tra xem đơn hàng đã bị hủy trước đó chưa
                    var cancelStatus = dbc.OrderStatusHistories.FirstOrDefault(s => s.ReceiptId == receiptId && s.State == 0);

                    if (cancelStatus != null)
                    {
                        return BadRequest(new { message = "Đơn hàng này đã bị hủy trước đó." });
                    }

                    // Thêm trạng thái hủy vào lịch sử
                    var orderStatusHistory = new OrderStatusHistory
                    {
                        ReceiptId = receiptId,
                        State = 0,
                        Notes = notes,
                        TimeStamp = DateTime.Now
                    };

                    dbc.OrderStatusHistories.Add(orderStatusHistory);
                    await dbc.SaveChangesAsync();

                    // Kiểm tra và cập nhật thông tin đơn hàng và kho hàng
                    var existingReceipt = dbc.Receipts
                        .Include(r => r.ReceiptVariants) // Tải trước các đối tượng liên quan
                        .FirstOrDefault(r => r.Id == receiptId);

                    if (existingReceipt == null)
                    {
                        return NotFound(new { message = "Không tìm thấy đơn hàng." });
                    }

                    foreach (var item in existingReceipt.ReceiptVariants)
                    {
                        var updatedVariant = dbc.Variants.FirstOrDefault(v => v.Id == item.VariantId);
                        if (updatedVariant == null) continue;

                        updatedVariant.Quantity += item.Quantity;

                        var product = dbc.Products.FirstOrDefault(p => p.Id == updatedVariant.ProductId);
                        if (product == null) continue;

                        product.Amount += item.Quantity;
                    }

                    var checkReceipt = dbc.Receipts.FirstOrDefault(r => r.Id == receiptId);
                    if (checkReceipt != null && checkReceipt.Interest == true)
                    {
                        Notification notification = new Notification
                        {
                            UserId = checkReceipt.UserId,
                            Message = $"Đã hủy đơn hàng HD{checkReceipt.Id:D10}",
                            Type = "Receipt",
                            ReferenceId = receiptId,
                            DateCreated = DateTime.Now,
                            IsRead = false,
                        };
                        dbc.Notifications.Add(notification);
                    }

                    await dbc.SaveChangesAsync();
                    transaction.Commit();

                    await _hubContext.Clients.Group(receiptId.ToString())
                        .SendAsync("StatusUpdated", new
                        {
                            receiptId,
                            state = orderStatusHistory.State,
                            notes,
                            timeStamp = orderStatusHistory.TimeStamp
                        });

                    return Ok(new { message = "Đơn hàng đã được hủy thành công.", orderStatusHistory });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, new { message = "Đã xảy ra lỗi.", details = ex.Message });
                }
            }
        }


        [HttpPost]
        [Route("/OrderStatusHistory/InsertStatus")]
        public async Task<IActionResult> InsertStatus(int receiptId, int state, string notes)
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
            
            var checkReceipt = dbc.Receipts.FirstOrDefault(r => r.Id == receiptId);
            if (checkReceipt != null && checkReceipt.Interest == true) {
                Notification notification = new Notification
                {
                    UserId = checkReceipt.UserId,
                    Message = $"Đã cập nhật trạng thái mới cho đơn hàng HD{checkReceipt.Id:D10}",
                    Type = "Receipt",
                    ReferenceId = receiptId,
                    DateCreated = DateTime.Now,
                    IsRead = false,
                };

                dbc.Notifications.Add(notification);
                dbc.SaveChanges();
            }

            // Phát tín hiệu tới client liên quan
            await _hubContext.Clients.Group(receiptId.ToString())
                .SendAsync("StatusUpdated", new
                {
                    receiptId,
                    state,
                    notes,
                    timeStamp = orderStatusHistory.TimeStamp
                });

            return Ok(new { orderStatusHistory });
        }

        [HttpPut]
        [Route("/OrderStatusHistory/UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id, int receiptId, int state, string notes)
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

            await _hubContext.Clients.Group(receiptId.ToString())
                .SendAsync("StatusUpdated", new
                {
                    receiptId,
                    state,
                    notes,
                    timeStamp = orderStatusHistory.TimeStamp
                });

            return Ok(new { orderStatusHistory });          
        }

        [HttpDelete]
        [Route("/OrderStatusHistory/Delete")]
        public async Task<IActionResult> DeleteSize(int id)
        {
            OrderStatusHistory orderStatus = dbc.OrderStatusHistories.FirstOrDefault(s => s.Id == id);

            if (orderStatus == null)
            {
                return NotFound(new { message = "Không tìm thấy trạng thái với id này" });
            }

            dbc.OrderStatusHistories.Remove(orderStatus);
            dbc.SaveChanges();

            await _hubContext.Clients.Group(orderStatus.ReceiptId.ToString())
                .SendAsync("StatusUpdated", new
                {
                    id = orderStatus.Id,
                    receiptId = orderStatus.ReceiptId,
                    state = orderStatus.State,
                    notes = orderStatus.Notes,
                    timeStamp = orderStatus.TimeStamp
                });

            return Ok(new { message = "Xóa thành công", orderStatus });
        }
    }
}
