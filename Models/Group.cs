using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Propose.Models
{
    public class Group
    {
        //Id grup
        [Key]
        public int GroupId { get; set; }
        //Nume
        [Required(ErrorMessage = "Numele grupului este obligatoriu")]
        [StringLength(20, ErrorMessage = "Numele nu poate avea mai mult de 20 caractere")]
        public string Name { get; set; }
        //Descriere
        [Required(ErrorMessage = "Descrierea grupului este obligatorie")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        //Data la care a fost creat
        public DateTime CreateDate { get; set; }
        //Eticheta 
        [Required(ErrorMessage = "Eticheta este obligatorie")]
        public int LabelId { get; set; }
        //Admin grup
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        //Privacy Settings
        public bool isPrivate = false;
        //Are mai multe postari + membri + drop-down cu etichete 
        public virtual Label Label { get; set; }
        public virtual ICollection<GroupPost> GroupPosts { get; set; }

      
        public virtual ICollection<ApplicationUser> Members { get; set; }

      

        public IEnumerable<SelectListItem> Lbl { get; set; }

    }
}