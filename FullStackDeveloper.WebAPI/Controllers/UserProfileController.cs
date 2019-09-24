using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FullStackDeveloper.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FullStackDeveloper.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserProfileController : ControllerBase
	{
		private UserManager<ApplicationUser> _userManager;
		public UserProfileController(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}
		/// <summary>
		/// To get the Profile of the user on basis of JWT payload
		/// </summary>
		/// <returns>
		/// Ruterns the User Details
		/// </returns>
		[HttpGet]
		[Authorize]
		//GET : /api/UserProfile
		public async Task<object> GetUserProfile()
		{
			string userId = User.Claims.First(c => c.Type == "UserID").Value;
			var user = await _userManager.FindByIdAsync(userId);
			return new
			{
				user.FullName,
				user.Email,
				user.UserName
			};
		}
		/// <summary>
		/// This endpoint can only be access by the Admin role users
		/// </summary>
		/// <returns>
		/// Returns Strings i.e "Web method for Admin"
		/// </returns>
		[HttpGet]
		[Authorize(Roles = "Admin")]
		[Route("ForAdmin")]
		public string GetForAdmin()
		{
			return "Web method for Admin";
		}
		/// <summary>
		/// It can be access by Only Customers
		/// </summary>
		/// <returns>
		/// Returns Strings i.e "Web method for Customer"
		/// </returns>
		[HttpGet]
		[Authorize(Roles = "Customer")]
		[Route("ForCustomer")]
		public string GetCustomer()
		{
			return "Web method for Customer";
		}
		/// <summary>
		/// This Endpoint can be access by both Admin And Customers
		/// </summary>
		/// <returns>
		/// Returns "Web methods for Admin"
		/// </returns>
		[HttpGet]
		[Authorize(Roles = "Admin,Customer")]
		[Route("ForAdminOrCustomer")]
		public string GetForAdminOrCustomer()
		{
			return "Web method for Admin or Customer";
		}
	}
}