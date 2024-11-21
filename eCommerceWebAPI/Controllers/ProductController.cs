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
        public IActionResult InsertProduct(int gender, int category, string name, string describe, int price, byte state)
        {
            Product existingProduct = dbc.Products.FirstOrDefault(p => p.NamePro == name);

            if (existingProduct != null)
            {
                return BadRequest(new { message = "Đã tồn tại sản phẩm này" });
            }

            Product product = new Product()
            {
                GenderId = gender,
                CategoryId = category,
                NamePro = name,
                Describe = describe,
                Discount = 0,
                Price = price,
                Amount = 0,
                State = state,
                DateCreate = DateTime.Now,
            };

            dbc.Products.Add(product);
            dbc.SaveChanges();
            return Ok(new { product });
        }

        [HttpPut]
        [Route("/Product/Update")]
        public IActionResult UpdateProduct(int id, int gender, int category, string name, string describe, int price, byte state)
        {
            Product product = dbc.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm này" });
            }

            Product existingProduct = dbc.Products.FirstOrDefault(p => p.NamePro == name && p.Id != id);
            if (existingProduct != null)
            {
                return BadRequest(new { message = "Tên sản phẩm đã tồn tại" });
            }

            product.GenderId = gender;
            product.CategoryId = category;
            product.NamePro = name ?? product.NamePro;
            product.Describe = describe ?? product.Describe;
            product.Price = price != 0 ? price : product.Price;
            product.State = state;
            product.DateCreate = DateTime.Now;

            dbc.Products.Update(product);
            dbc.SaveChanges();
            return Ok(new { product });
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
