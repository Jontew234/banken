using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class MessageViewModel
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
        public string ReceiverId { get; set; }

        [Required]
        public string Sender { get; set; }
    }
}
