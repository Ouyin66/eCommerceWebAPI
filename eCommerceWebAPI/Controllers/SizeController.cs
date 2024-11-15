using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace eCommerceWebAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        ECOMMERCE dbc;

        public SizeController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Size/List")]
        public IActionResult GetList()
        {
            return Ok(new { dataa = dbc.Sizes.ToList() });
        }

        [HttpGet]
        [Route("/Size/Get")]
        public IActionResult GetSize(int id)
        {
            Size size = dbc.Sizes.FirstOrDefault(s => s.Id == id);

            if (size == null)
            {
                return NotFound(new { message = "Không tìm thấy kích cỡ" });
            }

            return Ok(new { size });
        }

        [HttpGet]
        [Route("/Size/ListByProductId")]
        public IActionResult GetSizeByProduct(int productId)
        {
            // Lấy danh sách các ColorId duy nhất từ bảng Variant dựa trên productId
            List<int?> sizeIds = dbc.Variants
                .Where(s => s.ProductId == productId)
                .Select(s => s.SizeId)
                .Distinct()
                .ToList();

            foreach (var id in sizeIds)
            {
                Console.WriteLine(id.ToString());
            }

            if (sizeIds == null || !sizeIds.Any())
            {
                return NotFound(new { message = "Không tìm thấy kích cỡ cho sản phẩm" });
            }

            // Lấy danh sách các đối tượng Color dựa trên danh sách colorIds duy nhất
            List<Size> sizes = dbc.Sizes
                .Where(s => sizeIds.Contains(s.Id))
                .ToList();

            return Ok(new { sizes });
        }

        [HttpPost]
        [Route("/Size/Insert")]
        public IActionResult InsertSize(string name)
        {
            Size existingSize = dbc.Sizes.FirstOrDefault(s => s.Name == name);

            if (existingSize != null)
            {
                return BadRequest(new { message = "Đã tồn tại kích cỡ này" });
            }

            Size size = new Size();
            size.Name = name;

            dbc.Sizes.Add(size);
            dbc.SaveChanges();
            return Ok(new { size });
        }

        [HttpPut]
        [Route("/Size/Update")]
        public IActionResult UpdateSize(int id, string name)
        {
            Size existingSize = dbc.Sizes.FirstOrDefault(s => s.Name == name);

            if (existingSize != null)
            {
                return BadRequest(new { message = "Đã tồn tại kích cỡ này" });
            }

            Size size = new Size();
            size.Id = id;
            size.Name = name;

            dbc.Sizes.Update(size);
            dbc.SaveChanges();
            return Ok(new { size });
        }

        [HttpDelete]
        [Route("/Size/Delete")]
        public IActionResult DeleteSize(int id)
        {
            Size size = dbc.Sizes.FirstOrDefault(s => s.Id == id);

            if (size == null)
            {
                return NotFound(new { message = "Không tìm thấy kích cỡ với id này" });
            }

            dbc.Sizes.Remove(size);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", size });
        }
    }
}
