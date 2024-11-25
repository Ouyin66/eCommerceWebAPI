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
        public IActionResult GetCart(int userId, int variantId)
        {
            Cart cart = dbc.Carts.FirstOrDefault(c => c.UserId == userId && c.VariantId == variantId);

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
        public IActionResult UpdateCart(int userId, int oldVariantId, int variantId, int quantity, decimal price)
        {
            // Kiểm tra số lượng có hợp lệ không
            if (quantity <= 0)
            {
                return BadRequest(new { message = "Số lượng không hợp lệ" });
            }

            // Lấy sản phẩm trong giỏ hàng hiện tại
            var currentCart = dbc.Carts.FirstOrDefault(c => c.UserId == userId && c.VariantId == oldVariantId);
            if (currentCart == null)
            {
                return BadRequest(new { message = "Sản phẩm không tồn tại trong giỏ hàng" });
            }

            // Lấy thông tin của sản phẩm mới
            var newVariant = dbc.Variants.FirstOrDefault(v => v.Id == variantId);
            if (newVariant == null || newVariant.Quantity <= 0)
            {
                return BadRequest(new { message = "Sản phẩm đã hết hàng" });
            }

            // Kiểm tra tồn kho của sản phẩm mới
            if (quantity > newVariant.Quantity)
            {
                return BadRequest(new { message = "Số lượng yêu cầu vượt quá hàng trong kho" });
            }

            // Trường hợp thay đổi sang sản phẩm mới
            if (currentCart.VariantId != variantId)
            {
                // Kiểm tra xem giỏ hàng đã có sản phẩm mới này chưa
                var existingCart = dbc.Carts.FirstOrDefault(c => c.UserId == userId && c.VariantId == variantId);
                if (existingCart != null)
                {
                    // Cộng dồn số lượng nếu giỏ hàng đã có sản phẩm
                    int totalQuantity = (int)(existingCart.Quantity + quantity);

                    // Kiểm tra tồn kho
                    if (totalQuantity > newVariant.Quantity)
                    {
                        return BadRequest(new { message = "Số lượng tổng vượt quá tồn kho" });
                    }

                    // Cập nhật số lượng của sản phẩm mới
                    existingCart.Quantity = totalQuantity;

                    // Xóa sản phẩm cũ khỏi giỏ hàng
                    dbc.Carts.Remove(currentCart);
                }
                else
                {
                    // Xóa sản phẩm cũ khỏi giỏ hàng trước
                    dbc.Carts.Remove(currentCart);

                    // Thêm sản phẩm mới vào giỏ hàng
                    var newCart = new Cart
                    {
                        UserId = userId,
                        VariantId = variantId,
                        Quantity = quantity,
                        Price = price
                    };

                    // Thêm giỏ hàng mới
                    dbc.Carts.Add(newCart);
                }
            }
            else
            {
                // Trường hợp không thay đổi sản phẩm, chỉ cập nhật số lượng
                if (quantity > newVariant.Quantity)
                {
                    return BadRequest(new { message = "Số lượng vượt quá hàng tồn kho" });
                }

                currentCart.Quantity = quantity;

                // Cập nhật đối tượng currentCart trong context
                dbc.Carts.Update(currentCart);
            }

            // Lưu các thay đổi
            dbc.SaveChanges();

            // Trả về phản hồi thành công
            return Ok(new
            {
                message = "Cập nhật thành công",
                cart = dbc.Carts.FirstOrDefault(c => c.UserId == userId && c.VariantId == variantId),
            });

        }

        [HttpDelete]
        [Route("/Cart/Delete")]
        public IActionResult DeleteCart(int userId, int variantId)
        {
            Cart cart = dbc.Carts.FirstOrDefault(c => c.UserId == userId && c.VariantId == variantId);

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
