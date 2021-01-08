using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Propose.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace Propose.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public IEnumerable<SelectListItem> AllRoles { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        
  
    
       
        //mai multe de facut in legatura cu asta
        public byte[] ProfilePicture { get; set; }

        public string City { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsPrivate { get; set; }

        [StringLength(150, ErrorMessage = "Descrierea poate avea maxim 150 de caractere.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public DateTime? Birthday { get; set; }
   
        //[InverseProperty("Members")]
        public virtual ICollection<Group> Groups { get; set; }
  
        [ForeignKey("User1_Id")]
        public virtual ICollection<Friend> SentRequests { get; set; }

        [ForeignKey("User2_Id")]
        public virtual ICollection<Friend> ReceivedRequests { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext,
            Propose.Migrations.Configuration>("DefaultConnection"));

       
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<GroupPost> GroupPosts { get; set; }
        public DbSet<Friend> Friends { get; set; }
 
        public DbSet<Group> Groups { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}