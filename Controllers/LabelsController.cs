using Propose.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Propose.Controllers
{

    [Authorize(Roles = "Admin")]
    public class LabelsController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Category
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var labels = from label in db.Labels
                         orderby label.LabelName
                         select label;
            ViewBag.Labels = labels;
            return View();
        }

        public ActionResult Show(int id)
        {
            Label label = db.Labels.Find(id);
            return View(label);
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Label lab)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Labels.Add(lab);
                    db.SaveChanges();
                    TempData["message"] = "Eticheta a fost adaugata!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(lab);
                }
            }
            catch (Exception e)
            {
                return View(lab);
            }
        }

        public ActionResult Edit(int id)
        {
            Label label = db.Labels.Find(id);
            return View(label);
        }

        [HttpPut]
        public ActionResult Edit(int id, Label requestLabel)
        {
            try
            {
                Label label = db.Labels.Find(id);

                if (TryUpdateModel(label))
                {
                    label.LabelName = requestLabel.LabelName;
                    db.SaveChanges();
                    TempData["message"] = "Eticheta a fost modificata!";
                    return RedirectToAction("Index");
                }

                return View(requestLabel);
            }
            catch (Exception e)
            {
                return View(requestLabel);
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Label label = db.Labels.Find(id);
            db.Labels.Remove(label);
            TempData["message"] = "Eticheta a fost stearsa!";
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}