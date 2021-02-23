using FishBook.DAL.Models;

namespace FishBook.DAL.Services
{
    public interface IJwtGenerator
    {
        string CreateToken(User user);
    }
}
