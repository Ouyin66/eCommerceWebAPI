using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        ECOMMERCE dbc;

        public CategoryController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Category/List")]
        public IActionResult GetList()
        {
            return Ok(new { dataa = dbc.Categories.ToList() });
        }

        [HttpGet]
        [Route("/Category/Get")]
        public IActionResult GetCategory(int id)
        {
            Category category = dbc.Categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = "Không tìm thấy loại sản phẩm này" });
            }

            return Ok(new { category });
        }

        [HttpPost]
        [Route("/Category/Insert")]
        public IActionResult InsertCategory(string name)
        {
            Category existingCategory = dbc.Categories.FirstOrDefault(c => c.Name == name);

            if (existingCategory != null)
            {
                return BadRequest(new { message = "Đã tồn tại loại sản phẩm này" });
            }

            Category category = new Category();
            category.Name = name;

            dbc.Categories.Add(category);
            dbc.SaveChanges();
            return Ok(new { category });
        }

        [HttpPut]
        [Route("/Category/Update")]
        public IActionResult UpdateCategory(int id, string name)
        {
            Category existingCategory = dbc.Categories.FirstOrDefault(c => c.Name == name);

            if (existingCategory != null)
            {
                return BadRequest(new { message = "Đã tồn tại loại sản phẩm này" });
            }

            Category category = new Category();
            category.Id = id;
            category.Name = name;

            dbc.Categories.Update(category);
            dbc.SaveChanges();
            return Ok(new { category });
        }

        [HttpDelete]
        [Route("/Category/Delete")]
        public IActionResult DeleteCategory(int id)
        {
            Category category = dbc.Categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = "Không tìm thấy loại sản phẩmh với id này" });
            }

            dbc.Categories.Remove(category);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", category });
        }
    }
}
