using DoAnCN.Models;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnCN.Controllers
{
    public class HomeController : Controller
    {
        IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "NGybKn5jtzUYPIKMZzwUCe67H5y46J16XYUg40FR",
            BasePath = "https://doancn-fb163-default-rtdb.firebaseio.com/",
        };
        IFirebaseClient client;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Featured()
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Doans");

            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Doan>();
            foreach (var item in data)
            {
                list.Add(JsonConvert.DeserializeObject<Doan>(((JProperty)item).Value.ToString()));
            }


            
            return PartialView(list);
        }

        public ActionResult FeaturedMenu()
        {
            var client2 = new FireSharp.FirebaseClient(config);
            FirebaseResponse response2 = client2.Get("Loais");

            dynamic data2 = JsonConvert.DeserializeObject<dynamic>(response2.Body);
            var listloai = new List<Loai>();
            foreach (var item in data2)
            {
                listloai.Add(JsonConvert.DeserializeObject<Loai>(((JProperty)item).Value.ToString()));
            }
            return PartialView(listloai);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}