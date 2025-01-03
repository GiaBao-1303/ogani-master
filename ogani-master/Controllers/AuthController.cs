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


namespace ogani_master.Controllers
{
    public class AuthController : Controller
	{
		private readonly OganiMaterContext context;
		public AuthController(OganiMaterContext _context) {
			context = _context;
		}

		[Route("SignIn")]
		public IActionResult SignInPage()
		{
			var eixstingLogin = HttpContext.Session.Get("UserID");

			if(eixstingLogin != null)
			{
				return Redirect("/");
			}

			return View("~/Views/SignIn/Index.cshtml");
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

				if(!isValidPassword)
				{
					ViewBag.InvalidUserMessage = "The Username or password is not correct.";
					return View("~/Views/SignIn/Index.cshtml");
				}

				HttpContext.Session.SetInt32("UserID", existingUser.UserId);
				HttpContext.Session.SetString("role", existingUser.Role == (int)UserRole.Admin ? "Admin" : "User");

				if(existingUser.Role == (int)UserRole.Admin)
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
		public IActionResult VerifySignUpV1(UserRegistrationV1Dto userRegistrationV1Dto)
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

			var encryptedBacsicInfo = EncryptionHelper.Encrypt(JsonConvert.SerializeObject(userRegistrationV1Dto), key); 

			HttpContext.Session.SetString("BasicInfo", encryptedBacsicInfo);
			HttpContext.Session.SetString("Step", "v2");

			return RedirectToAction("SignUpV2");
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

		[Route("Logout")]
		[HttpPost]
		public IActionResult Logout()
		{
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
	}
}
