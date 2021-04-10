using DoAnCN.Models;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace LoaiCN.Controllers
{
    public class LoaiController : Controller
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "NGybKn5jtzUYPIKMZzwUCe67H5y46J16XYUg40FR",
            BasePath = "https://doancn-fb163-default-rtdb.firebaseio.com/",
        };
        IFirebaseClient client;
        // GET: Food
        public ActionResult Index(string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = "";
            ViewBag.CurrentFilter = "";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            //if (!String.IsNullOrEmpty(searchString))
            //{

            //}
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Loais");
            
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Loai>();
            foreach(var item in data)
            {
                list.Add(JsonConvert.DeserializeObject<Loai>(((JProperty)item).Value.ToString()));
            }
            list.OrderByDescending(v => v.maloai);

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            IPagedList result = list.ToPagedList(pageNumber, pageSize);
            return View(result);
            
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Loai Loai)
        {
            try
            {
                addLoaiToFirebase(Loai);
                ModelState.AddModelError(string.Empty, "Added Successfully");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            
            return View();
        }

        private void addLoaiToFirebase(Loai loai)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = loai;
            PushResponse response = client.Push("Loais/", data);
            data.maloai = response.Result.name;
            SetResponse setResponse = client.Set("Loais/" + data.maloai, data);
        }

        public ActionResult MenuLoai()
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Loais");

            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Loai>();
            foreach (var item in data)
            {
                list.Add(JsonConvert.DeserializeObject<Loai>(((JProperty)item).Value.ToString()));
            }
            return PartialView(list);
        }

        [HttpGet]
        public ActionResult Detail(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Loais/"+id);
            Loai data = JsonConvert.DeserializeObject<Loai>(response.Body);
            return View(data);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Loais/" + id);
            Loai data = JsonConvert.DeserializeObject<Loai>(response.Body);
            return View(data);
        }

        [HttpPost]
        public ActionResult Edit(Loai loai)
        {
            client = new FireSharp.FirebaseClient(config);
            SetResponse response = client.Set("Loais/" + loai.maloai, loai);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Loais/" + id);
            return RedirectToAction("Index");
        }

    }
}