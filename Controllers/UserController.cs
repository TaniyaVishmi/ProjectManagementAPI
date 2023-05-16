using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using API.Context;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementApp.API.Models;
namespace API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {

    private readonly AppDbContext _authContext;
    public UserController(AppDbContext appDbContext)
    {
      _authContext = appDbContext;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Authenticate([FromBody] User userObj)
    {

      if (userObj == null)
      {
        return BadRequest();
      }

      var user = await _authContext.Users
          .FirstOrDefaultAsync(x => x.Username == userObj.Username);

      if (user == null)
        return NotFound(new { Message = "User Not Found!" });


      if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
      {
        return BadRequest(new { Message = "Password is Incorrect" });
      }

     // user.Token = CreateJwt(user);

      return Ok(new

      {
        Token = "user.Token",
        Message = "Login Success!"
      }

      );
    }

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterUser([FromBody] User userObj)
    {

      if (userObj == null)
      {
        return BadRequest();

      }
      if (await CheckUserNameExistAsync(userObj.Username))
        return BadRequest(new { Message = "Usrname Already Exist" });

      if (await CheckEmaiExistAsync(userObj.Email))
        return BadRequest(new { Message = "Email Already Exist" });


      var pass = CheckPasswordStrength(userObj.Password);
      if (!string.IsNullOrEmpty(pass))
        return BadRequest(new { Message = pass.ToString() });


      userObj.Password = PasswordHasher.HashPassword(userObj.Password);
      userObj.Token = "";
      await _authContext.Users.AddAsync(userObj);
      await _authContext.SaveChangesAsync();
      return Ok(new
      {
        Message = "User Registed!"
      });

    }

    private object CheckPasswordStrength()
    {
      throw new NotImplementedException();
    }

    private Task<bool> CheckUserNameExistAsync(string username)
        => _authContext.Users.AnyAsync(x => x.Username == username);

    private Task<bool> CheckEmaiExistAsync(string email)
      => _authContext.Users.AnyAsync(x => x.Email == email);


    private string CheckPasswordStrength(string password)
    {
      StringBuilder sb = new StringBuilder();
      if (password.Length < 8)
        sb.Append("Minimum Password strength should be 8" + Environment.NewLine);
      return sb.ToString();
    }

    private string CreateJwt(User user)
    {
      var jwtTokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes("veryverysectret.....");
      var identity = new ClaimsIdentity(new Claim[]{
      new Claim(ClaimTypes.Role,user.Role),
      new Claim(ClaimTypes.Name,$"{user.Firstname}{user.Lastname}")

    });
      var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = identity,
        Expires = DateTime.Now.AddDays(1),
        SigningCredentials = credentials

      };
      var token = jwtTokenHandler.CreateToken(tokenDescriptor);
      return jwtTokenHandler.WriteToken(token);

    }

    [HttpGet]
    public async Task<ActionResult<User>>GetAllUsers()
    {
      return Ok(await _authContext.Users.ToListAsync());
    }

  }
}