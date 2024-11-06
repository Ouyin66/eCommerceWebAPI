using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        ECOMMERCE dbc;

        public CartController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Cart/List")]
        public IActionResult GetList()
        {
            return Ok(new { dataa = dbc.Carts.ToList() });
        }

        [HttpGet]
        [Route("/Cart/Get")]
        public IActionResult GetCart(int id)
        {
            Cart cart = dbc.Carts.FirstOrDefault(c => c.Id == id);

            if (cart == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm này trong giỏ" });
            }

            return Ok(new { cart });
        }

        [HttpGet]
        [Route("/Cart/ListByUserId")]
        public IActionResult GetCartByUser(int userId)
        {
            // Lấy danh sách các CartId duy nhất từ bảng Variant dựa trên productId
            List<Cart?> carts = dbc.Carts
                .Where(c => c.UserId == userId)
                .ToList();

            //if (carts == null || !carts.Any())
            //{
            //    return NotFound(new { message = "Không có sản phẩm nào trong giỏ" });
            //}    

            return Ok(new { carts });
        }

        [HttpPost]
        [Route("/Cart/Insert")]
        public IActionResult InsertCart(int userId, int variantId, int quantity, decimal price)
        {
            // Kiểm tra giới hạn số lượng
            if (quantity <= 0 || quantity > 3)
            {
                return BadRequest(new { message = "Số lượng phải > 0 và <= 3" });
            }

            // Kiểm tra tồn kho
            var variant = dbc.Variants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null || variant.Quantity == 0)
            {
                return BadRequest(new { message = "Sản phẩm đã hết hàng" });
            }

            var inventory = variant.Quantity;

            // Kiểm tra nếu sản phẩm đã có trong giỏ hàng
            Cart existingCart = dbc.Carts.FirstOrDefault(c => c.UserId == userId && c.VariantId == variantId);

            if (existingCart != null)
            {
                var totalQuantity = existingCart.Quantity + quantity;

                if (totalQuantity > 3)
                {
                    return BadRequest(new { message = "Sản phẩm đã đạt số lượng tối đa trong giỏ hàng" });
                }
                else if (totalQuantity > inventory)
                {
                    return BadRequest(new { message = "Số lượng yêu cầu vượt quá hàng trong kho" });
                }

                // Cập nhật số lượng nếu nằm trong giới hạn
                existingCart.Quantity = totalQuantity;
                dbc.SaveChanges();
                return Ok(new { message = "Đã thêm sản phẩm vào giỏ", existingCart });
            }

            // Thêm sản phẩm mới vào giỏ hàng: Kiểm tra nếu số lượng yêu cầu không vượt quá tồn kho
            if (quantity > inventory)
            {
                return BadRequest(new { message = "Số lượng yêu cầu vượt quá hàng trong kho" });
            }

            // Thêm mới vào giỏ hàng
            Cart cart = new Cart()
            {
                UserId = userId,
                VariantId = variantId,
                Quantity = quantity,
                Price = price,
            };

            dbc.Carts.Add(cart);
            dbc.SaveChanges();
            return Ok(new { message = "Đã thêm sản phẩm vào giỏ", cart });
        }

        [HttpPut]
        [Route("/Cart/Update")]
        public IActionResult UpdateCart(int id, int userId, int variantId, int quantity, decimal price)
        {
            if (quantity < 0)
            {
                return BadRequest(new { message = "Số lượng không hợp lệ" });
            }
            // Tìm kiếm sản phẩm trong giỏ hàng
            Cart searchingCart = dbc.Carts.FirstOrDefault(c => c.Id == id);
            var variant = dbc.Variants.FirstOrDefault(v => v.Id == variantId);

            // Kiểm tra tồn tại của sản phẩm và tồn kho
            if (searchingCart == null)
            {
                return BadRequest(new { message = "Sản phẩm không tồn tại trong giỏ hàng" });
            }
            if (variant == null || variant.Quantity == 0)
            {
                return BadRequest(new { message = "Sản phẩm đã hết hàng" });
            }

            var inventory = variant.Quantity;

            // Kiểm tra giới hạn số lượng và tồn kho
            if (quantity > 3)
            {
                return BadRequest(new { message = "Sản phẩm đã đạt số lượng tối đa trong giỏ hàng" });
            }
            else if (quantity > inventory)
            {
                return BadRequest(new { message = "Số lượng yêu cầu vượt quá hàng trong kho" });
            }

            if (searchingCart.VariantId != variantId)
            {
                var existingCart = dbc.Carts.FirstOrDefault(c => c.UserId == userId && c.VariantId == variantId);

                if (existingCart != null)
                {
                    var total = quantity + existingCart.Quantity;
                    if (total <= 3 && total <= variant.Quantity && existingCart.Id != id)
                    {
                        existingCart.Quantity = total;
                        dbc.Carts.Remove(searchingCart);
                        dbc.SaveChanges();
                        return Ok(new { message = "Cập nhật thành công", cart = existingCart });
                    }
                }
            }          

            // Cập nhật số lượng và giá
            searchingCart.VariantId = variantId;
            searchingCart.Quantity = quantity;
            searchingCart.Price = price; // Cập nhật giá nếu cần
            dbc.SaveChanges();

            return Ok(new { message = "Cập nhật thành công", cart = searchingCart });
        }

        [HttpDelete]
        [Route("/Cart/Delete")]
        public IActionResult DeleteCart(int id)
        {
            Cart cart = dbc.Carts.FirstOrDefault(c => c.Id == id);

            if (cart == null)
            {
                return NotFound(new { message = "Không tìm thấy màu với id này" });
            }

            dbc.Carts.Remove(cart);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", cart });
        }
    }
}
