using ShareBucket.UserMicroService.Helpers;
using ShareBucket.UserMicroService.Models.Users;
using ShareBucket.UserMicroService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShareBucket.JwtMiddlewareClient.Attributes;
using ShareBucket.DataAccessLayer.Models.Entities;

namespace ShareBucket.UserMicroService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(
            IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);
            Response.Cookies.Append("X-Access-Token", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest model)
        {
            _userService.Register(model);
            return Ok(new { message = "Registration successful" });
        }
        
        [HttpPut]
        public IActionResult Update(UpdateRequest model)
        {
            if (Request.HttpContext.Items["User"] is not User user)
            {
                return Unauthorized();
            }
            
            try
            {
                _userService.Update(user, model);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpDelete]
        public IActionResult Delete()
        {
            if (Request.HttpContext.Items["User"] is not User user)
            {
                return Unauthorized();
            }
            
            
            _userService.Delete(user);
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPost]
        [Route("AddFriend")]
        public IActionResult AddFriend(string userEmail)
        {
            if (Request.HttpContext.Items["User"] is not User user)
            {
                return Unauthorized();
            }

            // Verify if already friends
            if (_userService.IsFriend(user, userEmail))
            {
                return BadRequest(new { message = "Already friends" });
            }

            if (_userService.AddFriend(user, userEmail))
                return Ok(new { message = "Friend added successfully" });
            else
                return BadRequest(new { message = "Friend not found" });
        }
        [HttpPost]
        [Route("RemoveFriend")]
        public IActionResult RemoveFriend(string userEmail)
        {
            if (Request.HttpContext.Items["User"] is not User user)
            {
                return Unauthorized();
            }

            if (_userService.RemoveFriend(user, userEmail))
                return Ok(new { message = "Friend removed successfully" });
            else
                return BadRequest(new { message = "Friend not found" });
        }

        [HttpGet]
        [Route("GetFriends")]
        public IActionResult GetFriends()
        {
            if (Request.HttpContext.Items["User"] is not User user)
            {
                return Unauthorized();
            }

            var friendsUsers = _userService.GetFriends(user);

            return Ok(friendsUsers);
        }

    }
}
