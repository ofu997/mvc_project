using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Exp.Models
{
    public class Response
    {
        [Key]
        public Guid Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM-dd-yyyy hh:mm tt}")]
        [Display(Name = "Created at")]
        public DateTime Date { get; set; }

        public string? UserName { get; set; }

        public string? Prompt { get; set; }

        public string? Result { get; set; }
    }
}
