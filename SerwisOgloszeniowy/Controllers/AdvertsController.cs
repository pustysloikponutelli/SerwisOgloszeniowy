using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SerwisOgloszeniowy.Models;
using Microsoft.AspNet.Identity;
using System.IO;
using PagedList;
using Microsoft.AspNet.Identity.Owin;

namespace SerwisOgloszeniowy.Controllers
{
    public class AdvertsController : Controller
    {
        private OgloszeniaContext db = new OgloszeniaContext();

        
        public ActionResult Index(String sortOrder, AdvertSearch search, int? page)
        {
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            ViewBag.CurrentID = User.Identity.GetUserId();
            ViewBag.IdSortParm = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.TitleSortParm = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            var adverts = db.Adverts.Include(i => i.ImagePaths);
            switch (sortOrder)
            {
                case "title_desc": adverts = adverts.OrderByDescending(s => s.Title); break;
                case "title_asc": adverts = adverts.OrderBy(s => s.Title); break;
                case "id_desc": adverts = adverts.OrderByDescending(s => s.Id); break;
                default: adverts = adverts.OrderBy(s => s.Id); break;
            }
            
            if (search != null)
            {
                if (search.DescriptionSearch == null)
                    search.DescriptionSearch = "";
                if (search.TitleSearch == null)
                    search.TitleSearch = "";
                adverts = adverts.Where(
                        x => x.Title.Contains(search.TitleSearch) && 
                        x.Description.Contains(search.DescriptionSearch)
                        );
            }

            return View(adverts.ToPagedList(pageNumber, pageSize));
        }
        // GET: Adverts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Advert advert = db.Adverts.Include(i => i.ImagePaths).SingleOrDefault(i => i.Id == id);

            var users = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.ToList();
            var user = users.FirstOrDefault(x => x.Id == advert.UserID);
            ViewBag.Advertiser = user.Name;
                if (advert == null)
            {
                return HttpNotFound();
            }
            return View(advert);
        }

        // GET: Adverts/Create
        [Authorize(Roles = "Admin,User")]
        public ActionResult Create()

        {
            return View();
        }

        // POST: Adverts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,UserID,Images")] Advert advert)
        {

            
            advert.UserID = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                List<ImagePath> imagePaths = new List<ImagePath>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        ImagePath imagePath = new ImagePath()
                        {
                            FileName = fileName,
                            Extension = Path.GetExtension(fileName),
                            Id = Guid.NewGuid()
                        };
                        imagePaths.Add(imagePath);

                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), imagePath.Id + imagePath.Extension);
                        file.SaveAs(path);
                    }
                }

                advert.ImagePaths = imagePaths;
                db.Adverts.Add(advert);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(advert);
        }

        // GET: Adverts/Edit/5
        [Authorize(Roles = "Admin,User")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Advert advert = db.Adverts.Include(s => s.ImagePaths).SingleOrDefault(x => x.Id == id);
            if (advert == null)
            {
                return HttpNotFound();
            }
            return View(advert);
        }

        // POST: Adverts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( Advert advert)
        {
            if (ModelState.IsValid) { 

                List<ImagePath> imagePaths;
                if (advert.ImagePaths != null)
                    imagePaths = advert.ImagePaths.ToList();
                else
                    imagePaths = new List<ImagePath>();

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        ImagePath fileDetail = new ImagePath()
                        {
                            FileName = fileName,
                            Extension = Path.GetExtension(fileName),
                            Id = Guid.NewGuid(),
                            AdvertID = advert.Id.ToString()
                        };
                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), fileDetail.Id + fileDetail.Extension);
                        file.SaveAs(path);
                        //db.ImagePaths.Add(fileDetail);
                        imagePaths.Add(fileDetail);
                        db.Entry(fileDetail).State = EntityState.Added;
                    }
                }

                advert.ImagePaths = imagePaths;

                db.Entry(advert).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(advert);
        }

        // GET: Adverts/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Advert advert = db.Adverts.Find(id);
            if (advert == null)
            {
                return HttpNotFound();
            }
            return View(advert);
        }

        // POST: Adverts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Advert advert = db.Adverts.Find(id);

            foreach (var item in advert.ImagePaths)
            {
                String path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), item.Id + item.Extension);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            db.Adverts.Remove(advert);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult DeleteFile(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                Guid guid = new Guid(id);
                ImagePath fileDetail = db.ImagePaths.Find(guid);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }
                
                db.ImagePaths.Remove(fileDetail);
                db.SaveChanges();
                
                var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), fileDetail.Id + fileDetail.Extension);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
        public FileResult Download(String p, String d)
        {
            return File(Path.Combine(Server.MapPath("~/App_Data/Upload/"), p), System.Net.Mime.MediaTypeNames.Application.Octet, d);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
