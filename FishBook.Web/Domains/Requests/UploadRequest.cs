using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FishBook.Web.Domains.Requests
{
    public class UploadRequest
    {
        public IFormFile File { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public DateTime? DateTime { get; set; }
    }
}
