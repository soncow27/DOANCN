using DoAnCN.Common;
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

namespace DoAnCN.Controllers
{
    public class NguoiDungController : Controller
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "NGybKn5jtzUYPIKMZzwUCe67H5y46J16XYUg40FR",
            BasePath = "https://doancn-fb163-default-rtdb.firebaseio.com/",
        };
        IFirebaseClient client;
        // GET: NguoiDung
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
            FirebaseResponse response = client.Get("Nguoidungs");

            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Nguoidung>();
            foreach (var item in data)
            {
                list.Add(JsonConvert.DeserializeObject<Nguoidung>(((JProperty)item).Value.ToString()));
            }
            list.OrderByDescending(v => v.makh);

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
        public ActionResult Create(Nguoidung nguoidung)
        {
            try
            {
                addNguoiDungToFirebase(nguoidung);
                ModelState.AddModelError(string.Empty, "Added Successfully");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View();
        }

        private void addNguoiDungToFirebase(Nguoidung nguoidung)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = nguoidung;
            PushResponse response = client.Push("Nguoidungs/", data);
            data.makh = response.Result.name;
            SetResponse setResponse = client.Set("Nguoidungs/" + data.makh, data);
        }

        [HttpGet]
        public ActionResult Detail(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Nguoidungs/" + id);
            Nguoidung data = JsonConvert.DeserializeObject<Nguoidung>(response.Body);
            return View(data);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Nguoidungs/" + id);
            Nguoidung data = JsonConvert.DeserializeObject<Nguoidung>(response.Body);
            return View(data);
        }

        [HttpPost]
        public ActionResult Edit(Nguoidung nguoidung)
        {
            client = new FireSharp.FirebaseClient(config);
            SetResponse response = client.Set("Nguoidungs/" + nguoidung.makh, nguoidung);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Nguoidungs/" + id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangKy(Nguoidung nguoidung)
        {
            try
            {
                addNguoiDungToFirebase(nguoidung);
                ModelState.AddModelError(string.Empty, "Đăng ký thành công !");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DangNhap()
        {
            if (Session["USER_SESSION"] != null) return RedirectToAction("Index", new { controller = "Home" });
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(LoginModel nguoidung)
        {
            try
            {
                client = new FireSharp.FirebaseClient(config);
                FirebaseResponse response = client.Get("Nguoidungs");

                dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                var list = new List<Nguoidung>();
                foreach (var item in data)
                {
                    list.Add(JsonConvert.DeserializeObject<Nguoidung>(((JProperty)item).Value.ToString()));
                }
                foreach (var account in list)
                {
                    if (nguoidung.Username == account.taikhoan && nguoidung.Password == account.matkhau)
                    {
                        var userSession = new UserLogin();
                        userSession.UserName = account.taikhoan;
                        userSession.UserID = account.makh;
                        Session.Add(CommonConstants.USER_SESSION, userSession);
                        ModelState.AddModelError(string.Empty, "Đăng ký thành công !");
                        //if (data.taikhoan.Contains("admin")) return RedirectToAction("Index", "Admin");
                        return RedirectToAction("Index", "Home");
                    }
                   
                }
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return RedirectToAction("Index");
        }
        
        public ActionResult LoginStatus()
        {
            ViewBag.Status = 0;
            ViewBag.Ten = "Quý khách chưa đăng nhập";
            ViewBag.Anh = "";
            var user = (UserLogin)Session[CommonConstants.USER_SESSION];
            if (user != null)
            {
                ViewBag.Status = 1;
                ViewBag.Ten = user.UserName;
            }
            return PartialView();
        }
        public ActionResult DangXuat()
        {
            Session[CommonConstants.USER_SESSION] = null;
            return Redirect("/");
        }
    }
}