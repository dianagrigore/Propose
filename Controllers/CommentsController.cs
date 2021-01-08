using System;
using Propose.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Text;

namespace Propose.Controllers
{
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Comments
        public ActionResult Index()
        {
            return View();
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

        [HttpDelete]
        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);
            Post post = db.Posts.Find(comm.PostId);
            string authorEmail = comm.User.Email;
            if (comm.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();

                if (User.IsInRole("Admin"))
                {
                    string notificationBody = "<p> Comentariul dvs. la postarea cu titlul: </p>";
                    notificationBody += "<p><strong>" + post.Title + "<strong></p>";
                    notificationBody += "<p> a fost sters de catre administratorul aplicatiei. </p><br />";
                    notificationBody += "Comentariul sters este: <br /> <br /><em>";
                    notificationBody += comm.Content;
                    notificationBody += "</em>";
                    notificationBody += "<br/><br/> O zi frumoasa!";
                    SendEmailNotification(authorEmail, "Comentariul dvs. a fost sters ", notificationBody);
                }


                return Redirect("/Posts/Show/" + comm.PostId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Posts");
            }
        }

       
        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);
            if (comm.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(comm);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Posts");
            }
        }

        [HttpPut]
        [Authorize(Roles = "RegisteredUser,Admin")]
        public ActionResult Edit(int id, Comment requestComment)
        {
            try
            {
                Comment comm = db.Comments.Find(id);

                if (comm.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                {
                    if (TryUpdateModel(comm))
                    {
                        comm.Content = requestComment.Content;
                        db.SaveChanges();
                    }
                    return Redirect("/Posts/Show/" + comm.PostId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                    return RedirectToAction("Index", "Posts");
                }
            }
            catch (Exception e)
            {
                return View(requestComment);
            }

        }
    }
}
