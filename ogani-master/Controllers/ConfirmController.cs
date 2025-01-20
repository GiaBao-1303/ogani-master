using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using ogani_master.dto;
using ogani_master.Models;

namespace ogani_master.Controllers
{
	public class ConfirmController:Controller
	{
		private readonly OganiMaterContext context;
		private readonly IWebHostEnvironment _hostEnv;
		public ConfirmController(OganiMaterContext _context, IWebHostEnvironment hostEnv)
		{
			context = _context;
			_hostEnv = hostEnv;
		}

		public async Task<ActionResult> Index(string token) 
		{
			User? user = await this.context.users.FirstOrDefaultAsync(u => u.token == token);

			if (user == null) return NotFound();

			if (user.token_expired > DateTime.Now)
			{
				ViewBag.token = token;
				return View("~/Views/Confirm/Index.cshtml");
            }
            ViewBag.Settings = context.Settings.ToList();
            return View("~/Views/Confirm/Expired.cshtml");
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		[Route("Confirm/ResetPassword")]
		public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
		{
			if(!ModelState.IsValid)
			{
				if(resetPasswordDto.token == null)
				{
                    TempData["ErrorMessage"] = "Invalid or missing token. Please request a new password reset link.";
                    return RedirectToAction("ForgotPasswordPage", "Auth");
				}
                ViewBag.token = resetPasswordDto.token;
                return View("~/Views/Confirm/Index.cshtml");
            }

			User? user = await this.context.users.FirstOrDefaultAsync(u => u.token == resetPasswordDto.token);

			if (user == null) return NotFound();

            if (user.token_expired <= DateTime.Now)
			{
                return View("~/Views/Confirm/Expired.cshtml");
            }

			user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.newPassword, 12);

			user.token = null;
			user.token_expired = null;

			await this.context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been successfully reset. You can now log in with your new password.";
            return RedirectToAction("ForgotPasswordPage", "Auth");
        }
	}
}
