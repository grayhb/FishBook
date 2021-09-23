using FishBook.DAL.Interfaces;
using FishBook.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishBook.DAL.Repositories
{
    public interface IPhotoRepository : IBaseRepository<Photo>
    {
        Task<List<Photo>> GetPhotosByOwnerId(string ownerId);
    }

    public class PhotoRepository : BaseRepository<Photo>, IPhotoRepository
    {
        private readonly FishBookContext _context;

        public PhotoRepository(FishBookContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Photo>> GetPhotosByOwnerId(string ownerId)
        {
            return await _context.Photos
                .Where(e => e.OwnerId == ownerId)
                .OrderByDescending(e => e.Created)
                .ToListAsync();
        }
    }
}
