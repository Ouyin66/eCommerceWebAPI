using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PictureController : ControllerBase
    {
        ECOMMERCE dbc;

        public PictureController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Picture/List")]
        public IActionResult GetList()
        {
            return Ok(new { dataa = dbc.Pictures.ToList() });
        }

        [HttpGet]
        [Route("/Picture/Get")]
        public IActionResult GetPicture(int id)
        {
            Picture picture = dbc.Pictures.FirstOrDefault(p => p.Id == id);

            if (picture == null)
            {
                return NotFound(new { message = "Không tìm thấy màu sắc" });
            }

            return Ok(new { picture });
        }

        [HttpGet]
        [Route("/Picture/ListByProductId")]
        public IActionResult GetPictureByProduct(int productId)
        {
            List<Picture> picutures = dbc.Pictures.Where(p => p.ProductId == productId).ToList();

            if (picutures == null || !picutures.Any())
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm biến thể của sản phẩm" });
            }

            return Ok(new { picutures });
        }

        [HttpPost]
        [Route("/Picture/Insert")]
        public IActionResult InsertPicture(int productId, string image)
        {
            Picture existingPicture = dbc.Pictures.FirstOrDefault(p => p.Image == image);

            if (existingPicture != null)
            {
                return BadRequest(new { message = "Đã tồn tại màu này" });
            }

            Picture picture = new Picture();
            picture.ProductId = productId;
            picture.Image = image;

            dbc.Pictures.Add(picture);
            dbc.SaveChanges();
            return Ok(new { picture });
        }

        [HttpPut]
        [Route("/Picture/Update")]
        public IActionResult UpdatePicture(int id, int productId, string image)
        {
            Picture existingPicture = dbc.Pictures.FirstOrDefault(p => p.Image == image);

            if (existingPicture != null)
            {
                return BadRequest(new { message = "Đã tồn tại hình ảnh này" });
            }

            Picture picture = new Picture();
            picture.Id = id;
            picture.ProductId = productId;
            picture.Image = image;

            dbc.Pictures.Update(picture);
            dbc.SaveChanges();
            return Ok(new { picture });
        }

        [HttpDelete]
        [Route("/Picture/Delete")]
        public IActionResult DeletePicture(int id)
        {
            Picture picture = dbc.Pictures.FirstOrDefault(p => p.Id == id);

            if (picture == null)
            {
                return NotFound(new { message = "Không tìm thấy hình ảnh với id này" });
            }

            dbc.Pictures.Remove(picture);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", picture });
        }
    }
}
