using System.Threading.Tasks;
using FishBook.DAL.Models;
using FishBook.DAL.ViewModels;

namespace FishBook.DAL.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<UserView> Login(string email, string password);

        Task<UserView> Register(User userData, string password);

        Task<User> GetUserByNameAsync(string name);

        Task<UserView> GetProfileByNameAsync(string name);
    }
}
