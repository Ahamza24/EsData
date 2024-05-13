using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using EsData.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EsData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet("List")]
        [Authorize(Roles = "Admin,Worker")]
        public ActionResult<IEnumerable<EditableUserViewModel>> List()
        {
            var list = userManager
                        .Users
                        .OrderBy(u => u.Email)
                        .Select(u => new EditableUserViewModel()
                        {
                            Id = u.Id,
                            EmailAddress = u.Email,
                            FirstName = u.Firstname,
                            SureName = u.Surename
                        })
                        .ToList();
            return list;
        }

        [HttpGet("Summary/{id}")]
        [Authorize]
        public async Task<ActionResult<EditableUserViewModel>> Summary(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            // find the user 
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // construct ViewModel and populate with the summary user details
            return new EditableUserViewModel()
            {
                Id = user.Id,
                EmailAddress = user.Email,
                FirstName = user.Firstname,
                SureName = user.Surename,
            };
        }

        [HttpGet("Modify/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<EditableUserViewModel>> Modify(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            // find the user 
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // get list of roles assigned to this user
            var userRoles = await userManager.GetRolesAsync(user);

            // construct ViewModel and populate with the user to be modified and the roles
            return new EditableUserViewModel()
            {
                Id = user.Id,
                EmailAddress = user.Email,
                FirstName = user.Firstname,
                SureName = user.Surename,
                Admin = userRoles.Contains(Roles.Admin),
                Worker = userRoles.Contains(Roles.Worker),
                Premium = userRoles.Contains(Roles.Premium)
            };
        }

        [HttpPost]
        [Route("Register")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Register([FromBody] EditableUserViewModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.EmailAddress);
            if (userExists != null)
            {
                return Problem("User already exists!", null, StatusCodes.Status500InternalServerError, "Error", nameof(ProblemDetails));
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.EmailAddress,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.EmailAddress,
                Firstname = model.FirstName,
                Surename = model.SureName
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return Problem("User creation failed! Please check user details and try again.", null, StatusCodes.Status500InternalServerError, "Error", nameof(ProblemDetails));
            }

            // update the roles the stored user is assigned to 
            await this.AddOrRemoveRoleFromUserAsync(user, Roles.Admin, model.Admin);
            await this.AddOrRemoveRoleFromUserAsync(user, Roles.Worker, model.Worker);
            await this.AddOrRemoveRoleFromUserAsync(user, Roles.Premium, model.Premium);

            return Ok(new { Id = user.Id });
        }

        [HttpPut("Modify/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Modify(string id, [FromBody] EditableUserViewModel modifiedUser)
        {
            try
            {
                // the id passed and the id in the object should match, if they don't reject the request
                if (id == null || id != modifiedUser.Id)
                {
                    return Problem("Id passed to call and Id in user object do not match!", null, StatusCodes.Status400BadRequest, "Error", nameof(ProblemDetails));
                }

                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // username and email must be the same due to quirks in ASP.NET Identity and the "out of the box" way we're using it
                user.UserName = user.Email = modifiedUser.EmailAddress;
                user.Firstname = modifiedUser.FirstName;
                user.Surename = modifiedUser.SureName;

                // update the roles the stored user is assigned to 
                await this.AddOrRemoveRoleFromUserAsync(user, Roles.Admin, modifiedUser.Admin);
                await this.AddOrRemoveRoleFromUserAsync(user, Roles.Worker, modifiedUser.Worker);
                await this.AddOrRemoveRoleFromUserAsync(user, Roles.Premium, modifiedUser.Premium);

                if (!string.IsNullOrEmpty(modifiedUser.Password))
                {
                    var resetPasswordToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    await userManager.ResetPasswordAsync(user, resetPasswordToken, modifiedUser.Password);
                }

                await userManager.UpdateAsync(user);
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserViewModel model)
        {
            var user = await userManager.FindByNameAsync(model.EmailAddress);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Name, user.UserName),
                    new Claim(JwtClaimTypes.GivenName, user.Firstname),
                    new Claim(JwtClaimTypes.FamilyName, user.Surename),
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.Id, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(JwtClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddHours(24),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                var tokenViewModel = new TokenViewModel()
                {
                    Data = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo
                };

                var t = new JwtSecurityToken(tokenViewModel.Data);

                return Ok(tokenViewModel);
            }
            return Unauthorized();
        }

        private async Task<bool> AddOrRemoveRoleFromUserAsync(ApplicationUser user, string role, bool addUserToRole)
        {
            // check stated role actually exists
            if (!roleManager.Roles.Select(r => r.Name).Contains(role))
            {
                return false;
            }

            // get list of roles assigned to this user
            var userRoles = await userManager.GetRolesAsync(user);

            // check to see if user already in role
            // you cannot remove a user from a role they're already in
            // and you cannot add them to a role twice
            // both are errors and we need to code to prevent them
            var alreadyInRole = userRoles.Contains(role);

            // now add or remove the role from user as necessary
            bool success = false;
            if (addUserToRole && !alreadyInRole)
            {
                var ir = await userManager.AddToRoleAsync(user, role);
                success = ir.Succeeded;
            }
            else if (!addUserToRole && alreadyInRole)
            {
                var ir = await userManager.RemoveFromRoleAsync(user, role);
                success = ir.Succeeded;
            }
            else
            {
                success = true;
            }

            return success;
        }
    }
}
