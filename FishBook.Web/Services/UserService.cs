using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FishBook.DAL.Interfaces;
using FishBook.DAL.Models;

namespace FishBook.Web.Services
{
    public interface IUserService
    {
        Task<User> GetUser();
    }

    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public UserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public async Task<User> GetUser()
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _userRepository.GetUserByNameAsync(userName);
        }
    }
}
