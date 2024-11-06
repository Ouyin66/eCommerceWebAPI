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

        [HttpPost]
        [Route("/Receipts/Insert")]
        public async Task<IActionResult> CreateReceiptWithVariants([FromBody] Receipt receipt)
        {
            if (receipt == null) return BadRequest("Invalid data.");

            // Add the Receipt to the context
            dbc.Receipts.Add(receipt);

            // Ensure each ReceiptVariant is linked to the Receipt's ID after it's generated
            foreach (var variant in receipt.ReceiptVariants)
            {
                variant.Receipt = receipt; // Set the Receipt reference for each variant
            }

            // Save all changes in a single transaction
            await dbc.SaveChangesAsync();

            return Ok(receipt);
        }
    }
}
