using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SerwisOgloszeniowy.Models
{
    public class Advert
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Tytuł")]
        public string Title { get; set; }
        [DisplayName("Opis")]
        public string Description { get; set; }
        public string UserID { get; set; }
        [DisplayName("Zdjęcia")]
        public virtual ICollection<ImagePath> ImagePaths { get; set; }
    }
}