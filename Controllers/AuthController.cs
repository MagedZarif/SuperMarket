using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SuperMarket.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


[Route("superMarket/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
    {
        
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    // [HttpGet]
    // public IActionResult Register()
    // {
    //     return View();
    // }
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        var user = new IdentityUser { UserName = model.UserName, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, model.Role);
        return Ok("User registered successfully");
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        try
        {

            if (model == null || string.IsNullOrWhiteSpace(model.UserName) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            // Find user by username
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // Check password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // Get roles for token (optional if you want roles in JWT)
            var roles = await _userManager.GetRolesAsync(user);


            Console.WriteLine($"Username: {user.UserName}, Id: {user.Id}");
            // Generate JWT token
            var token = GenerateJwtToken(user, roles);
            
            
            var tokenEntry = new IdentityUserToken<string>
            {
                UserId = user.Id,
                LoginProvider = "JWT",
                Name = "AccessToken",
                Value = token
            };
            await _userManager.SetAuthenticationTokenAsync(user, tokenEntry.LoginProvider, tokenEntry.Name, tokenEntry.Value);
            return Ok(new
            {
                token = token,
                username = user.UserName,
                roles = roles
            });

        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }

    }
    
    [HttpPost("logout")]

    public async Task<IActionResult> Logout()
    {
        
        var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userName))
        {
            
            return Unauthorized("User not found in token.");
        }
    
       
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return Unauthorized("notfound in token.");
        }
        
        await _userManager.UpdateSecurityStampAsync(user);

        await _signInManager.SignOutAsync();
        
        return Ok(new { message = "Logged out successfully!",user=user });
    }
    
    

    [HttpDelete("delete-account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
       
        var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userName))
        {
            
            return Unauthorized("User not found in token.");
        }
    
       
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return Unauthorized("notfound in token.");
        }
        
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
    
        return Ok(new { message = "Account deleted successfully!", username = user.UserName });
    }
    private string GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Id))
            throw new ArgumentException("Invalid user data for token generation.");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("SecurityStamp", user.SecurityStamp)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(720),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
