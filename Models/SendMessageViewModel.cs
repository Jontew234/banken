using Platinum.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Platinum.Models
{
    public class SendMessageViewModel
    {
        public string? Subject { get; set; }
        [StringLength(5000, ErrorMessage = "Max lenght of a message is 5000 syllables")]
        public string? Body { get; set; }

        public List<Customer>? Receivers { get; set; }

        public List<string> SelectedReceiverIds { get; set; }

        public string? PreSelectedId { get; set; }
    }
}
