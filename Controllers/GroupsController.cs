using Microsoft.AspNet.Identity;
using Microsoft.Security.Application;
using Propose.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Propose.Controllers
{
    public class GroupsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private int _perPage = 3;

        // GET: Groups
        [Authorize(Roles = "RegisteredUser, Admin")]
        public ActionResult Index()
        {
            var groups = db.Groups.Include("Label").Include("User").OrderBy(a => a.CreateDate);
            var totalItems = groups.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));
            var search = "";
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }

            var paginatedPosts = groups.Skip(offset).Take(this._perPage);


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"].ToString();
            }
            ViewBag.SearchString = search;
            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.Groups = paginatedPosts;
            ViewBag.utilizatorulCurent = User.Identity.GetUserId();
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


        [Authorize(Roles = "RegisteredUser, Admin")]
        public ActionResult Show(int id)
        {
            Group group = db.Groups.Find(id);
            SetAccessRights(id);
            return View(group);
        }

        //Show POSTS in Groups
        [HttpPost]
        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Show(GroupPost pst)
        {
            pst.Date = DateTime.Now;
            pst.UserId = User.Identity.GetUserId();
            try
            {
                if (ModelState.IsValid)
                {
                    db.GroupPosts.Add(pst);
                    db.SaveChanges();

                    Group postGroup = db.Groups.Find(pst.GroupId);
                    string authorEmail = postGroup.User.Email;

                    return Redirect("/Groups/Show/" + pst.GroupId);
                }

                else
                {
                    Group a = db.Groups.Find(pst.GroupId);

                    SetAccessRights(pst.GroupId);

                    return View(a);
                }

            }

            catch (Exception e)
            {
                Group a = db.Groups.Find(pst.GroupId);

                SetAccessRights(pst.GroupId);

                return View(a);
            }

        }



        [Authorize(Roles = "RegisteredUser, Admin")]
        public ActionResult New()
        {
            Group group = new Group();

            // preluam lista de categorii din metoda GetAllCategories()
            group.Lbl = GetAllLabels();
            return View(group);
        }

        [HttpPost]
        [Authorize(Roles = "RegisteredUser,Admin")]
        [ValidateInput(false)]
        public ActionResult New(Group group)
        {

            db.SaveChanges();
            try
            {
                string currentUser = User.Identity.GetUserId();
                group.UserId = currentUser;
                if (ModelState.IsValid)
                {

                    var user = db.Users.Find(currentUser);
                    group.CreateDate = DateTime.Now;
                    group.User = user;
                    group.Members = new HashSet<ApplicationUser>();
                    group.Members.Add(user);

                    group.Description = Sanitizer.GetSafeHtmlFragment(group.Description);
                    db.Groups.Add(group);
                    db.SaveChanges();
                    TempData["message"] = "Ai creat un nou grup!";
                    return RedirectToAction("Index");
                }
                else
                {
                    group.Lbl = GetAllLabels();
                    return View(group);
                }
            }
            catch (Exception e)
            {
                group.Lbl = GetAllLabels();
                return View(group);
            }
        }

        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Edit(int id)
        {

            Group group = db.Groups.Find(id);
            group.Lbl = GetAllLabels();

            if (group.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(group);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui grup pe care nu l-ati creat";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "RegisteredUser,Admin")]
        [ValidateInput(false)]
        public ActionResult Edit(int id, Group requestGroup)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Group group = db.Groups.Find(id);

                    if (group.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(group))
                        {
                            group.Name = requestGroup.Name;

                            requestGroup.Description = Sanitizer.GetSafeHtmlFragment(requestGroup.Description);

                            group.Description = requestGroup.Description;
                            group.LabelId = requestGroup.LabelId;
                            db.SaveChanges();
                            TempData["message"] = "Setarile grupului au fost modificate";
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui grup pe care nu l-ati creat";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    requestGroup.Lbl = GetAllLabels();
                    return View(requestGroup);
                }
            }
            catch (Exception e)
            {
                requestGroup.Lbl = GetAllLabels();
                return View(requestGroup);
            }


        }

        [AcceptVerbs(HttpVerbs.Delete | HttpVerbs.Post)]
        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Delete(int id)
        {
            Group group = db.Groups.Find(id);
            ApplicationUser user = db.Users.Find(group.UserId);
            string authorEmail = user.Email;
            if (group.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                group.Members.Clear();
                db.Groups.Remove(group);

                db.SaveChanges();
                TempData["message"] = "Grupul a fost sters";

                if (User.IsInRole("Admin"))
                {
                    string notificationBody = "<p> Grupul dvs. cu numele: </p>";
                    notificationBody += "<p><strong>" + group.Name + "<strong></p>";
                    notificationBody += "<p> a fost sters de catre administratorul aplicatiei. </p><br />";
                    notificationBody += "<br/><br/> O zi frumoasa!";
                    SendEmailNotification(authorEmail, "Grupul dvs. a fost sters", notificationBody);

                }


                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un grup care nu va apartine.";
                return RedirectToAction("Index");
            }

        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Delete | HttpVerbs.Post | HttpVerbs.Get)]
        public ActionResult Join(FormCollection formData)
        {
            try
            {

                Group group = db.Groups.Find(Int32.Parse(formData.Get("GroupIdJ")));
                string currentUser = User.Identity.GetUserId();
                ApplicationUser user = db.Users.Find(currentUser);
                if (group.Members.Contains(user))
                {
                    TempData["status"] = "Deja esti parte din grup";
                    return RedirectToAction("Index");
                }
                group.Members.Add(user);
                TempData["status"] = "Ai intrat in grup";
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                TempData["status"] = "Nu faci parte din grup sau inexistent";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Delete | HttpVerbs.Post | HttpVerbs.Get)]
        public ActionResult Leave(FormCollection formData)
        {
            try
            {
                string currentUser = User.Identity.GetUserId();
                ApplicationUser user = db.Users.Find(currentUser);
                Group group = db.Groups.Find(Int32.Parse(formData.Get("GroupId")));
                if (group.UserId == currentUser)
                {
                    TempData["status"] = "Esti admin-ul grupului, nu poti iesi";
                    return RedirectToAction("Index");
                }
                if (!group.Members.Remove(user))
                {
                    TempData["status"] = "Nu faci parte din grup, nu poti iesi";
                    return RedirectToAction("Index");
                }
                db.SaveChanges();
                TempData["status"] = "Ai iesit din grup";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                TempData["status"] = "Nu exista grupul";
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

        private void SetAccessRights(int id_grup)
        {

            ViewBag.afisareButoane = false;
            if (User.IsInRole("RegisteredUser") || User.IsInRole("Admin"))
            {
                ViewBag.afisareButoane = true;
            }
            Group group = db.Groups.Find(id_grup);
            bool flag = false;
            foreach (var membru in group.Members)
                if (membru.Id == User.Identity.GetUserId())
                    flag = true;
            if (User.IsInRole("Admin"))
                flag = true;
            ViewBag.afisareEditor = flag;
            ViewBag.esteAdmin = User.IsInRole("Admin");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
        }


    }


}
