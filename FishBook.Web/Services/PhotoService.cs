using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using FishBook.DAL.Models;
using FishBook.DAL.Repositories;
using FishBook.Web.Domains.Requests;
using Microsoft.Extensions.Configuration;

namespace FishBook.Web.Services
{
    public interface IPhotoService
    {
        Task<Photo> CreateAsync(UploadRequest request);
        Task<List<Photo>> GetUserPhotosAsync();
        Task<Photo> GetAsync(Guid id);
        Task<string> GetThumbPhotoAsync(Guid id);
        Task<string> GetPhotoAsync(Guid id);

        Task DeleteAsync(Guid id);

        Task<Photo> UpdateAsync(Photo item);
    }

    public class PhotoService : IPhotoService
    {
        private readonly IUserService _userService;
        private readonly IPhotoRepository _photoRepository;
        private readonly IConfiguration _configuration;

        public PhotoService(IUserService userService, IPhotoRepository photoRepository, IConfiguration configuration)
        {
            _configuration = configuration;
            _userService = userService;
            _photoRepository = photoRepository;
        }

        public async Task<Photo> GetAsync(Guid id)
        {
            //var user = await _userService.GetUser();
            var item = await _photoRepository.GetAsync(id);

            //if (item == null || item.OwnerId != user.Id)
            if (item == null)
                return null;

            return item;
        }

        public async Task<string> GetThumbPhotoAsync(Guid id)
        {
            var item = await GetAsync(id);

            if (item == null)
                return null;

            return Path.Combine(BaseDirectory(), item.FileImageThumb);
        }

        public async Task<string> GetPhotoAsync(Guid id)
        {
            var item = await GetAsync(id);

            if (item == null)
                return null;

            return Path.Combine(BaseDirectory(), item.FileImage);
        }

        public async Task<Photo> CreateAsync(UploadRequest request)
        {
            //await _photoRepository.RemoveAsync(Guid.Parse("6ee87b9c-c0d9-4c55-8a66-7b5b6b226e51"));
            var user = await _userService.GetUser();

            var item = new Photo()
            {
                Id = Guid.NewGuid(),
                OwnerId = user.Id,
                Created = DateTime.Now,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                DateTime = request.DateTime,
                FishName = request.FishName
            };

            var image = Image.FromStream(request.File.OpenReadStream());
            image = NormalizeOrientation(image);

            var filePath = Path.Combine(user.Id, $"{item.Id}.jpg");
            var filePathThumb = Path.Combine(user.Id, $"{item.Id}_thumb.jpg");

            SaveImage(image, filePath);
            SaveImage(image, filePathThumb, true);

            item.FileImage = filePath;
            item.FileImageThumb = filePathThumb;

            await _photoRepository.CreateAsync(item);

            return item;
        }

        public async Task<List<Photo>> GetUserPhotosAsync()
        {
            var user = await _userService.GetUser();
            return await _photoRepository.GetPhotosByOwnerId(user.Id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await GetAsync(id);
            if (item == null)
                return;

            var filePath = Path.Combine(BaseDirectory(), item.FileImage);
            if (File.Exists(filePath))
                File.Delete(filePath);

            var fileThumbPath = Path.Combine(BaseDirectory(), item.FileImageThumb);
            if (File.Exists(fileThumbPath))
                File.Delete(fileThumbPath);

            await _photoRepository.RemoveAsync(item);
        }

        public async Task<Photo> UpdateAsync(Photo item)
        {
            var existItem = await GetAsync(item.Id);

            if (existItem == null)
                return null;
            
            var user = await _userService.GetUser();

            if (user.Id != existItem.OwnerId)
                return null;

            existItem.FishName = item.FishName;

            await _photoRepository.EditAsync(existItem);

            return existItem;
        }

        private string SaveImage(Image image, string filePath, bool thumb = false)
        {
            try
            {
                var img = image.Clone() as Image;
                int width = 2500;

                if (thumb)
                    width = 300;

                img = ResizeImage(img, width, width);

                var fileName = Path.Combine(BaseDirectory(), filePath);

                // создаем папки
                Directory.CreateDirectory(new FileInfo(fileName).DirectoryName);

                var codecInfo = GetEncoderInfo("image/jpeg");
                var encoder = Encoder.Quality;
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(encoder, 90L);

                img.Save(fileName, codecInfo, encoderParameters);

                return fileName;
            }
            catch
            {
                return null;
            }
        }

        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private Image ResizeImage(Image img, int maxWidth, int maxHeight)
        {
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                Double xRatio = (double)img.Width / maxWidth;
                Double yRatio = (double)img.Height / maxHeight;
                Double ratio = Math.Max(xRatio, yRatio);
                int nnx = (int)Math.Floor(img.Width / ratio);
                int nny = (int)Math.Floor(img.Height / ratio);
                Bitmap cpy = new Bitmap(nnx, nny, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                        new Rectangle(0, 0, nnx, nny),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }

        }

        private Image NormalizeOrientation(Image image)
        {
            var ExifOrientationTagId = 274;

            if (Array.IndexOf(image.PropertyIdList, ExifOrientationTagId) > -1)
            {
                int orientation;

                orientation = image.GetPropertyItem(ExifOrientationTagId).Value[0];

                if (orientation >= 1 && orientation <= 8)
                {
                    switch (orientation)
                    {
                        case 2:
                            image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                        case 3:
                            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 4:
                            image.RotateFlip(RotateFlipType.Rotate180FlipX);
                            break;
                        case 5:
                            image.RotateFlip(RotateFlipType.Rotate90FlipX);
                            break;
                        case 6:
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 7:
                            image.RotateFlip(RotateFlipType.Rotate270FlipX);
                            break;
                        case 8:
                            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }

                    image.RemovePropertyItem(ExifOrientationTagId);
                }
            }

            return image;
        }

        private string BaseDirectory()
        {
            return _configuration["FishBookUploads"];
        }
    }
}
