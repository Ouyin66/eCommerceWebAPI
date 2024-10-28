using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariantController : ControllerBase
    {
        ECOMMERCE dbc;

        public VariantController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Variant/List")]
        public IActionResult GetList()
        {
            return Ok(new { dataa = dbc.Variants.ToList() });
        }

        [HttpGet]
        [Route("/Variant/Get")]
        public IActionResult GetVariant(int id)
        {
            Variant variant = dbc.Variants.FirstOrDefault(v => v.Id == id);

            if (variant == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm biến thể" });
            }

            return Ok(new { variant });
        }

        [HttpGet]
        [Route("/Variant/ListByProductId")]
        public IActionResult GetVariantByProduct(int productId)
        {
            List<Variant> variants = dbc.Variants.Where(v => v.ProductId == productId).ToList();

            if (variants == null || !variants.Any())
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm biến thể của sản phẩm" });
            }

            return Ok(new { variants });
        }

        [HttpPost]
        [Route("/Variant/Insert")]
        public IActionResult InsertVariant(int product, int color, int? size, string picture, int quantity)
        {
            // Kiểm tra sự tồn tại của sản phẩm
            Product myProduct = dbc.Products.FirstOrDefault(p => p.Id == product);
            if (myProduct == null)
            {
                return NotFound(new { message = "Không tìm thấy product" });
            }

            // Kiểm tra xem biến thể đã tồn tại chưa
            Variant existingVariant = dbc.Variants.FirstOrDefault(v => v.ProductId == product && v.ColorId == color && v.SizeId == size);
            if (existingVariant != null)
            {
                return BadRequest(new { message = "Đã tồn tại sản phẩm này" });
            }

            // Tạo mới Variant nếu không có sẵn
            Variant variant = new Variant()
            {
                ProductId = product,
                ColorId = color,
                SizeId = size,
                Picture = picture,
                Price = myProduct.Price,
                Quantity = quantity,
                DateCreate = DateTime.Now,
            };

            Picture addPicture = new Picture()
            {
                ProductId = variant.ProductId,
                Image = variant.Picture,
            };

            // Cập nhật số lượng sản phẩm
            myProduct.Amount += quantity;

            // Thêm Variant mới và cập nhật sản phẩm
            dbc.Variants.Add(variant);
            dbc.Products.Update(myProduct);
            if (dbc.Pictures.FirstOrDefault(p => p.Image == addPicture.Image && p.ProductId == addPicture.ProductId) == null)
            {
                dbc.Pictures.Add(addPicture);
            }     
            dbc.SaveChanges();

            // Trả về kết quả thành công
            return Ok(new { variant });
        }

        [HttpPut]
        [Route("/Variant/Update")]
        public IActionResult UpdateVariant(int id, int product, int color, int? size, string picture, int price, int quantity)
        {
            Variant variant = dbc.Variants.FirstOrDefault(v => v.Id == id);

            if (variant == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm biến thể này" });
            }

            Variant existingVariant = dbc.Variants.FirstOrDefault(v => v.ProductId == product && v.ColorId == color && v.SizeId == size && v.Id != id);
            if (existingVariant != null)
            {
                return BadRequest(new { message = "Đã tồn tại sản phẩm biến thể này" });
            }

            variant.ProductId = product;
            variant.ColorId = color;
            variant.SizeId = size;
            variant.Picture = picture ?? variant.Picture;
            variant.Price = price != 0 ? price : variant.Price;

            if (variant.Quantity != quantity) {
                var myProduct = dbc.Products.FirstOrDefault(p => p.Id == product);
                if (quantity != 0) 
                {
                    myProduct.Amount = myProduct.Amount - variant.Quantity + quantity;
                    variant.Quantity = quantity;                  
                } else
                {
                    variant.Quantity = variant.Quantity;
                }                                
            }         

            Picture addPicture = new Picture()
            {
                ProductId = variant.ProductId,
                Image = variant.Picture,
            };

            dbc.Variants.Update(variant);
            var existingPicture = dbc.Pictures.FirstOrDefault(p => p.Image == addPicture.Image && p.ProductId == addPicture.ProductId);
            if (existingPicture == null)
            {              
                dbc.Pictures.Add(addPicture);
            }
            dbc.SaveChanges();
            return Ok(new { variant });
        }

        [HttpDelete]
        [Route("/Variant/Delete")]
        public IActionResult DeleteVariant(int id)
        {
            Variant variant = dbc.Variants.FirstOrDefault(v => v.Id == id);

            if (variant == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm biến thể với id này" });
            }

            dbc.Variants.Remove(variant);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", variant });
        }
    }
}
