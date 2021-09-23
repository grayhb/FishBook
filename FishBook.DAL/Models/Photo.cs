using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FishBook.DAL.Models
{
    [Table("Photo")]
    public class Photo
    {
        public Photo()
        {
            Tags = "";
        }

        [Key]
        public Guid Id { get; set; }

        [JsonIgnore]
        public string OwnerId { get; set; }

        public DateTime Created { get; set; }

        [JsonIgnore]
        public string FileImage { get; set; }

        [JsonIgnore]
        public string FileImageThumb { get; set; }
        
        public string Tags { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }

        public DateTime? DateTime { get; set; }

    }
}
