


using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend_ProjectmanagementV2.Data;
using backend_ProjectmanagementV2.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace backend_ProjectManagement.Controllers;

[ApiController]
[Route("users")]

public class UserController : ControllerBase
{

    private const string TokenSecret = "welcometojwtsigninthiskeycanchanginappdotjson";

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
    private DatabaseContext _db = new DatabaseContext();
    private readonly ILogger<UserController> _logger;
    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }


    // GEN TOKEN
    [HttpPost("token")]
    public string GenerateToken([FromBody] User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);
        if (user.UserName != null)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name , user.UserName),

        };


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifetime),
                Issuer = "http://localhost:5157",
                Audience = "http://localhost:5157",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            if (token != null)
            {
                var jwt = tokenHandler.WriteToken(token);
                return jwt;
            }
            else
            {
                return "Failed to create token.";
            }
        }
        return "Failed to create token.";
    }

    [HttpPost("login")]

    public ActionResult<Response> Login([FromBody] UserLogin requestUser)
    {
        User? user = _db.Users.FirstOrDefault(x => x.UserName == requestUser.Username && x.IsDeleted == false);
        string bearerToken = "";
        if (user == null)
        {
            return NotFound(new Response
            {
                Code = 404,
                Message = "NOT FOUND USER",
                Data = null,
            }
                );
        };
        try
        {
            if (user.UserName == requestUser.Username && user.Password == requestUser.Password)
            {
                bearerToken = GenerateToken(user);
            }
            else
            {
                return BadRequest(new Response
                {
                    Code = 400,
                    Message = "user password or pin is wrong or all are wrong",
                    Data = user
                });
            }
        }
        catch
        {
            return StatusCode(500);
        }
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = new Dictionary<string, object>
    {
        { "BearerToken", bearerToken },
        { "UserId", user.Id },
        { "UserName", user.UserName }
    }
        });

    }

    [HttpGet("User", Name = "ShowAllUsers")]

    public ActionResult GetAll()
    {
        // .OrderBy(q => q.Salary) เรียงจากน้อยไปมาก
        // .OrderByDescending(q => q.Salary) เรียงจากมากไปน้อย
        List<User> users = backend_ProjectmanagementV2.Models.User.GetAllUser(_db);
        return Ok(users);
    }


    [HttpPost("User", Name = "CreateUser")]

    public ActionResult<Response> CreateUser([FromBody] UserCreate userCreate)
    {
        if (userCreate.Username != null && userCreate.Password != null )
        {
           
                User user = new User
                {
                    UserName = userCreate.Username,
                    Password = userCreate.Password,
                };
                user.CreateDate = DateTime.Now;
                user.UpdateDate = DateTime.Now;
                string Message = backend_ProjectmanagementV2.Models.User.Create(_db, user);
                return new Response
                {
                    Code = 200,
                    Message = Message,
                    Data = user,
                };
            
        }
        return BadRequest(new Response
        {
            Code = 404,
            Message = "กรอกข้อมูลให้ครบก่อน ****",
        });


    }



    [HttpPut("User", Name = "UserUpdate")]

    public ActionResult<Response> EditUserPassword([FromBody] UserEditer NewData)
    {
        User UserData = _db.Users.Where(e => e.UserName == NewData.Username).AsNoTracking().ToList().First();

        if (NewData.NewPassword != null )
        {
            if (NewData.Username == UserData.UserName && NewData.Password == UserData.Password && UserData.IsDeleted != true)
            {

                UserData.Password = (NewData.NewPassword == null || NewData.NewPassword == "string") ? UserData.Password : NewData.NewPassword;
                


                backend_ProjectmanagementV2.Models.User.EditPassword(_db, UserData);
                _db.SaveChanges();
                return new Response
                {
                    Code = 200,
                    Message = "Password changed successfully",
                };
            }
            return BadRequest(new Response
            {
                Code = 404,
                Message = "UserName password or pin incorrect",
            });
        }
        else
            return new Response
            {
                Code = 400,
                Message = "Bad Request",
                Data = null
            };
    }




    [HttpDelete("User", Name = "DeleteUser")]

    public ActionResult DeleteUser(int id)
    {
        User user = backend_ProjectmanagementV2.Models.User.Delete(_db, id);
        return Ok(user);
    }


    [HttpGet("User/{id}", Name = "User")]

    public ActionResult GetUserById(int id)
    {
        User user = backend_ProjectmanagementV2.Models.User.GetById(_db, id);
        return Ok(user);
    }


}
