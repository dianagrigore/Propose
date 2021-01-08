using Microsoft.AspNet.Identity;
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
    public class GroupPostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Comments
        public ActionResult Index()
        {
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


        [HttpDelete]
        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Delete(int id)
        {
            GroupPost pos = db.GroupPosts.Find(id);
            Group group = db.Groups.Find(pos.GroupId);
            string authorEmail = pos.User.Email;
            if (pos.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.GroupPosts.Remove(pos);
                db.SaveChanges();

                if (User.IsInRole("Admin"))
                {
                    string notificationBody = "<p> Postarea din grupul: </p>";
                    notificationBody += "<p><strong>" + group.Name + "<strong></p>";
                    notificationBody += "<p> a fost stearsa de catre administratorul aplicatiei. </p><br />";
                    notificationBody += "Continutul postarii este: <br /> <br /><em>";
                    notificationBody += pos.Content;
                    notificationBody += "</em>";
                    notificationBody += "<br/><br/> O zi frumoasa!";
                    SendEmailNotification(authorEmail, "Postarea dvs. a fost stearsa", notificationBody);

                }

                return Redirect("/Groups/Show/" + pos.GroupId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Groups");
            }
        }


        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Edit(int id)
        {
            GroupPost pos = db.GroupPosts.Find(id);
            if (pos.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(pos);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Groups");
            }
        }

        [HttpPut]
        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Edit(int id, GroupPost requestGroupPost)
        {
            try
            {
                GroupPost pos = db.GroupPosts.Find(id);

                if (pos.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                {
                    if (TryUpdateModel(pos))
                    {
                        pos.Content = requestGroupPost.Content;
                        db.SaveChanges();
                    }
                    return Redirect("/Groups/Show/" + pos.GroupId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                    return RedirectToAction("Index", "Groups");
                }
            }
            catch (Exception e)
            {
                return View(requestGroupPost);
            }

        }
    }
}