﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult OrderHistory()
        {
            return View();
        }
    }
}