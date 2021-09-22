using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FishBook.DAL.Interfaces;
using FishBook.DAL.Models;
using FishBook.DAL.ViewModels;
using FishBook.Web.Domains.Requests;
using Microsoft.AspNetCore.Authorization;

namespace FishBook.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserView>> LoginAsync([FromBody]LoginRequest request)
        {
            try
            {
                return await _userRepository.Login(request.Email, request.Password);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("registration")]
        public async Task<ActionResult<UserView>> RegistrationAsync([FromBody] RegistrationRequest request)
        {
            try
            {
                var user = new User() {
                    DisplayName = request.DisplayName,
                    Email = request.Email
                };

                return await _userRepository.Register(user, request.Password);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
