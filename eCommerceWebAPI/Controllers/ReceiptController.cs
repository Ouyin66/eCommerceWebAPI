using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.Controllers
{
    [Route("[controller]")]
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
            List<Receipt> receipts = dbc.Receipts.Where(r => r.UserId == userId).ToList();

            //if (locations == null || !locations.Any())
            //{
            //    return Ok(new { message = "Không Có địa điểm của người dùng", locations });
            //}

            return Ok(new { receipts });
        }

        [HttpPost]
        [Route("/Receipts/Insert")]
        public async Task<IActionResult> CreateReceiptWithVariants([FromBody] Receipt receipt)
        {
            if (receipt == null) return BadRequest("Invalid data.");

            
            dbc.Receipts.Add(receipt);

            
            foreach (var variant in receipt.ReceiptVariants)
            {
                variant.Receipt = receipt; 
            }

            
            await dbc.SaveChangesAsync();

            
            int receiptId = receipt.Id;

            
            var orderStatusHistory = new OrderStatusHistory
            {
                ReceiptId = receiptId,
                State = 0,
                Notes = "New order created",
                TimeStamp = DateTime.Now
            };

            // Add OrderStatusHistory to the context
            dbc.OrderStatusHistories.Add(orderStatusHistory);

            // Save changes to include the new OrderStatusHistory
            await dbc.SaveChangesAsync();

            return Ok(receipt);
        }
    }
}
