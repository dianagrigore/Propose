using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Propose.Models
{
    public class Label
    {
        [Key]
        public int LabelId { get; set; }
        [Required(ErrorMessage = "Numele etichetei este obligatoriu!")]
        public string LabelName { get; set; }

 
        public virtual ICollection<Post> Posts { get; set; }
    }
}