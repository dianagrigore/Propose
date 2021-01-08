using Propose.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.Security.Application;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.IO;
using Microsoft.AspNet.Identity.Owin;

namespace Propose.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Users
        public ActionResult Index()
        {
            bool esteAdmin = false;
            if (User.IsInRole("Admin"))
            {
                esteAdmin = true;
            }
            var users = from user in db.Users
                        orderby user.UserName
                        select user;
            //Implementare search pe useri
            var search = "";
            if (Request.Params.Get("search") != null)
            {
                search = Request.Params.Get("search").Trim();
                List<string> userIds = db.Users.Where(
                    at => at.Email.Contains(search)
                    || at.UserName.Contains(search)
                    || at.FirstName.Contains(search)
                    || at.LastName.Contains(search)
                    ).Select(a => a.Id).ToList();

                var usersSearched = db.Users.Where(user => userIds.Contains(user.Id));
                ViewBag.UsersList = usersSearched;
            }
            else
            {
                ViewBag.UsersList = users;
            }
            ViewBag.userId = User.Identity.GetUserId();
            ViewBag.esteAdmin = esteAdmin;
            string currentUser = User.Identity.GetUserId();
            // TODO: trebuie validare (verificare daca userul exista)
            ApplicationUser user1 = db.Users.Find(currentUser);
            List<string> prieteni = new List<string>();
            if (User.IsInRole("Admin") || User.IsInRole("RegisteredUser"))
            {
                foreach (Friend f in user1.ReceivedRequests)
                {
                    prieteni.Add(f.User1_Id);
                }
            }
            ViewBag.vizitator = false;
            if (User.IsInRole("Admin") || User.IsInRole("RegisteredUser"))
            {
                foreach (Friend f in user1.SentRequests)
                {
                    prieteni.Add(f.User2_Id);
                }
            }
            else
            {
                ViewBag.vizitator = true;
            }
            ViewBag.prieteni = prieteni;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"].ToString();
            }
            return View();
        }

        private void SendEmailNotification(string toEmail, string subject, string content)
        {
            const string senderEmail = "testemaildaw@gmail.com";
            const string senderPassword = "parola1!@#";
            const String smtpServer = "smtp.gmail.com";
            const int smtPort = 587;
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtPort);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            MailMessage email = new MailMessage(senderEmail, toEmail, subject, content);
            email.IsBodyHtml = true;
            email.BodyEncoding = UTF8Encoding.UTF8;

            try
            {
                System.Diagnostics.Debug.WriteLine("Sending email...");
                smtpClient.Send(email);
                System.Diagnostics.Debug.WriteLine("Email sent!");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error occured while trying to send email");
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
            }


        }

        [HttpPost]
        public ActionResult AddFriend(FormCollection formData)
        {
            string currentUser = User.Identity.GetUserId();
            // TODO: trebuie validare (verificare daca userul exista)
            ApplicationUser user = db.Users.Find(currentUser);
            try
            {
                if (ModelState.IsValid)
                {

                    string friendToAdd = formData.Get("UserId");

                    if (friendToAdd == null)
                        throw new Exception();

                    bool flag = false;

                    foreach (Friend f in user.SentRequests)
                    {
                        if (f.User2_Id == friendToAdd)
                            flag = true;
                    }

                    foreach (Friend f in user.ReceivedRequests)
                    {
                        if (f.User1_Id == friendToAdd)
                            flag = true;
                    }

                    if (flag == false)
                    {
                        Friend friendship = new Friend();
                        friendship.User1_Id = currentUser;
                        friendship.User2_Id = friendToAdd;
                        friendship.Accepted = true; // Accepted = false, iar in lista de cereri -> accept
                        friendship.RequestTime = DateTime.Now;

                        db.Friends.Add(friendship);
                        db.SaveChanges();
                    }
                    return Redirect("/Users/Index/");
                }
                else
                {
                    return Redirect("/Users/Index/");
                }
            }
            catch (Exception)
            {
                return View();
            }

        }
        [HttpPost]
        public ActionResult Accept(FormCollection formData)
        {
            string currentUser = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Find(currentUser);
            try
            {
                Friend friendToAccept = db.Friends.Find(Int32.Parse(formData.Get("FriendId")));
                if (user.ReceivedRequests.Contains(friendToAccept))
                {
                    friendToAccept.Accepted = false;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {

            }

            return Redirect("/Users/Index/");
        }

        [HttpPost]
        public ActionResult Decline(FormCollection formData)
        {
            string currentUser = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Find(currentUser);
            try
            {
                Friend friendToRemove = db.Friends.Find(Int32.Parse(formData.Get("FriendId")));
                if (user.ReceivedRequests.Contains(friendToRemove))
                {
                    db.Friends.Remove(friendToRemove);
                    db.SaveChanges();
                }

            }
            catch (Exception e) { }
            return Redirect("/Users/Index/");
        }




        public ActionResult Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);

            ViewBag.utilizatorCurent = User.Identity.GetUserId();

            ViewBag.esteAdmin = User.IsInRole("Admin");

            ViewBag.Titlu = "Profil utilizator";
            string fullName = user.FirstName + " " + user.LastName;
            if (fullName != " ")
                ViewBag.Titlu = fullName;


            string currentRole = user.Roles.FirstOrDefault().RoleId;

            var userRoleName = (from role in db.Roles
                                where role.Id == currentRole
                                select role.Name).First();

            ViewBag.roleName = userRoleName;

            DateTime dataMin = new DateTime(1800, 1, 1);

            ViewBag.compara = dataMin;



            ViewBag.roleName = userRoleName;
            //Friend Requests
            var frusers = (from userSent in db.Users
                                          .Include(u => u.ReceivedRequests)
                           select userSent).ToList();
            ViewBag.Users = frusers;
            return View(user);
        }


        [Authorize(Roles = "Admin, RegisteredUser")]
        public ActionResult ShowMine()
        {
            return RedirectToAction("Show", new { id = User.Identity.GetUserId() });
        }



        [Authorize(Roles = "RegisteredUser, Admin")]
        public ActionResult Edit(string id)
        {
            try { 
                
            ApplicationUser user = db.Users.Find(id);
                if (User.IsInRole("Admin") || User.Identity.GetUserId() == id)
                {
                    user.AllRoles = GetAllRoles();
                    var userRole = user.Roles.FirstOrDefault();
                    ViewBag.userRole = userRole.RoleId;
                    ViewBag.esteAdmin = User.IsInRole("Admin");

                    return View(user);
                }
                else
                {
                    TempData["message"] = "Nu poti edita profilul altui user.";
                    return RedirectToAction("Index");
                }
            }
            catch(Exception e)
            {
                TempData["message"] = "Operatiunea a esuat.";
                return RedirectToAction("Index");
            }
        }



        [HttpPut]
        [Authorize(Roles = "RegisteredUser, Admin")]

        public ActionResult Edit(string id, ApplicationUser newData, string privacy)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            ViewBag.esteAdmin = User.IsInRole("Admin");
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                if (User.IsInRole("Admin") || User.Identity.GetUserId() == id)
                {
                    if (TryUpdateModel(user))
                    {
                        user.FirstName = newData.FirstName;
                        user.LastName = newData.LastName;
                        user.UserName = newData.UserName;
                        user.Description = newData.Description;
                        user.Birthday = newData.Birthday;
                        user.Email = newData.Email;
                        user.PhoneNumber = newData.PhoneNumber;
                        if (string.Equals(privacy, "Privat"))
                        {
                            user.IsPrivate = true;
                        }
                        else
                            user.IsPrivate = false;
                        if (User.IsInRole("Admin"))
                        {
                            var roles = from role in db.Roles select role;
                            foreach (var role in roles)
                            {
                                UserManager.RemoveFromRole(id, role.Name);
                            }

                            var selectedRole = db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                            UserManager.AddToRole(id, selectedRole.Name);
                        }
                        db.SaveChanges();
                    }
                    return RedirectToAction("Show", new { id = user.Id });
                }
                else
                {
                    TempData["message"] = "Nu poti edita profilul altui user.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                newData.Id = id;
                return View(newData);
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles select role;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var user = UserManager.Users.FirstOrDefault(u => u.Id == id);

            string notificationBody = "<p> Contul dvs. a fost sters de catre administratorul aplicatiei. </p><br />";
            notificationBody += "<br/><br/> O zi frumoasa!";
            SendEmailNotification(user.Email, "Contul dvs. a fost sters", notificationBody);

            var posts = db.Posts.Where(a => a.UserId == id);
            foreach (var post in posts)
            {
                db.Posts.Remove(post);

            }

            var comments = db.Comments.Where(comm => comm.UserId == id);
            foreach (var comment in comments)
            {
                db.Comments.Remove(comment);
            }

            // Commit 
            db.SaveChanges();
            UserManager.Delete(user);
            return RedirectToAction("Index");
        }
    }
}