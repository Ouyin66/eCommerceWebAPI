using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GenderController : ControllerBase
    {
        ECOMMERCE dbc;

        public GenderController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Gender/List")]
        public IActionResult GetList() 
        {
            return Ok(new {dataa = dbc.Genders.ToList() });
        }

        [HttpGet]
        [Route("/Gender/Get")]
        public IActionResult GetGender(int id)
        {
            Gender gender = dbc.Genders.FirstOrDefault(g => g.Id == id);

            if (gender == null)
            {
                return NotFound(new { message = "Không tìm thấy giới tính" });
            }

            return Ok(new { gender });
        }

        [HttpPost]
        [Route("/Gender/Insert")]
        public IActionResult InsertGender(string name)
        {
            Gender existingGender = dbc.Genders.FirstOrDefault(u => u.Name == name);

            if (existingGender != null)
            {
                return BadRequest(new { message = "Đã tồn tại giới tính này" });
            }

            Gender gender = new Gender();
            gender.Name = name;

            dbc.Genders.Add(gender);
            dbc.SaveChanges();
            return Ok(new { gender });
        }

        [HttpPut]
        [Route("/Gender/Update")]
        public IActionResult UpdateGender(int id, string name)
        {
            Gender existingGender = dbc.Genders.FirstOrDefault(u => u.Name == name);

            if (existingGender != null)
            {
                return BadRequest(new { message = "Đã tồn tại giới tính này" });
            }

            Gender gender = new Gender();
            gender.Id = id;
            gender.Name = name;

            dbc.Genders.Update(gender);
            dbc.SaveChanges();
            return Ok(new { gender });
        }

        [HttpDelete]
        [Route("/Gender/Delete")]
        public IActionResult DeleteGender(int id)
        {
            Gender gender = dbc.Genders.FirstOrDefault(c => c.Id == id);

            if (gender == null)
            {
                return NotFound(new { message = "Không tìm thấy giới tính với id này" });
            }

            dbc.Genders.Remove(gender);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", gender });
        }
    }

}
