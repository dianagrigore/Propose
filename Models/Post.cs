using Propose.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Propose.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }
        [Required(ErrorMessage = "Titlul postarii este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 20 caractere")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Continutul postarii este obligatoriu")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
        public DateTime Date { get; set; }            
        [Required(ErrorMessage = "Eticheta este obligatorie")]            
        public int LabelId { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } 

        public virtual Label Label { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        public IEnumerable<SelectListItem> Lbl { get; set; }
    }
}