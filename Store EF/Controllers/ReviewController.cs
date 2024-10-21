using Store_EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class ReviewController : Controller
    {
        StoreEntities store = new StoreEntities();

        // GET: Review
        public ActionResult GetReviewsOneProd(int productId)
        {
            List<Review> reviews = store.Reviews.Where(r => r.ProductId == productId).ToList();
            ViewBag.ProductId = productId;
            return PartialView("GetReviewsOneProd", reviews);
        }

        [HttpPost]
        public ActionResult AddReview(Review review)
        {
            store.Reviews.Add(review);
            store.SaveChanges();
            return RedirectToAction("Detail", "Products", new { id = review.ProductId });
        }
    }
}