using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FishBook.DAL.Interfaces;
using FishBook.DAL.ViewModels;

namespace FishBook.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserView>> GetProfile()
        {
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                return await _userRepository.GetProfileByNameAsync(userName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
