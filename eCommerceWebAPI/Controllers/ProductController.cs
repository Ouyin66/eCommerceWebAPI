using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace eCommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        ECOMMERCE dbc;

        public ProductController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Product/List")]
        public IActionResult GetList()
        {
            return Ok(new { data = dbc.Products.ToList() });
        }

        [HttpGet]
        [Route("/Product/Get")]
        public IActionResult GetProduct(int id)
        {
            Product product = dbc.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm" });
            }

            return Ok(new { product });
        }

        [HttpGet]
        [Route("/Product/Filter")]
        public IActionResult Filter(string productName, List<int?> categoryIds)
        {
            // Lấy danh sách sản phẩm dựa trên các tham số
            var products = dbc.Products.AsQueryable();

            // Lọc theo tên sản phẩm nếu có
            if (!string.IsNullOrEmpty(productName))
            {
                products = products.Where(p => p.NamePro.Contains(productName));
            }

            // Lọc theo danh sách categoryIds nếu có
            if (categoryIds != null && categoryIds.Any())
            {
                products = products.Where(p => categoryIds.Contains(p.CategoryId));
            }

            // Chuyển đổi kết quả thành danh sách
            var productList = products.ToList();

            // Kiểm tra nếu không tìm thấy sản phẩm
            if (productList == null || !productList.Any())
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm nào" });
            }

            return Ok(new { data = productList });
        }

        [HttpPost]
        [Route("/Product/Insert")]
        public IActionResult InsertProduct([FromBody] Product product)
        {
            try
            {
                Product existingProduct = dbc.Products.FirstOrDefault(p => p.NamePro == product.NamePro);

                if (existingProduct != null)
                {
                    return BadRequest(new { message = "Đã tồn tại sản phẩm này" });
                }

                if (!product.DateCreate.HasValue)
                {
                    product.DateCreate = DateTime.Now;
                }

                product.Pictures = null;

                dbc.Products.Add(product);
                dbc.SaveChanges();
                return Ok(new { product });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
        }

        [HttpPut]
        [Route("/Product/Update")]
        public IActionResult UpdateProduct([FromBody] Product product)
        {
            try
            {
                var productCheck = dbc.Products.FirstOrDefault(p => p.Id == product.Id);

                if (productCheck == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm này" });
                }

                Product existingProduct = dbc.Products.FirstOrDefault(p => p.NamePro == product.NamePro && p.Id != product.Id);
                if (existingProduct != null)
                {
                    return BadRequest(new { message = "Tên sản phẩm đã tồn tại" });
                }

                productCheck.CategoryId = product.CategoryId ?? productCheck.CategoryId;
                productCheck.GenderId = product.GenderId ?? productCheck.GenderId;
                productCheck.NamePro = product.NamePro ?? productCheck.NamePro;
                productCheck.Describe = product.Describe ?? productCheck.Describe;
                productCheck.Price = product.Price ?? productCheck.Price;
                productCheck.Discount = product.Discount ?? productCheck.Discount;
                productCheck.State = product.State ?? productCheck.State;

                dbc.Products.Update(productCheck);
                dbc.SaveChanges();
                return Ok(new { product });
            }
            catch (Exception ex) {
                return StatusCode(500, new { message = "Lỗi hệ thống", details = ex.Message });
            }
            
        }

        [HttpDelete]
        [Route("/Product/Delete")]
        public IActionResult DeleteProduct(int id)
        {
            Product product = dbc.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm với id này" });
            }

            dbc.Products.Remove(product);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", product });
        }
    }
}
