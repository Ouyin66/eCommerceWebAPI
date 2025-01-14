using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ReceiptController : ControllerBase
    {
        ECOMMERCE dbc;

        public ReceiptController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Receipt/GetList")]
        public IActionResult GetList()
        {
            List<Receipt> receipts = dbc.Receipts.OrderByDescending(r => r.DateCreate).ToList();

            foreach (var receipt in receipts)
            {
                List<ReceiptVariant> lstVariant = dbc.ReceiptVariants.Where(v => v.ReceiptId == receipt.Id).ToList();
                receipt.ReceiptVariants = lstVariant;

                List<OrderStatusHistory> lstStatus = dbc.OrderStatusHistories.Where(s => s.ReceiptId == receipt.Id).ToList();

                // Nếu tồn tại trạng thái hủy đơn hàng thì ưu tiên lấy trạng thái này
                if (lstStatus.Any(s => s.State == 0))
                {
                    lstStatus = lstStatus.Where(s => s.State == 0).ToList();
                }
                else
                {
                    var maxState = lstStatus.Max(s => s.State);
                    lstStatus = lstStatus.Where(s => s.State == maxState).ToList();
                }
                receipt.OrderStatusHistories = lstStatus;
            }

            return Ok(new { receipts });
        }

        [HttpGet]
        [Route("/Receipt/ListByUserId")]
        public IActionResult GetReceiptnByUserId(int userId)
        {
            List<Receipt> receipts = dbc.Receipts
                                .Where(r => r.UserId == userId)
                                .OrderByDescending(r => r.DateCreate) // Sắp xếp giảm dần theo ngày tạo
                                .ToList();

            foreach (var receipt in receipts)
            {
                List<ReceiptVariant> lstVariant = dbc.ReceiptVariants.Where(v => v.ReceiptId == receipt.Id).ToList();
                receipt.ReceiptVariants = lstVariant;

                List<OrderStatusHistory> lstStatus = dbc.OrderStatusHistories.Where(s => s.ReceiptId == receipt.Id).ToList();
                
                // Nếu tồn tại trạng thái hủy đơn hàng thì ưu tiên lấy trạng thái này
                if (lstStatus.Any(s => s.State == 0))
                {
                    lstStatus = lstStatus.Where(s => s.State == 0).ToList();
                } else
                {
                    var maxState = lstStatus.Max(s => s.State);
                    lstStatus = lstStatus.Where(s => s.State == maxState).ToList();
                }       
                receipt.OrderStatusHistories = lstStatus;
            }

            return Ok(new { receipts });
        }

        [HttpGet]
        [Route("/Receipt/Get")]
        public IActionResult GetReceipt(int receiptId)
        {
            Receipt receipt = dbc.Receipts.FirstOrDefault(r => r.Id == receiptId);

            if (receipt == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn này" });
            }

            List<ReceiptVariant> lstVariant = dbc.ReceiptVariants.Where(v => v.ReceiptId == receipt.Id).ToList();
            receipt.ReceiptVariants = lstVariant;

            List<OrderStatusHistory> lstStatus = dbc.OrderStatusHistories.Where(s => s.ReceiptId == receipt.Id).ToList();

            // Nếu tồn tại trạng thái hủy đơn hàng thì ưu tiên lấy trạng thái này
            if (lstStatus.Any(s => s.State == 0))
            {
                lstStatus = lstStatus.Where(s => s.State == 0).ToList();
            }
            else
            {
                var maxState = lstStatus.Max(s => s.State);
                lstStatus = lstStatus.Where(s => s.State == maxState).ToList();
            }
            receipt.OrderStatusHistories = lstStatus;

            return Ok(new { receipt });
        }

        [HttpPost]
        [Route("/Receipt/Insert")]
        public async Task<IActionResult> CreateReceiptWithVariants([FromBody] Receipt receipt)
        {
            if (receipt == null) return BadRequest("Dữ liệu không hợp lệ");
            if (receipt.ReceiptVariants == null || !receipt.ReceiptVariants.Any())
            {
                return BadRequest("Hóa đơn phải có ít nhất một sản phẩm.");
            }

            if (!receipt.DateCreate.HasValue)
            {
                receipt.DateCreate = DateTime.Now;
            }

            receipt.OrderStatusHistories = null;

            dbc.Receipts.Add(receipt);

            foreach (var variant in receipt.ReceiptVariants)
            {
                //variant.Receipt = receipt;
                variant.ReceiptId = receipt.Id;
                var updateVariant = dbc.Variants.FirstOrDefault(v => v.Id == variant.VariantId);
                if (updateVariant == null)
                {
                    return NotFound($"Variant with ID {variant.VariantId} not found.");
                }

                if (updateVariant.Quantity < variant.Quantity)
                {
                    return BadRequest($"Not enough stock for variant {variant.VariantId}. Available: {updateVariant.Quantity}, Requested: {variant.Quantity}");
                }

                updateVariant.Quantity -= variant.Quantity;
                var product = dbc.Products.FirstOrDefault(p => p.Id == updateVariant.ProductId);
                product.Amount -= variant.Quantity;

                // Xóa mục trong bảng Cart dựa trên userId và variantId
                var cartItem = dbc.Carts.FirstOrDefault(c => c.UserId == receipt.UserId && c.VariantId == variant.VariantId);
                if (cartItem != null)
                {
                    dbc.Carts.Remove(cartItem);
                }
            }

            // Lưu thay đổi sau khi thêm receipt và cập nhật variants
            await dbc.SaveChangesAsync();

            // Tạo mục lịch sử trạng thái đơn hàng
            int? receiptId = receipt.Id;
            var orderStatusHistory = new OrderStatusHistory
            {
                ReceiptId = receiptId,
                State = 1,
                Notes = "Đang xử lý",
                TimeStamp = DateTime.Now
            };

            dbc.OrderStatusHistories.Add(orderStatusHistory);

            // Tạo thông báo
            Notification notification = new Notification
            {
                UserId = receipt.UserId,
                Message = $"Bạn vừa tạo thành công một đơn hàng HD{receipt.Id:D10}",
                Type = "Receipt",
                ReferenceId = receiptId,
                DateCreated = DateTime.Now,
                IsRead = false,
            };
            dbc.Notifications.Add(notification);

            await dbc.SaveChangesAsync();

            return Ok(new { receipt });
        }

        [HttpPut]
        [Route("/Receipt/IsInterest")]
        public IActionResult UpdateStatus(int receiptId, bool isInterest)
        {
            Receipt existingReceipt = dbc.Receipts.FirstOrDefault(r => r.Id == receiptId);

            if (existingReceipt == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng này" });
            }

            existingReceipt.Interest = isInterest;

            dbc.Receipts.Update(existingReceipt);
            dbc.SaveChanges();
            return Ok(new { existingReceipt, message = "Cập nhật thành công" });
        }
    }
}
