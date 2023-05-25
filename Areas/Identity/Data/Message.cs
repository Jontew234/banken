using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platinum.Areas.Identity.Data
{
    public class Message
    {
        [Required]
        public int Id { get; set; }
        [StringLength(200, ErrorMessage = "Your subject is to long")]
        public string Subject { get; set; }
        [StringLength(5000, ErrorMessage = "Max lenght of a message is 5000 syllables")]
        public string Body { get; set; }
        [Required]
        public DateTime SentDate { get; set; }
        [Required]
        public bool IsRead { get; set; }

        [Required]
        public string SenderId { get; set; } // foreign key to the user who sent the message
        public virtual Customer Sender { get; set; } // navigation property to the sender
        [Required]
        public string ReceiverId { get; set; } // foreign key to the user who received the message
        public virtual Customer Receiver { get; set; } // navigation property to the receiver
    }
}
