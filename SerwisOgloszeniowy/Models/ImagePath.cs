using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SerwisOgloszeniowy.Models
{
    public class ImagePath
    {
        public Guid Id { get; set; }
        [StringLength(255)]
        public string FileName { get; set; }
        public string Extension { get; set; }

        public string AdvertID { get; set; }

        public virtual Advert Advert { get; set; }
    }
}