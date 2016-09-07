using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SerwisOgloszeniowy.Models
{
    public class OgloszeniaContext : DbContext
    {
        public DbSet<Advert> Adverts { get; set; }
        public DbSet<ImagePath> ImagePaths { get; set; }
    }
}