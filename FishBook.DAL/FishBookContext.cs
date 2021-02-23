using FishBook.DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FishBook.DAL
{
    public class FishBookContext : IdentityDbContext<User>
    {
        public FishBookContext(DbContextOptions<FishBookContext> options) : base(options) { }


    }
}
