using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Propose.Models
{
    public class GroupPost
    {

        [Key]
        public int GroupPostId { get; set; }
        [Required(ErrorMessage = "Continutul postarii este obligatoriu")]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int GroupId { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Group Group { get; set; }



    }
}