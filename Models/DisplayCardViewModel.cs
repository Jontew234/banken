﻿using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class DisplayCardViewModel
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }

        [Range(1, 12, ErrorMessage = "Invalid Month")]
        [Required(ErrorMessage = "Expiration Month is required")]
        public int ExpirationMonth { get; set; }

        [Range(2022, 2050, ErrorMessage = "Invalid Year")]
        [Required(ErrorMessage = "Expiration Year is required")]
        public int ExpirationYear { get; set; }

        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Invalid CVV")]
        [Required(ErrorMessage = "CVV is required")]
        public string CVV { get; set; }

        [Required]
        public bool OnlinePurchase { get; set; }

        [Required]
        public bool Active { get; set; }
    }
}
