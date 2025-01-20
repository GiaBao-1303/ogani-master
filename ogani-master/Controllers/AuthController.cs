using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using ogani_master.dto;
using ogani_master.Helpers;
using ogani_master.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using ogani_master.enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ogani_master.Areas.Admin.DTO;
using ogani_master.utils;
using Microsoft.AspNetCore.DataProtection;
using System.Linq;


namespace ogani_master.Controllers
{
	public class AuthController : Controller
	{
		private readonly OganiMaterContext context;
		private readonly IWebHostEnvironment _hostEnv;
		private readonly List<string> _roleAccess = new List<string> { "Admin", "Moderator" };
        public AuthController(OganiMaterContext _context, IWebHostEnvironment hostEnv) {
			context = _context;
            _hostEnv = hostEnv;
        }

		[Route("SignIn")]
		public IActionResult SignInPage()
		{
			var eixstingLogin = HttpContext.Session.Get("UserID");

			if (eixstingLogin != null)
			{
				return Redirect("/");
			}

			return View("~/Views/SignIn/Index.cshtml");
		}

		[Route("SignIn/forgot-password")]
		public IActionResult ForgotPasswordPage()
		{
			return View("~/Views/SignIn/ForgotPassword.cshtml");
		}

		[HttpPost]
		[Route("SignIn/forgot-password")]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
		{
			if(!ModelState.IsValid)
			{
				return View("~/Views/SignIn/ForgotPassword.cshtml", forgotPasswordDto);
			}


			User? user = await this.context.users.FirstOrDefaultAsync(u => u.UserName == forgotPasswordDto.Username && u.Email == forgotPasswordDto.email);

			if (user == null) {
				ViewData["ErrorMessage"] = "Username or email is incorrect.";
				return View("~/Views/SignIn/ForgotPassword.cshtml", forgotPasswordDto);
			}

			var request = HttpContext.Request;

			Guid newGuid = Guid.NewGuid();

			string domain = $"{request.Scheme}://{request.Host}";

			string secretUrl = $"{domain}/Confirm?token={newGuid.ToString()}";

			user.token = newGuid.ToString();
			user.token_expired = DateTime.Now.AddMinutes(3);

			await this.context.SaveChangesAsync();

			bool isSuccess = await MailUtils.SendMailGoogleSmtpForgotPasswordAsync(user.Email, "Thay đổi mật khẩu", user.FirstName + " " + user.LastName, secretUrl);

			if (!isSuccess) {
				ViewData["ErrorMessage"] = "There was an issue sending the password reset email. Please try again later.";
				return View("~/Views/SignIn/ForgotPassword.cshtml", forgotPasswordDto);
			}

			ViewData["SuccessMessage"] = "A confirmation email has been sent. Please check your email.";
			return View("~/Views/SignIn/ForgotPassword.cshtml", forgotPasswordDto);
		}

		[HttpPost]
		[Route("v1/SignIn")]
		public async Task<IActionResult> SignIn(UserSignInDto userSignInDto)
		{
			try
			{

				if (!ModelState.IsValid)
				{
					ViewBag.userSignInDto = userSignInDto;
					return View("~/Views/SignIn/Index.cshtml");
				}

				var existingUser = await this.context.users.FirstOrDefaultAsync(u => u.UserName == userSignInDto.Username);

				if (existingUser == null)
				{
					ViewBag.InvalidUserMessage = "The Username or password is not correct.";
					return View("~/Views/SignIn/Index.cshtml");
				}

				bool isValidPassword = BCrypt.Net.BCrypt.Verify(userSignInDto.Password, existingUser.Password);

				if (!isValidPassword)
				{
					ViewBag.InvalidUserMessage = "The Username or password is not correct.";
					return View("~/Views/SignIn/Index.cshtml");
				}

				UserRole role = (UserRole)existingUser.Role;

                HttpContext.Session.SetInt32("UserID", existingUser.UserId);
				HttpContext.Session.SetString("role", role.ToString());

				if (_roleAccess.Contains(role.ToString()))
				{
					return Redirect("/Admin");
				}

				return Redirect("/");

			}
			catch (Exception ex) {
				return Json(new
				{
					Message = ex.Message,
					StackTrace = ex.StackTrace
				});
			}
		}

		[HttpGet]
		[Route("SignUp")]
		public IActionResult SignUp()
		{
			var eixstingLogin = HttpContext.Session.Get("UserID");

			if (eixstingLogin != null)
			{
				return Redirect("/");
			}
			return View("~/Views/SignUp/Index.cshtml");
		}

		[Route("SignUp/v1")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> VerifySignUpV1(UserRegistrationV1Dto userRegistrationV1Dto)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.userRegistrationV1Dto = userRegistrationV1Dto;
				return View("~/Views/SignUp/Index.cshtml");
			}
			string key = Environment.GetEnvironmentVariable("EncryptionKey");
			if (string.IsNullOrEmpty(key)) {
				throw new Exception("The key is not available");
			}

            Guid newGuid = Guid.NewGuid();
            byte[] guidBytes = new byte[16]; 
            newGuid.TryWriteBytes(guidBytes);
            byte[] fourByteArray = new byte[4];
            Array.Copy(guidBytes, 0, fourByteArray, 0, 4);
            string hexString = BitConverter.ToString(fourByteArray).Replace("-", "").ToLower();

            userRegistrationV1Dto.otpCode = hexString;
			userRegistrationV1Dto.otp_expired = DateTime.Now.AddMinutes(3);

            var encryptedBacsicInfo = EncryptionHelper.Encrypt(JsonConvert.SerializeObject(userRegistrationV1Dto), key);

            HttpContext.Session.SetString("BasicInfo", encryptedBacsicInfo);
			HttpContext.Session.SetString("Step", "v2");

			string fullname = userRegistrationV1Dto.FirstName + " " + userRegistrationV1Dto.LastName;

			bool isSuccess = await MailUtils.SendMailGoogleSmtpConfirmEmailAsync(userRegistrationV1Dto.Email, "Xác thực Email", fullname, hexString);

			if(!isSuccess)
			{
				HttpContext.Session.Clear();
				TempData["ErrorMessage"] = "Internal server error";
				return RedirectToAction("SignInPage", "Auth");
            }



			return RedirectToAction("SignUpV2");
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task<IActionResult> SendOtp()
		{
			string? key = Environment.GetEnvironmentVariable("EncryptionKey");
			if (string.IsNullOrEmpty(key)) throw new Exception("The key is not available");

			string? dataEncrypted = HttpContext.Session.GetString("BasicInfo");
			if (string.IsNullOrEmpty(dataEncrypted))
			{
				return RedirectToAction("SignUp");
			}

			var DecryptData = EncryptionHelper.Decrypt(dataEncrypted, key);
			UserRegistrationV1Dto? userRegistrationV1Dto = JsonConvert.DeserializeObject<UserRegistrationV1Dto>(DecryptData);

			if (userRegistrationV1Dto == null || !TryValidateModel(userRegistrationV1Dto))
			{
				return RedirectToAction("SignUp");
			}

			Guid newGuid = Guid.NewGuid();
			byte[] guidBytes = new byte[16];
			newGuid.TryWriteBytes(guidBytes);
			byte[] fourByteArray = new byte[4];
			Array.Copy(guidBytes, 0, fourByteArray, 0, 4);
			string hexString = BitConverter.ToString(fourByteArray).Replace("-", "").ToLower();

			userRegistrationV1Dto.otpCode = hexString;
			userRegistrationV1Dto.otp_expired = DateTime.Now.AddMinutes(3);

			var encryptedBacsicInfo = EncryptionHelper.Encrypt(JsonConvert.SerializeObject(userRegistrationV1Dto), key);

			HttpContext.Session.SetString("BasicInfo", encryptedBacsicInfo);

			string fullname = userRegistrationV1Dto.FirstName + " " + userRegistrationV1Dto.LastName;
			bool isSuccess = await MailUtils.SendMailGoogleSmtpConfirmEmailAsync(userRegistrationV1Dto.Email, "Xác thực Email", fullname, hexString);

			if (!isSuccess)
			{
				HttpContext.Session.Clear();
				TempData["ErrorMessage"] = "Internal server error";
				return RedirectToAction("SignInPage", "Auth");
			}

			TempData["SuccessMessage"] = "The otp has sent";;
			return RedirectToAction("SignUpV2", "Auth");
		}

		[Route("SignUp/v2")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> VerifySignUpV2(UserRegistrationV2Dto userRegistrationV2Dto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					ViewBag.userRegistrationV2Dto = userRegistrationV2Dto;
					return View("~/Views/SignUp/v2.cshtml");
				}

				User? existingUser = await this.context.users.FirstOrDefaultAsync(u => u.UserName == userRegistrationV2Dto.UserName);

				if (existingUser != null)
				{
					ModelState.AddModelError("UserName", "The username already exist. Please choose a different one");
					ViewBag.userRegistrationV2Dto = userRegistrationV2Dto;
					return View("~/Views/SignUp/v2.cshtml");
				}

				string? key = Environment.GetEnvironmentVariable("EncryptionKey");
				if (string.IsNullOrEmpty(key)) throw new Exception("The key is not available");

				string? dataEncrypted = HttpContext.Session.GetString("BasicInfo");
				if (string.IsNullOrEmpty(dataEncrypted))
				{
					return RedirectToAction("SignUp");
				}

				var DecryptData = EncryptionHelper.Decrypt(dataEncrypted, key);

				UserRegistrationV1Dto? userRegistrationV1Dto = JsonConvert.DeserializeObject<UserRegistrationV1Dto>(DecryptData);

				if (userRegistrationV1Dto == null || !TryValidateModel(userRegistrationV1Dto))
				{
					return RedirectToAction("SignUp");
				}

				if(userRegistrationV1Dto.otpCode != userRegistrationV2Dto.otp)
				{
                    ModelState.AddModelError("otp", "The otp is not valid");
                    ViewBag.userRegistrationV2Dto = userRegistrationV2Dto;
                    return View("~/Views/SignUp/v2.cshtml");
                }

				if (userRegistrationV1Dto.otp_expired <= DateTime.Now)
				{
					ModelState.AddModelError("otp", "The otp has expired");
					ViewBag.userRegistrationV2Dto = userRegistrationV2Dto;
					return View("~/Views/SignUp/v2.cshtml");
				}

				var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRegistrationV2Dto.Password, 12);

				var user = new User
				{
					UserName = userRegistrationV2Dto.UserName,
					Password = hashedPassword,
					FirstName = userRegistrationV1Dto.FirstName,
					LastName = userRegistrationV1Dto.LastName,
					Email = userRegistrationV1Dto.Email,
					Address = userRegistrationV1Dto.Address,
					Phone = userRegistrationV1Dto.Phone,
					Gender = userRegistrationV1Dto.Gender == 1,
					Role = (int)UserRole.User,
					ProfilePictureUrl = userRegistrationV1Dto.Gender == 1 ? "User/images/avatar-male-default.png" : "User/images/avatar-woman-default.png",
					Status = 1,
					CreatedBy = "User",
					CreatedDate = DateTime.Now,
					UpdatedBy = "User",
					UpdatedDate = DateTime.Now,
				};

				this.context.users.Add(user);
				this.context.SaveChanges();

				HttpContext.Session.SetInt32("UserID", user.UserId);
				HttpContext.Session.SetString("role", user.Role == (int)UserRole.Admin ? "Admin" : "User");

				return Redirect("/");
			}
			catch (Exception ex) {

				return Json(new
				{
					Message = ex.Message,
					StackTrace = ex.StackTrace
				});
			}
		}

		[Route("SignUp/v2")]
		public IActionResult SignUpV2()
		{
			string step = HttpContext.Session.GetString("Step");

			if (string.IsNullOrEmpty(step) || !step.Equals("v2"))
			{
				return RedirectToAction("SignUp");
			}
			return View("~/Views/SignUp/v2.cshtml");
		}

		private async Task<User?> getCurrentUser()
		{
			var userId = HttpContext.Session.GetInt32("UserID");

			User? user = await this.context.users.FirstOrDefaultAsync(u => u.UserId == userId);

			return user;

		}

		[Route("Logout")]
		[HttpPost]
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Index", "Home");
		}

		[Route("/Profile")]
		public async Task<IActionResult> ProfilePage()
		{

			User? user = await this.getCurrentUser();

			if (user == null) return RedirectToAction("SignInPage", "Auth");

			ViewBag.CurrentUser = user;

			return View("~/Views/Profile/Index.cshtml");
		}

		[Route("/EditProfile")]
		public async Task<IActionResult> EditProfilePage()
		{
			User? user = await this.getCurrentUser();

			if (user == null) return RedirectToAction("SignInPage", "Auth");


            ViewBag.CurrentUser = user;

			return View("~/Views/Profile/EditProfile.cshtml");
		}

		[Route("/UpdateProfile")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateProfileDto)
		{
			try
            {
                User? currentUser = await this.getCurrentUser();

                if (currentUser == null) return RedirectToAction("SignInPage", "Auth");

                if (!ModelState.IsValid)
                {
					User? user = new User
					{
						ProfilePictureUrl = "",
						Phone = updateProfileDto.Phone,
						Address = updateProfileDto.Address,
						Email = updateProfileDto.Email,
						FirstName = updateProfileDto.FirstName,
						LastName = updateProfileDto.LastName,
						Gender = updateProfileDto.Gender == 1,
						UserId = currentUser.UserId,
					};

                    ViewBag.CurrentUser = user;
                    return View("~/Views/Profile/EditProfile.cshtml");
                }


				if(updateProfileDto.ProfilePictureUrl != null)
				{
					string? newPathImageAvatar = null;
                    string extension = Path.GetExtension(updateProfileDto.ProfilePictureUrl.FileName);
                    newPathImageAvatar = $"User/images/{Guid.NewGuid().ToString()}{extension}";
                    var filePath = Path.Combine(_hostEnv.WebRootPath, newPathImageAvatar);
					updateProfileDto.ProfilePictureUrl.CopyTo(new FileStream(filePath, FileMode.Create));

                    currentUser.ProfilePictureUrl = newPathImageAvatar;
                }

                currentUser.Phone = updateProfileDto.Phone;
				currentUser.Email = updateProfileDto.Email;
				currentUser.Address = updateProfileDto.Address;
				currentUser.FirstName = updateProfileDto.FirstName;
				currentUser.LastName = updateProfileDto.LastName;
				currentUser.Gender = updateProfileDto.Gender == 1;

				await this.context.SaveChangesAsync();

                return RedirectToAction("ProfilePage", "Auth");
            }
			catch (Exception ex) {
				return RedirectToAction("Index", "Home");
			}
		}

		[HttpPost]
		[Route("/AddToFavorite")]
		[AutoValidateAntiforgeryToken]
		public async Task<IActionResult> AddToFavorite(int? prodID)
		{
			User? user = await this.getCurrentUser();

			string? role = HttpContext.Session.GetString("role");

			if (user == null) return RedirectToAction("SignInPage", "Auth");

			Product? existingProduct = await this.context.Products.FirstOrDefaultAsync(p => p.PRO_ID == prodID);

			if (existingProduct == null) return NotFound();

			FavoritesModel? existingFavorite = await this.context.Favorites.FirstOrDefaultAsync(f => f.UserID == user.UserId && f.ProductId == prodID);

			if(existingFavorite != null)
			{
				this.context.Favorites.Remove(existingFavorite);
				await this.context.SaveChangesAsync();

                return RedirectToAction("Index", "Product", new { uid = prodID });
            }

			FavoritesModel favorite = new FavoritesModel
			{
				ProductId = existingProduct.PRO_ID,
				UserID = user.UserId,
				CreatedBy = role,
				UpdatedBy = role,
				CreatedDate = DateTime.Now,
				UpdatedDate = DateTime.Now,
			};

			this.context.Favorites.Add(favorite);

			await this.context.SaveChangesAsync();

			return RedirectToAction("Index", "Product", new { uid = prodID });
		}

		[Route("/AccessDenied")]
		public IActionResult AccessDenied()
		{
			return View("~/Views/Access/Denied.cshtml");
		}
    }
}
