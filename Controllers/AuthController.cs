using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SuperMarket.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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

        return RedirectToAction("Login");//for front
        // return Ok("User registered successfully");
    }

    //
    // [HttpGet]
    // public IActionResult Login()
    // {
    //     return View();
    // }
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

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

   

        await _userManager.UpdateSecurityStampAsync(user);
        
        // return RedirectToAction("Login");//for front
        return Ok(new { message = "Logged out successfully!" });
    }


    private string GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        // Null check
        if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Id))
            throw new ArgumentException("Invalid user data for token generation.");

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim("SecurityStamp", user.SecurityStamp)
    };

        // Add role claims if any
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "yourIssuer",
            audience: "yourAudience",
            claims: claims,
            expires: DateTime.Now.AddHours(720),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
