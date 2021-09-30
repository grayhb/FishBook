using Microsoft.AspNetCore.Http;
using System;

namespace FishBook.Web.Domains.Requests
{
    public class UploadRequest
    {
        public UploadRequest()
        {
            FishName = "";
        }

        public IFormFile File { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public DateTime? DateTime { get; set; }
        public string FishName { get; set; }
    }
}
