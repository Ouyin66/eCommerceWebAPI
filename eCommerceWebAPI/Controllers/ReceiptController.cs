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
        [Route("/Receipts/ListByUserId")]
        public IActionResult GetReceiptnByUserId(int userId)
        {
            List<Receipt> receipts = dbc.Receipts
                                .Where(r => r.UserId == userId)
                                .OrderByDescending(r => r.DateCreate) // Sắp xếp giảm dần theo ngày tạo
                                .ToList();

            return Ok(new { receipts });
        }

        [HttpPost]
        [Route("/Receipts/Insert")]
        public async Task<IActionResult> CreateReceiptWithVariants([FromBody] Receipt receipt)
        {
            if (receipt == null) return BadRequest("Invalid data.");
            if (receipt.ReceiptVariants == null || !receipt.ReceiptVariants.Any())
            {
                return BadRequest("Receipt must have at least one variant.");
            }

            if (!receipt.DateCreate.HasValue)
            {
                receipt.DateCreate = DateTime.Now;
            }

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
                Message = $"Bạn vừa tạo thành công một đơn hàng HD{receipt.Id,10}",
                Type = "Receipt",
                ReferenceId = receiptId,
                DateCreated = DateTime.Now,
                IsRead = false,
            };
            dbc.Notifications.Add(notification);

            await dbc.SaveChangesAsync();

            return Ok(new { receipt });
        }     
    }
}
