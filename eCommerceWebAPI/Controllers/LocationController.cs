using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceWebAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        ECOMMERCE dbc;

        public LocationController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Location/List")]
        public IActionResult GetList()
        {
            return Ok(new { dataa = dbc.Locations.ToList() });
        }

        [HttpGet]
        [Route("/Location/Get")]
        public IActionResult GetLocation(int id)
        {
            Location location = dbc.Locations.FirstOrDefault(l => l.Id == id);

            if (location == null)
            {
                return NotFound(new { message = "Không tìm thấy địa điểm" });
            }

            return Ok(new { location });
        }

        [HttpGet]
        [Route("/Location/ListByUserId")]
        public IActionResult GetLocationByUserId(int userId)
        {
            List<Location> locations = dbc.Locations.Where(l => l.UserId == userId).ToList();

            //if (locations == null || !locations.Any())
            //{
            //    return Ok(new { message = "Không Có địa điểm của người dùng", locations });
            //}

            return Ok(new { locations });
        }

        [HttpPost]
        [Route("/Location/Insert")]
        public IActionResult InsertLocation(int userId,string name, string address)
        {
            Location existingName = dbc.Locations.FirstOrDefault(l => l.Name == name);

            if (existingName != null)
            {
                return BadRequest(new { errorName = "Đã tồn tại tên của địa chỉ này" });
            }

            Location existingAddress = dbc.Locations.FirstOrDefault(l => l.Address == address);

            if (existingAddress != null)
            {
                return BadRequest(new { errorAddress = "Đã tồn tại địa chỉ này" });
            }

            Location location = new Location();
            location.UserId = userId;
            location.Name = name;
            location.Address = address;
            location.DateCreate = DateTime.Now;

            dbc.Locations.Add(location);
            dbc.SaveChanges();
            return Ok(new { message = "Thêm địa chỉ thành công", location });
        }

        [HttpPut]
        [Route("/Location/Update")]
        public IActionResult UpdateLocation(int id, string name, string address)
        {
            Location existingName = dbc.Locations.FirstOrDefault(l => l.Name == name);

            if (existingName != null && existingName.Id != id)
            {
                return BadRequest(new { errorName = "Đã tồn tại tên của địa chỉ này" });
            }

            Location existingAddress = dbc.Locations.FirstOrDefault(l => l.Address == address);

            if (existingAddress != null && existingAddress.Id != id)
            {
                return BadRequest(new { errorAddress = "Đã tồn tại địa chỉ này" });
            }

            Location location = dbc.Locations.FirstOrDefault(l => l.Id == id);
            location.Name = name;
            location.Address = address;

            dbc.Locations.Update(location);
            dbc.SaveChanges();
            return Ok(new {message = "Chỉnh sửa địa chỉ thành công", location });
        }

        [HttpDelete]
        [Route("/Location/Delete")]
        public IActionResult DeleteLocation(int id)
        {
            Location location = dbc.Locations.FirstOrDefault(l => l.Id == id);

            if (location == null)
            {
                return NotFound(new { message = "Không tìm thấy địa chỉ với id này" });
            }

            dbc.Locations.Remove(location);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", location });
        }
    }
}
