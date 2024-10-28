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

            if (carts == null || !carts.Any())
            {
                return NotFound(new { message = "Không tìm sản phẩm này trong giỏ" });
            }

            // Lấy danh sách các đối tượng Cart dựa trên danh sách cartIds duy nhất        

            return Ok(new { carts });
        }

        [HttpPost]
        [Route("/Cart/Insert")]
        public IActionResult InsertCart(int idUser, int idVariant, int quantity, decimal price)
        {
            if (quantity < 0 && quantity > 3)
            {
                return BadRequest(new { message = "Số lượng phải > 1 và <= 3" });
            }

            Cart existingCart = dbc.Carts.FirstOrDefault(c => c.UserId == idUser && c.VariantId == idVariant);
                
            var inventory = dbc.Variants.FirstOrDefault(v => v.Id == idVariant).Quantity;

            if (inventory == 0)
            {
                return BadRequest(new { message = "Sản phẩm đã hết hàng" });
            }

            if (existingCart != null)
            {
                var totalQuantity = (existingCart.Quantity + quantity);
                //if (inventory == 0)
                //{
                //    return BadRequest(new { message = "Sản phẩm đã hết hàng" });
                //}

                if (totalQuantity <= 3) 
                {
                    if (existingCart.Quantity <= inventory && totalQuantity <= inventory)
                    {
                        existingCart.Quantity = totalQuantity;
                        dbc.SaveChanges();
                        return Ok(new { existingCart });
                    } else
                    {
                        return BadRequest(new { message = "Sản phẩm đã số lượng tối đa của số lượng" });
                    }
                }
            }

            Cart cart = new Cart()
            {
                UserId = idUser,
                VariantId = idVariant,
                Quantity = quantity <= inventory && inventory != 0 ? quantity : inventory,
                Price = price,
            };
            

            dbc.Carts.Add(cart);
            dbc.SaveChanges();
            return Ok(new { cart });
        }

        [HttpPut]
        [Route("/Cart/Update")]
        public IActionResult UpdateCart(int id, int idUser, int idVariant, int quantity, decimal price)
        {
            Cart existingCart = dbc.Carts.FirstOrDefault(c => c.Id == id);

            var inventory = dbc.Variants.FirstOrDefault(v => v.Id == idVariant).Quantity;

            if (existingCart != null)
            {
                if (existingCart.Quantity < 3 && existingCart.Quantity < inventory)
                {
                    existingCart.Quantity += quantity;
                    dbc.SaveChanges();
                    return Ok(new { existingCart });

                }
                else
                {
                    return BadRequest(new { message = "Đã tồn tại sản phẩm này trong giỏ" });
                }
            }

            Cart cart = new Cart()
            {
                UserId = idUser,
                VariantId = idVariant,
                Quantity = quantity > 3 ? (quantity > inventory && inventory <= 3 && inventory > 0) ? inventory : 3 : quantity,
                Price = price,
            };

            dbc.Carts.Update(cart);
            dbc.SaveChanges();
            return Ok(new { cart });
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
