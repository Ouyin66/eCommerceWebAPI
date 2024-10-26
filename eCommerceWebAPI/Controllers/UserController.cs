using eCommerceWebAPI.ModelFromDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Org.BouncyCastle.Crypto.Generators;
using System.Text;
using System.Security.Cryptography;

namespace eCommerceWebAPI.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        ECOMMERCE dbc;

        public UserController(ECOMMERCE db)
        {
            dbc = db;
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

        [HttpPost]
        [Route("/User/Register")]
        public IActionResult Register(string email, string password, string name)
        {
            var existingUser = dbc.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email đã được sử dụng!" });
            }
          
            User user = new User();
            user.Name = name;
            user.Email = email;
            //user.Password = HashPassword(password);
            user.Password = password;
            user.Role = 1;
            try
            {
                dbc.Users.Add(user);
                dbc.SaveChanges();
                return Ok(new { user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi đăng ký", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("/User/GoogleSignIn")]
        public IActionResult GoogleSignIn(string email, string providerID, string displayName, string photoUrl)
        {
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
                        existingUser.Image = photoUrl;
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

            // Nếu email chưa tồn tại, tạo một người dùng mới
            User user = new User
            {
                Name = displayName,
                Email = email,
                ProviderId = providerID,
                Role = 1,
                Password = GenerateRandomPassword() // Tạo mật khẩu ngẫu nhiên
            };

            try
            {
                dbc.Users.Add(user);
                dbc.SaveChanges();
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
        public IActionResult InsertUser(int id, string email, string password, string name, string location, string phone, string image, byte gender, byte role, byte state)
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
                Location = location,
                Phone = phone,
                Image = image,
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
        [Route("/User/Update")]
        public IActionResult UpdateUser(int id, string email, string password, string name, string location, string phone, string image, byte gender, byte role, byte state)
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
            user.Email = email ?? user.Email;

            // Kiểm tra nếu có thay đổi mật khẩu, băm mật khẩu trước khi lưu
            //if (!string.IsNullOrEmpty(password))
            //{
            //    user.Password = HashPassword(password); // Băm mật khẩu
            //}
            user.Password = password ?? user.Password;
            user.Location = location ?? user.Location;
            user.Phone = phone ?? user.Phone;
            user.Image = image ?? user.Image;
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
}
