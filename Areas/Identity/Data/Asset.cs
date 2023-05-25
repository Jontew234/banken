using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;

namespace Platinum.Areas.Identity.Data
{
    public class Asset
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Exchange { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Risk { get; set; }
        [Required]
        public string Type { get; set; }

        public DateTime LastUpdated { get; set; }

        //public decimal Roi { get; set; }

      

       

    }
}
