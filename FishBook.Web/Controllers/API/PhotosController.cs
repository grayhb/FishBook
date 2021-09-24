using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishBook.DAL.Models;
using FishBook.Web.Domains.Requests;
using FishBook.Web.Domains.Responses;
using FishBook.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace FishBook.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoService _photoService;

        public PhotosController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Photo>>> GetItems()
        {
            try
            {
                return await _photoService.GetUserPhotosAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse() { Error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetPhoto(Guid id)
        {
            try
            {
                var filePath = await _photoService.GetPhotoAsync(id);

                if (filePath == null || !System.IO.File.Exists(filePath))
                    return NotFound();

                var image = System.IO.File.OpenRead(filePath);

                return File(image, "image/jpeg");
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse() { Error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}/thumb")]
        public async Task<ActionResult> GetThumbPhoto(Guid id)
        {
            try
            {
                var filePath = await _photoService.GetThumbPhotoAsync(id);

                if (filePath == null || !System.IO.File.Exists(filePath))
                    return NotFound();

                var image = System.IO.File.OpenRead(filePath);

                return File(image, "image/jpeg");
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse() { Error = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<Photo>> CreateItem([FromForm] UploadRequest request)
        {
            try
            {
                var item = await _photoService.CreateAsync(request);

                return item;
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse() { Error = ex.Message });
            }
        }

        [HttpDelete("{id}/delete")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {
            try
            {
                await _photoService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse() { Error = ex.Message });
            }
        }

        [HttpPut("{id}/update")] 
        public async Task<ActionResult<Photo>> UpdateItem(Photo item)
        {
            try
            {
                return await _photoService.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse() { Error = ex.Message });
            }
        }
        
    }
}
