using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        ECOMMERCE dbc;

        public ColorController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Color/List")]
        public IActionResult GetList()
        {
            return Ok(new { dataa = dbc.Colors.ToList() });
        }

        [HttpGet]
        [Route("/Color/Get")]
        public IActionResult GetColor(int id)
        {
            Color color = dbc.Colors.FirstOrDefault(c => c.Id == id);

            if (color == null)
            {
                return NotFound(new { message = "Không tìm thấy màu sắc" });
            }

            return Ok(new { color });
        }

        [HttpGet]
        [Route("/Color/ListByProductId")]
        public IActionResult GetColorByProduct(int productId)
        {
            // Lấy danh sách các ColorId duy nhất từ bảng Variant dựa trên productId
            List<int?> colorIds = dbc.Variants
                .Where(v => v.ProductId == productId)
                .Select(v => v.ColorId)
                .Distinct()
                .ToList();

            if (colorIds == null || !colorIds.Any())
            {
                return NotFound(new { message = "Không tìm thấy màu sắc cho sản phẩm" });
            }

            // Lấy danh sách các đối tượng Color dựa trên danh sách colorIds duy nhất
            List<Color> colors = dbc.Colors
                .Where(c => colorIds.Contains(c.Id))
                .ToList();

            return Ok(new { colors });
        }

        [HttpPost]
        [Route("/Color/Insert")]
        public IActionResult InsertColor(string name, string image)
        {
            Color existingColor = dbc.Colors.FirstOrDefault(c => c.Name == name);

            if (existingColor != null)
            {
                return BadRequest(new { message = "Đã tồn tại màu này" });
            }

            Color color = new Color();
            color.Name = name;
            color.Image = image;

            dbc.Colors.Add(color);
            dbc.SaveChanges();
            return Ok(new { color });
        }

        [HttpPut]
        [Route("/Color/Update")]
        public IActionResult UpdateColor(int id, string name, string image)
        {
            Color existingColor = dbc.Colors.FirstOrDefault(c => c.Name == name);

            if (existingColor != null)
            {
                return BadRequest(new { message = "Đã tồn tại màu này" });
            }

            Color color = new Color();
            color.Id = id;
            color.Name = name;
            color.Image = image;

            dbc.Colors.Update(color);
            dbc.SaveChanges();
            return Ok(new { color });
        }

        [HttpDelete]
        [Route("/Color/Delete")]
        public IActionResult DeleteColor(int id)
        {
            Color color = dbc.Colors.FirstOrDefault(c => c.Id == id);

            if (color == null)
            {
                return NotFound(new { message = "Không tìm thấy màu với id này" });
            }

            dbc.Colors.Remove(color);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", color });
        }
    }
}
