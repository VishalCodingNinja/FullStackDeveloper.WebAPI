using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FullStackDeveloper.WebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FullStackDeveloper.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ApplicationUserController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _singInManager;
		private readonly ApplicationSettings _appSettings;

		public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<ApplicationSettings> appSettings)
		{
			_userManager = userManager;
			_singInManager = signInManager;
			_appSettings = appSettings.Value;
		}
		/// <summary>
		/// To register a new user on Admin role
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Register")]
		//POST : /api/ApplicationUser/Register
		public async Task<object> PostApplicationUser(ApplicationUserModel model)
		{
			model.Role = "Admin";
			var applicationUser = new ApplicationUser()
			{
				UserName = model.UserName,
				Email = model.Email,
				FullName = model.FullName
			};

			try
			{
				var result = await _userManager.CreateAsync(applicationUser, model.Password);
				await _userManager.AddToRoleAsync(applicationUser, model.Role);
				return Ok(result);
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		/// <summary>
		/// To Login into FullStackDeveloper Website
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Login")]
		//POST : /api/ApplicationUser/Login
		public async Task<IActionResult> Login(LoginModel model)
		{
			var user = await _userManager.FindByNameAsync(model.UserName);
			if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
			{
				//Get role assigned to the user
				var role = await _userManager.GetRolesAsync(user);
				IdentityOptions _options = new IdentityOptions();

				var tokenDescriptor = new SecurityTokenDescriptor
				{
					Subject = new ClaimsIdentity(new Claim[]
					{
						new Claim("UserID",user.Id.ToString()),
						new Claim(_options.ClaimsIdentity.RoleClaimType,role.FirstOrDefault())
					}),
					Expires = DateTime.UtcNow.AddDays(1),
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
				};
				var tokenHandler = new JwtSecurityTokenHandler();
				var securityToken = tokenHandler.CreateToken(tokenDescriptor);
				var token = tokenHandler.WriteToken(securityToken);
				return Ok(new { token });
			}
			else
				return BadRequest(new { message = "Username or password is incorrect." });
		}
	}
}