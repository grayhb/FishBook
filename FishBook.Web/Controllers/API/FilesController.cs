using FishBook.Web.Domains.Requests;
using Microsoft.AspNetCore.Mvc;

namespace FishBook.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : Controller
    {
        [Route("upload")]
        [HttpPost]
        public IActionResult Index([FromForm] UploadRequest request)
        {
            return Ok();
        }
    }
}
