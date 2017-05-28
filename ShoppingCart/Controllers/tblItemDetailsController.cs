using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShoppingCart;
using Microsoft.AspNet.Identity;
using System.Web.SessionState;

namespace ShoppingCart.Controllers
{
    public class tblItemDetailsController : Controller
    {
        private OnlineShoppingDBEntities db = new OnlineShoppingDBEntities();

        // GET: tblItemDetails
        public ActionResult Index()
        {

            if (CheckUserAuthorize() == true)    // checking user is authorized or not 
            {
                if (CheckIfAdmin() == true)     // checking user is admin or not
                {
                    var tblItemDetail = db.tblItemDetails.Include(t => t.AspNetUser);
                    return View(tblItemDetail);
                }
                else
                {
                    var strUserId = User.Identity.GetUserId();
                    // var tblMovieDetails = db.tblMovieDetails.Include(t => t.AspNetUser);
                    var tblItemDetail = db.tblItemDetails.Where(t => t.UserID == strUserId);
                    
                    return View(tblItemDetail.ToList());
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            //var tblItemDetails = db.tblItemDetails.Include(t => t.AspNetUser);
            //return View(tblItemDetails.ToList());
        }

        // GET: tblItemDetails/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblItemDetail tblItemDetail = db.tblItemDetails.Find(id);
            if (tblItemDetail == null)
            {
                return HttpNotFound();
            }
            return View(tblItemDetail);
        }

        // GET: tblItemDetails/Create
        public ActionResult Create()
        {
            if (CheckUserAuthorize() == true)
            {
                ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email");
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: tblItemDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserID,ItemName,Description,Quantity,Price")] tblItemDetail tblItemDetail)
        {
            if (CheckUserAuthorize() == true)
            {
                tblItemDetail.UserID = User.Identity.GetUserId();
                if (ModelState.IsValid)
                {
                    db.tblItemDetails.Add(tblItemDetail);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", tblItemDetail.UserID);
                return View(tblItemDetail);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // GET: tblItemDetails/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblItemDetail tblItemDetail = db.tblItemDetails.Find(id);
            if (tblItemDetail == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email", tblItemDetail.UserID);
            return View(tblItemDetail);
        }

        // POST: tblItemDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserID,ItemName,Description,Quantity,Price")] tblItemDetail tblItemDetail)
        {
            if (CheckUserAuthorize() == true)
            {
                tblItemDetail.UserID = User.Identity.GetUserId();
                if (ModelState.IsValid)
                {
                    db.Entry(tblItemDetail).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", tblItemDetail.UserID);
                return View(tblItemDetail);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // GET: tblItemDetails/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblItemDetail tblItemDetail = db.tblItemDetails.Find(id);
            if (tblItemDetail == null)
            {
                return HttpNotFound();
            }
            return View(tblItemDetail);
        }

        // POST: tblItemDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tblItemDetail tblItemDetail = db.tblItemDetails.Find(id);
            db.tblItemDetails.Remove(tblItemDetail);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        [Authorize]
        public ActionResult Search(string strUserSearch)
        {

            var strUserId = User.Identity.GetUserId();

            if (strUserSearch != "" && strUserSearch != null)
            {
                Session["SearchString"] = strUserSearch;
                var objtblItemDetails = db.tblItemDetails.Where(t => (t.UserID != strUserId) && ( t.ItemName.Contains(strUserSearch) || t.Description.Contains(strUserSearch) ));
                
                if (objtblItemDetails == null)
                {
                    return HttpNotFound(); // show error message that there is no data which matach your search. 
                }
                return View(objtblItemDetails);
            }
            else
            {
                
                var tblItemDetails = db.tblItemDetails.Where(t => t.UserID != strUserId);
                return View(tblItemDetails.ToList());

            }
        }

        public ActionResult _ViewItemPartial(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblItemDetail tblItemDetail1 = db.tblItemDetails.Find(id);
            if (tblItemDetail1 == null)
            {
                return HttpNotFound();
            }
            return PartialView("_ViewItemPartial", tblItemDetail1);
        }

        public ActionResult AddToCart(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int itemId = (int)id;
            tblItemDetail tblItemDetail1 = db.tblItemDetails.Find(id);
            
            if (tblItemDetail1 == null)
            {
                return HttpNotFound();
            }
            tblCartItem objCartItem = new tblCartItem();
            objCartItem.UserId = User.Identity.GetUserId();
            objCartItem.ItemId = itemId;
            
            if(ModelState.IsValid)
            {
                db.tblCartItems.Add(objCartItem);
                db.SaveChanges();
            }

            //Session["CartItems"] = tblItemDetail1;
            //tblItemDetail sessionitemdetails = (tblItemDetail)Session["CartItems"];
            if (Session["SearchString"] != null)
            {
                // return RedirectToAction("Search?strUserSearch="+ Session["SearchString"]);
                return RedirectToAction("Search", "tblItemDetails", new { strUserSearch = Session["SearchString"] });
            }
            else
            {
                return RedirectToAction("Search");
            }

        }

        public ActionResult CartView()
        {
            var strUserId = User.Identity.GetUserId();

            var query = from cart in db.tblCartItems
                        join item in db.tblItemDetails on cart.ItemId equals item.Id
                        where cart.UserId == strUserId
                        select item;

            return View(query.ToList());
         }

        
        public ActionResult DeleteCartItem(int id)
        {
            var strUserId = User.Identity.GetUserId();

            var query = (from cartItem in db.tblCartItems where cartItem.ItemId == id && cartItem.UserId == strUserId
                         select cartItem).FirstOrDefault();
            db.tblCartItems.Remove(query);
            //db.tblItemDetails.Remove(tblItemDetail);
            db.SaveChanges();
            return RedirectToAction("CartView");
        }


        public ActionResult ItemBuy(int id)
        {
            var strUserId = User.Identity.GetUserId();

            var query = (from cartItem in db.tblCartItems
                         where cartItem.ItemId == id && cartItem.UserId == strUserId
                         select cartItem).FirstOrDefault();
            tblTotalMoneySpent objtblTolalMoneySpent = new tblTotalMoneySpent();

            objtblTolalMoneySpent.Buyer_UserID = strUserId;
            objtblTolalMoneySpent.Seller_UserID = query.tblItemDetail.UserID;
            objtblTolalMoneySpent.Total_Spent = query.tblItemDetail.Price;

            if(ModelState.IsValid)
            { 
                db.tblTotalMoneySpents.Add(objtblTolalMoneySpent);    // Adding data into total money spent table...
                db.SaveChanges();

                var quantity = query.tblItemDetail.Quantity;
                quantity = quantity - 1;
                query.tblItemDetail.Quantity = quantity;
                db.Entry(query.tblItemDetail).State = EntityState.Modified;   // decresing quantity from item details table..
                //db.SaveChanges();

                db.tblCartItems.Remove(query); // Remove from item from Cart...
                db.SaveChanges();
                             

            }

            return RedirectToAction("CartView");

        }
            public Boolean CheckUserAuthorize()
        {
            var strUserID = User.Identity.GetUserId();
            var strUserName = User.Identity.GetUserName();
            if (strUserID != null && strUserName != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Boolean CheckIfAdmin()
        {
            if (User.Identity.GetUserName() == "admin@admin.com")
            {
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}
