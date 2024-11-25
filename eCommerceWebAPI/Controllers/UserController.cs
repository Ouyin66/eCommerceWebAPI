using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Org.BouncyCastle.Crypto.Generators;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace eCommerceWebAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        ECOMMERCE dbc;
        private readonly ImageHelper _imageHelper;

        public UserController(ECOMMERCE db)
        {
            dbc = db;
            _imageHelper = new ImageHelper();
        }

        [HttpGet]
        [Route("/User/List")]
        public IActionResult GetList()
        {
            return Ok(new { dataa = dbc.Users.ToList() });
        }

        [HttpGet]
        [Route("/User/Get")]
        public IActionResult GetUser(int id)
        {
            User user = dbc.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            return Ok(new { user });
        }

        [HttpGet]
        [Route("/User/Login")]
        public IActionResult Login(string email, string password)
        {
            User user = dbc.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            //if (email == "admin@gmail.com")
            //{
            //    if (user.Password == password)
            //    {
            //        return Unauthorized(new { message = "Mật khẩu không đúng" });
            //    }
            //} else
            //{
            //    string hashedPassword = HashPassword(password);

            //    // So sánh mật khẩu đã nhập với mật khẩu trong cơ sở dữ liệu
            //    if (user.Password != hashedPassword)
            //    {
            //        return Unauthorized(new { message = "Mật khẩu không đúng" });
            //    }
            //}

            if (user.Password != password)
            {
                return Unauthorized(new { message = "Mật khẩu không đúng" });
            }


            return Ok(new { user });
        }

        [HttpGet]
        [Route("/Users/VariantsPendingFeedback")]
        public async Task<IActionResult> GetVariantsPendingFeedbackForUser(int userId)
        {
            // Lấy tất cả các Receipt của người dùng
            var receipts = await dbc.Receipts
                                     .Where(r => r.UserId == userId)
                                     .ToListAsync();

            // Lấy tất cả các ReceiptVariant cần thiết
            var variantsInReceipts = await (from rv in dbc.ReceiptVariants
                                            join r in receipts on rv.ReceiptId equals r.Id
                                            select rv)
                                .ToListAsync();

            // Lấy tất cả các Feedback đã tồn tại cho các Receipt của người dùng
            var feedbacks = await dbc.Feedbacks.Where(f => receipts.Any(r => r.Id == f.ReceiptId))
                .Select(f => new { f.ReceiptId, f.VariantId })
                .ToListAsync();

            // Lọc các Variant chưa có feedback
            var variantsPendingFeedback = variantsInReceipts.Where(rv => !feedbacks.Any(f => f.ReceiptId == rv.ReceiptId && f.VariantId == rv.VariantId))
                .Select(rv => rv.Variant).ToList(); 

            // Trả về danh sách các Variant cần feedback
            return Ok(variantsPendingFeedback);
        }

        [HttpPost]
        [Route("/User/Register")]
        public async Task<IActionResult> Register(string email, string phone, string password, string name)
        {
            var existingUser = dbc.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email đã được sử dụng!" });
            }
          
            User user = new User();
            user.Name = name;
            user.Email = email;
            user.Phone = phone;
            //user.Password = HashPassword(password);
            user.Password = password;
            user.Role = 1;
            user.State = 1;
            user.DateCreate = DateTime.Now;
            try
            {
                dbc.Users.Add(user);
                await dbc.SaveChangesAsync();

                Notification notification = new Notification
                {
                    UserId = user.Id,
                    Message = $"Chào mừng bạn đến với UNIQLO, hãy cùng mua sắm thỏa thích nhé!",
                    Type = "Register",
                    ReferenceId = null,
                    DateCreated = DateTime.Now,
                    IsRead = false,
                };
                dbc.Notifications.Add(notification);

                await dbc.SaveChangesAsync();

                return Ok(new { user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi đăng ký", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("/User/GoogleSignIn")]
        public async Task<IActionResult> GoogleSignIn(string email, string providerID, string displayName, string photoUrl)
        {
            byte[] imageBytes;

            var existingUser = dbc.Users.FirstOrDefault(u => u.Email == email);

            if (existingUser != null)
            {
                if (existingUser.ProviderId == null)
                {
                    existingUser.ProviderId = providerID;

                    if (!string.IsNullOrEmpty(displayName))
                    {
                        existingUser.Name = displayName;
                    }

                    if (!string.IsNullOrEmpty(photoUrl))
                    {
                        imageBytes = await _imageHelper.DownloadImageAsByteArray(photoUrl);
                        existingUser.Image = imageBytes;
                    }

                    try
                    {
                        dbc.SaveChanges();
                        return Ok(new { existingUser });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { message = "Đã có lỗi xảy ra khi cập nhật thông tin người dùng", error = ex.Message });
                    }
                }
                else if (existingUser.ProviderId == providerID)
                {
                    // Nếu đã có ProviderId, trả về người dùng đã tồn tại
                    return Ok(new { existingUser });
                }
            }

            imageBytes = await _imageHelper.DownloadImageAsByteArray(photoUrl);
            // Nếu email chưa tồn tại, tạo một người dùng mới
            User user = new User
            {

                Name = displayName,
                Email = email,
                Image = imageBytes ?? null,
                ProviderId = providerID,
                Role = 1,
                State = 1,
                Password = GenerateRandomPassword(), // Tạo mật khẩu ngẫu nhiên
                DateCreate = DateTime.Now,
            };

            try
            {
                dbc.Users.Add(user);
                await dbc.SaveChangesAsync();

                Notification notification = new Notification
                {
                    UserId = user.Id,
                    Message = $"Chào mừng bạn đến với UNIQLO, hãy cùng mua sắm thỏa thích nhé!",
                    Type = "Register",
                    ReferenceId = null,
                    DateCreated = DateTime.Now,
                    IsRead = false,
                };
                dbc.Notifications.Add(notification);

                await dbc.SaveChangesAsync();

                return Ok(new { user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi đăng nhập bằng Google", error = ex.Message });
            }
        }       

        [HttpPost]
        [Route("/User/ForgotPassword")]
        public IActionResult ForgotPassword(string email)
        {
            // Tìm người dùng trong hệ thống dựa trên email
            User user = dbc.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return NotFound(new { message = "Không tồn tại email này" });
            }

            // Gửi email chứa mật khẩu
            try
            {
                SendPasswordEmail(user.Email, user.Password); // Hàm gửi email
                return Ok(new { message = "Mật khẩu đã được gửi qua email" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi gửi email", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("/User/Insert")]
        public IActionResult InsertUser(int id, string email, string password, string name, string phone, string? image, byte gender, byte role, byte state)
        {                     
            // Kiểm tra xem email đã tồn tại hay chưa
            if (dbc.Users.Any(u => u.Email == email))
            {
                return Conflict(new { message = "Email này đã được sử dụng." });
            }

            User user = new User
            {
                Name = name,
                Email = email,
                Password = password,
                Phone = phone,
                Image = image != null ? Convert.FromBase64String(image) : null,
                Gender = gender,
                Role = role,
                State = state,
                DateCreate = DateTime.Now
            };

            try
            {
                dbc.Users.Add(user);
                dbc.SaveChanges();
                return Ok(new { user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi thêm người dùng.", error = ex.Message });
            }
        }

        [HttpPut]
        [Route("/User/ChangePassword")]
        public IActionResult UpdateUser(int id, string newPassword)
        {
            // Tìm người dùng dựa trên ID
            User user = dbc.Users.FirstOrDefault(u => u.Id == id);

            // Kiểm tra nếu người dùng không tồn tại
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            user.Password = newPassword ?? user.Password;

            // Lưu thay đổi
            dbc.Users.Update(user);
            dbc.SaveChanges();

            return Ok(new { message = "Mật khẩu đã được thay đổi", user });
        }

        [HttpPut]
        [Route("/User/UpdateInformation")]
        public IActionResult UpdateInformation(int id, string? name, string? phone, string? image, byte? gender)
        {
            // Tìm người dùng dựa trên ID
            User user = dbc.Users.FirstOrDefault(u => u.Id == id);

            // Kiểm tra nếu người dùng không tồn tại
            if (user == null)
            {

                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            // Cập nhật các thông tin của người dùng
            user.Name = name ?? user.Name;
            user.Phone = phone ?? user.Phone;
            user.Image = Convert.FromBase64String(image) ?? user.Image;
            user.Gender = gender ?? null;

            // Lưu thay đổi
            dbc.Users.Update(user);
            dbc.SaveChanges();

            return Ok(new { message = "Người dùng đã được cập nhật thành công", user });
        }      

        [HttpPut]
        [Route("/User/Update")]
        public IActionResult UpdateUser(int id, string email, string password, string name, string phone, string image, byte gender, byte role, byte state)
        {
            // Tìm người dùng dựa trên ID
            User user = dbc.Users.FirstOrDefault(u => u.Id == id);

            // Kiểm tra nếu người dùng không tồn tại
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            var existingUser = dbc.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                if (existingUser.Id == user.Id)
                {
                    user.Email = email ?? user.Email;
                } else
                {
                    return BadRequest(new { message = "Đã tồn tại email này" });
                }            
            }

            // Cập nhật các thông tin của người dùng
            user.Name = name ?? user.Name;
            user.Password = password ?? user.Password;
            user.Phone = phone ?? user.Phone;
            user.Image = Convert.FromBase64String(image) ?? user.Image;
            user.Gender = gender;
            user.Role = role;
            user.State = state;

            // Lưu thay đổi
            dbc.Users.Update(user);
            dbc.SaveChanges();

            return Ok(new { message = "Người dùng đã được cập nhật thành công", user });
        }

        [HttpDelete]
        [Route("/User/Delete")]
        public IActionResult DeleteUser(int id)
        {
            User user = dbc.Users.FirstOrDefault(u => u.Id == id);

            // Kiểm tra nếu người dùng không tồn tại
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            // Xóa người dùng nếu tìm thấy
            dbc.Users.Remove(user);
            dbc.SaveChanges();

            return Ok(new { message = "Người dùng đã bị xóa", user });
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Phương thức để tạo mật khẩu ngẫu nhiên
        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            Random random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void SendPasswordEmail(string toEmail, string password)
        {
            string fromEmail = "ouyin2603@gmail.com"; // Địa chỉ email của bạn
            string fromPassword = "eksr ufqw iywd cyjl"; // Mật khẩu email của bạn (hoặc mật khẩu ứng dụng nếu dùng Gmail)

            var fromAddress = new MailAddress(fromEmail, "Support Team");
            var toAddress = new MailAddress(toEmail);

            string subject = "Yêu cầu cấp lại mật khẩu";
            string body = $"Mật khẩu của bạn là: {password}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // Sử dụng Gmail làm ví dụ
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }

    public class ImageHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<byte[]> DownloadImageAsByteArray(string imageUrl)
        {
            try
            {
                // Gửi yêu cầu GET đến URL của hình ảnh
                HttpResponseMessage response = await _httpClient.GetAsync(imageUrl);

                // Kiểm tra xem phản hồi có thành công không
                response.EnsureSuccessStatusCode();

                // Đọc nội dung và chuyển thành byte[]
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần thiết
                Console.WriteLine($"Error downloading image: {ex.Message}");
                return null;
            }
        }
    }
}
