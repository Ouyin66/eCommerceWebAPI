using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace eCommerceWebAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        ECOMMERCE dbc;

        public PromotionController(ECOMMERCE db)
        {
            dbc = db;
        }

        [HttpGet]
        [Route("/Promotion/List")]
        public IActionResult GetList()
        {
            return Ok(new { dataa = dbc.Promotions.ToList() });
        }

        [HttpGet]
        [Route("/Promotion/ListPromotionHasCode")]
        public IActionResult GetPromotionHasCode()
        {
            return Ok(new { dataa = dbc.Promotions.Where(p => p.Code != null).ToList() });
        }

        [HttpGet]
        [Route("/Promotion/Get")]
        public IActionResult GetPromotion(int id)
        {
            Promotion promotion = dbc.Promotions.FirstOrDefault(p => p.Id == id);

            if (promotion == null)
            {
                return NotFound(new { message = "Không tìm thấy khuyến mãi" });
            }

            return Ok(new { promotion });
        }

        [HttpGet]
        [Route("/Promotion/GetPromotionByCode")]
        public IActionResult GetPromotionByCode(string? code)
        {
            Promotion promotion = dbc.Promotions.FirstOrDefault(p => p.Code == code);

            if (promotion == null)
            {
                return NotFound(new { message = "Không tìm thấy khuyến mãi" });
            }

            var currentDate = DateTime.Now;
            bool isPromotionActive = promotion.StartDate.HasValue && promotion.EndDate.HasValue &&
                                     currentDate >= promotion.StartDate.Value && currentDate <= promotion.EndDate.Value;

            if (!isPromotionActive)
            {
                return BadRequest(new { message = "Khuyến mãi đã hết hạn" });
            }

            return Ok(new { promotion });
        }


        [HttpPost]
        [Route("/Promotion/Insert")]
        public IActionResult InsertPromotion([FromBody] Promotion promotion)
        {
            // Định dạng ngày giờ bạn mong muốn: "dd/MM/yyyy"
            string dateFormat = "dd/MM/yyyy";
            if (!DateTime.TryParseExact(promotion.StartDate.ToString(), dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime parsedStartDate) ||
                !DateTime.TryParseExact(promotion.EndDate.ToString(), dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime parsedEndDate))
            {
                return BadRequest(new { message = "Định dạng ngày giờ không hợp lệ. Vui lòng nhập theo định dạng dd/MM/yyyy" });
            }

            // Kiểm tra khuyến mãi đã tồn tại
            Promotion existingPromotion = dbc.Promotions.FirstOrDefault(p => p.Name == promotion.Name || p.Code == promotion.Code);
            if (existingPromotion != null)
            {
                return BadRequest(new { message = "Đã tồn tại khuyến mãi này" });
            }

            if (!promotion.DateCreate.HasValue)
            {
                promotion.DateCreate = DateTime.Now;
            }
            
            promotion.StartDate = parsedEndDate;
            promotion.EndDate = parsedStartDate;

            dbc.Promotions.Add(promotion);
            dbc.SaveChanges();
            return Ok(new { promotion });
        }

        [HttpPost]
        [Route("/Promotion/AddProduct")]
        public IActionResult AddProductToPromotion(int promotionId, List<int> productIds)
        {
            // Lấy khuyến mãi theo ID
            var promotion = dbc.Promotions.Include(p => p.Products).FirstOrDefault(p => p.Id == promotionId);
            if (promotion == null)
            {
                return NotFound(new { message = "Khuyến mãi không tồn tại." });
            }

            // Thêm sản phẩm vào khuyến mãi
            foreach (var productId in productIds)
            {
                var product = dbc.Products.FirstOrDefault(p => p.Id == productId);
                if (product != null && !promotion.Products.Contains(product))
                {
                    promotion.Products.Add(product);
                }
            }

            // Lưu thay đổi
            dbc.SaveChanges();

            return Ok(new { message = "Sản phẩm đã được thêm vào khuyến mãi." });
        }

        [HttpPost]
        [Route("/Promotion/AddProductByCategory")]
        public IActionResult AddProductToPromotionByCategory(int promotionId, int categoryId)
        {
            // Lấy khuyến mãi theo ID
            var promotion = dbc.Promotions.Include(p => p.Products).FirstOrDefault(p => p.Id == promotionId);
            if (promotion == null)
            {
                return NotFound(new { message = "Khuyến mãi không tồn tại." });
            }

            // Lấy tất cả sản phẩm trong category
            var category = dbc.Categories.Include(c => c.Products).FirstOrDefault(c => c.Id == categoryId);
            if (category == null)
            {
                return NotFound(new { message = "Danh mục không tồn tại." });
            }

            // Thêm tất cả sản phẩm của Category vào Promotion
            foreach (var product in category.Products)
            {
                if (!promotion.Products.Contains(product))
                {
                    promotion.Products.Add(product);
                }
            }

            // Lưu thay đổi
            dbc.SaveChanges();

            return Ok(new { message = "Sản phẩm đã được thêm vào khuyến mãi." });
        }

        [HttpPost]
        [Route("/Promotion/AddProductByGender")]
        public IActionResult AddProductToPromotionByGender(int promotionId, int genderId)
        {
            // Lấy khuyến mãi theo ID
            var promotion = dbc.Promotions.Include(p => p.Products).FirstOrDefault(p => p.Id == promotionId);
            if (promotion == null)
            {
                return NotFound(new { message = "Khuyến mãi không tồn tại." });
            }

            // Lấy tất cả sản phẩm trong Gender
            var gender = dbc.Genders.Include(g => g.Products).FirstOrDefault(g => g.Id == genderId);
            if (gender == null)
            {
                return NotFound(new { message = "Danh mục không tồn tại." });
            }

            // Thêm tất cả sản phẩm của Gender vào Promotion
            foreach (var product in gender.Products)
            {
                if (!promotion.Products.Contains(product))
                {
                    promotion.Products.Add(product);
                }
            }

            // Lưu thay đổi
            dbc.SaveChanges();

            return Ok(new { message = "Sản phẩm đã được thêm vào khuyến mãi." });
        }

        [HttpPost]
        [Route("/Promotion/UpdateAllProductDiscounts")]
        public IActionResult UpdateAllProductDiscounts()
        {
            // Lấy tất cả khuyến mãi trong database
            var promotions = dbc.Promotions.Include(p => p.Products).ToList();
            var currentDate = DateTime.Now;

            // Lặp qua tất cả các khuyến mãi
            foreach (var promotion in promotions)
            {
                // Kiểm tra nếu khuyến mãi có hiệu lực hay không (ngày hiện tại phải nằm trong phạm vi ngày bắt đầu và kết thúc)
                bool isPromotionActive = promotion.StartDate.HasValue && promotion.EndDate.HasValue &&
                                         currentDate >= promotion.StartDate.Value && currentDate <= promotion.EndDate.Value;

                // Cập nhật giá trị discount cho các sản phẩm trong khuyến mãi
                foreach (var product in promotion.Products)
                {
                    if (isPromotionActive)
                    {
                        // Nếu khuyến mãi có perDiscount là tỷ lệ phần trăm (ví dụ: 10% -> 0.10)
                        if (promotion.PerDiscount.HasValue && promotion.PerDiscount.Value < 1)
                        {
                            // Tính discount theo phần trăm của giá sản phẩm
                            product.Discount = product.Price.HasValue && product.Price.Value > 0
                                ? product.Price.Value * promotion.PerDiscount.Value
                                : 0;
                        }
                        else if (promotion.PerDiscount.HasValue)
                        {
                            // Nếu perDiscount là giá trị cố định (ví dụ: 699000)
                            product.Discount = promotion.PerDiscount.Value;
                        }
                    }
                    else
                    {
                        // Nếu khuyến mãi đã hết hạn, đặt discount về 0
                        product.Discount = 0;
                    }
                }
            }

            // Lưu thay đổi
            dbc.SaveChanges();

            return Ok(new { message = "Đã cập nhật discount cho các sản phẩm trong tất cả khuyến mãi." });
        }

        [HttpPut]
        [Route("/Promotion/Update")]
        public IActionResult UpdatePromotion(int id, string? name, string? code, string? describe, decimal? perDiscount, string? startDate, string? endDate, string? banner)
        {
            byte[] imageBytes = Convert.FromBase64String(banner);

            // Tìm bản ghi hiện tại
            Promotion existingPromotion = dbc.Promotions.FirstOrDefault(p => p.Id == id);

            if (existingPromotion == null)
            {
                return NotFound(new { message = "Không tìm thấy khuyến mãi với ID này." });
            }

            // Kiểm tra định dạng ngày tháng nếu được cung cấp
            string dateFormat = "dd/MM/yyyy";
            if (startDate != null && !DateTime.TryParseExact(startDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime parsedStartDate))
            {
                return BadRequest(new { message = "Định dạng ngày bắt đầu không hợp lệ. Vui lòng nhập theo định dạng dd/MM/yyyy." });
            }

            if (endDate != null && !DateTime.TryParseExact(endDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime parsedEndDate))
            {
                return BadRequest(new { message = "Định dạng ngày kết thúc không hợp lệ. Vui lòng nhập theo định dạng dd/MM/yyyy." });
            }

            // Kiểm tra nếu tên hoặc mã bị trùng (trừ trường hợp chính nó)
            if (!string.IsNullOrEmpty(name) && dbc.Promotions.Any(p => p.Name == name && p.Id != id))
            {
                return BadRequest(new { message = "Tên khuyến mãi đã tồn tại." });
            }

            if (!string.IsNullOrEmpty(code) && dbc.Promotions.Any(p => p.Code == code && p.Id != id))
            {
                return BadRequest(new { message = "Mã khuyến mãi đã tồn tại." });
            }

            // Cập nhật các thông tin (chỉ thay đổi nếu giá trị mới được cung cấp)
            existingPromotion.Name = name ?? existingPromotion.Name;
            existingPromotion.Code = code ?? existingPromotion.Code;
            existingPromotion.Describe = describe ?? existingPromotion.Describe;
            existingPromotion.PerDiscount = perDiscount ?? existingPromotion.PerDiscount;
            if (!string.IsNullOrEmpty(startDate)) existingPromotion.StartDate = DateTime.ParseExact(startDate, dateFormat, null);
            if (!string.IsNullOrEmpty(endDate)) existingPromotion.EndDate = DateTime.ParseExact(endDate, dateFormat, null);
            existingPromotion.Banner = imageBytes ?? existingPromotion.Banner;

            // Lưu thay đổi
            dbc.Promotions.Update(existingPromotion);
            dbc.SaveChanges();

            return Ok(new { promotion = existingPromotion });
        }

        [HttpDelete]
        [Route("/Promotion/Delete")]
        public IActionResult DeletePromotion(int id)
        {
            Promotion promotion = dbc.Promotions.FirstOrDefault(p => p.Id == id);

            if (promotion == null)
            {
                return NotFound(new { message = "Không tìm thấy khuyến mãi với id này" });
            }

            dbc.Promotions.Remove(promotion);
            dbc.SaveChanges();

            return Ok(new { message = "Xóa thành công", promotion });
        }
    }
}
