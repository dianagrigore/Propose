using Propose.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace Propose.Controllers
{
    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private int _perPage = 3;

        // GET: Posts
        [Authorize(Roles = "RegisteredUser, Admin")]
        public ActionResult Index()
        {
            var posts = db.Posts.Include("Label").Include("User").OrderBy(a => a.Date);
            var totalItems = posts.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));
            var search = "";
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }

            var paginatedPosts = posts.Skip(offset).Take(this._perPage);


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"].ToString();
            }
            ViewBag.SearchString = search;
            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.Posts = paginatedPosts;


            return View();
        }

        [Authorize(Roles = "RegisteredUser, Admin")]
        public ActionResult Show(int id)
        {
            Post post = db.Posts.Find(id);
            SetAccessRights();

            return View(post);

        }

        //TRIMITERE NOTIFICARI PRIN MAIL
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
        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Show(Comment comm)
        {
            comm.Date = DateTime.Now;
            comm.UserId = User.Identity.GetUserId();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Comments.Add(comm);
                    db.SaveChanges();

                    Post commPost = db.Posts.Find(comm.PostId);
                    string authorEmail = commPost.User.Email;

                    string notificationBody = "<p> A fost adaugat un nou comentariu la postarea dvs. cu titlul: </p>";
                    notificationBody += "<p><strong>" + commPost.Title + "<strong></p>";
                    notificationBody += "<br />";
                    notificationBody += "Comentariul adaugat este: <br /> <br /><em>";
                    notificationBody += comm.Content;
                    notificationBody += "</em>";
                    notificationBody += "<br/><br/> O zi frumoasa!";
                    SendEmailNotification(authorEmail, "Un nou comenariu a fost adaugat la postarea dvs.", notificationBody);

                    return Redirect("/Posts/Show/" + comm.PostId);
                }

                else
                {
                    Post a = db.Posts.Find(comm.PostId);

                    SetAccessRights();

                    return View(a);
                }

            }

            catch (Exception e)
            {
                Post a = db.Posts.Find(comm.PostId);

                SetAccessRights();

                return View(a);
            }

        }

        [Authorize(Roles = "RegisteredUser, Admin")]
        public ActionResult New()
        {
            Post post = new Post();

            // preluam lista de categorii din metoda GetAllCategories()
            post.Lbl = GetAllLabels();

            //preluam ID-ul utilizatorului curent
            post.UserId = User.Identity.GetUserId();

            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "RegisteredUser,Admin")]
        [ValidateInput(false)]
        public ActionResult New(Post post)
        {

            post.Date = DateTime.Now;
            post.UserId = User.Identity.GetUserId();
            try
            {
                if (ModelState.IsValid)
                {
                    post.Content = Sanitizer.GetSafeHtmlFragment(post.Content);
                    db.Posts.Add(post);
                    db.SaveChanges();
                    TempData["message"] = "Postarea a fost adaugata";
                    return RedirectToAction("Index");
                }
                else
                {
                    post.Lbl = GetAllLabels();
                    return View(post);
                }
            }
            catch (Exception e)
            {
                post.Lbl = GetAllLabels();
                return View(post);
            }
        }

        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Edit(int id)
        {

            Post post = db.Posts.Find(id);
            post.Lbl = GetAllLabels();

            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(post);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "RegisteredUser,Admin")]
        [ValidateInput(false)]
        public ActionResult Edit(int id, Post requestPost)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Post post = db.Posts.Find(id);

                    if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(post))
                        {
                            post.Title = requestPost.Title;

                            requestPost.Content = Sanitizer.GetSafeHtmlFragment(requestPost.Content);

                            post.Content = requestPost.Content;
                            post.LabelId = requestPost.LabelId;
                            db.SaveChanges();
                            TempData["message"] = "Postarea a fost modificata";
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    requestPost.Lbl = GetAllLabels();
                    return View(requestPost);
                }
            }
            catch (Exception e)
            {
                requestPost.Lbl = GetAllLabels();
                return View(requestPost);
            }


        }

        [HttpDelete]
        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Delete(int id)
        {
            Post post = db.Posts.Find(id);
            string authorEmail = post.User.Email;
            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Posts.Remove(post);
                db.SaveChanges();
                TempData["message"] = "Postarea a fost stearsa";

                if (User.IsInRole("Admin"))
                {
                    string notificationBody = "<p> Postarea dvs. cu titlul: </p>";
                    notificationBody += "<p><strong>" + post.Title + "<strong></p>";
                    notificationBody += "<p> a fost stearsa de catre administratorul aplicatiei. </p><br />";
                    notificationBody += "Continutul postarii este: <br /> <br /><em>";
                    notificationBody += post.Content;
                    notificationBody += "</em>";
                    notificationBody += "<br/><br/> O zi frumoasa!";
                    SendEmailNotification(authorEmail, "Postarea dvs. a fost stearsa", notificationBody);

                }

                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti o postare care nu va apartine";
                return RedirectToAction("Index");
            }

        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllLabels()
        {

            var selectList = new List<SelectListItem>();


            var labels = from l in db.Labels
                         select l;

            // iteram prin categorii
            foreach (var label in labels)
            {
                // adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = label.LabelId.ToString(),
                    Text = label.LabelName.ToString()
                });
            }
            return selectList;
        }

        private void SetAccessRights()
        {
            ViewBag.afisareButoane = false;
            if (User.IsInRole("RegisteredUser") || User.IsInRole("Admin"))
            {
                ViewBag.afisareButoane = true;
            }

            ViewBag.esteAdmin = User.IsInRole("Admin");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
        }


    }
}