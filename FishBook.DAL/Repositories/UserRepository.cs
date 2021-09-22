using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FishBook.DAL.Interfaces;
using FishBook.DAL.Models;
using FishBook.DAL.Services;
using FishBook.DAL.ViewModels;

namespace FishBook.DAL.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly FishBookContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtGenerator _jwtGenerator;

        public UserRepository(
            FishBookContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtGenerator jwtGenerator
            ) : base(context)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<UserView> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("Email обязательное поле");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new Exception("Пароль обязательное поле");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            if (result.Succeeded)
            {
                return new UserView
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Token = _jwtGenerator.CreateToken(user),
                };
            }

            throw new Exception("Ошибка аутентификации");
        }

        public async Task<UserView> Register(User userData, string password)
        {
            if (string.IsNullOrEmpty(userData.Email))
            {
                throw new Exception("Email обязательное поле");
            }

            if (string.IsNullOrEmpty(userData.DisplayName))
            {
                throw new Exception("Имя обязательное поле");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new Exception("Пароль обязательное поле");
            }

            if (await _context.Users.Where(x => x.Email == userData.Email).AnyAsync())
            {
                throw new Exception($"{userData.Email} уже зарегистрирован!");
            }

            var newUser = new User
            {
                Email = userData.Email,
                DisplayName = userData.DisplayName,
                UserName = userData.Email
            };

            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                return new UserView
                {
                    Id = newUser.Id,
                    Email = newUser.Email,
                    DisplayName = newUser.DisplayName,
                    Token = _jwtGenerator.CreateToken(newUser),
                };
            }

            throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        public async Task<User> GetUserByNameAsync(string name)
        {
            var user = await _context.Users.SingleOrDefaultAsync(e => e.NormalizedUserName.Equals(name.ToUpper()));

            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }

            return user;
        }

        public async Task<UserView> GetProfileByNameAsync(string name)
        {
            var user = await GetUserByNameAsync(name);

            return new UserView
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Id = user.Id
            };
        }

    }
}
