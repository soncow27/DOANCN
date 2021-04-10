using DoAnCN.Models;
using Firebase.Auth;
using Firebase.Storage;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DoAnCN.Controllers
{
    public class DoAnController : Controller
    {
        private static string ApiKey = "AIzaSyDA6iWd1pYuY2pEMVBU-r5Xaq1Pby3M0uU";
        private static string Bucket = "doancn-fb163.appspot.com";
        private static string AuthEmail = "dev.duyson27@gmail.com";
        private static string AuthPassword = "cowphamduyson27";
        IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "NGybKn5jtzUYPIKMZzwUCe67H5y46J16XYUg40FR",
            BasePath = "https://doancn-fb163-default-rtdb.firebaseio.com/",
        };
        IFirebaseClient client;
        // GET: DoAn
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
            FirebaseResponse response = client.Get("Doans");

            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Doan>();
            foreach (var item in data)
            {
                list.Add(JsonConvert.DeserializeObject<Doan>(((JProperty)item).Value.ToString()));
            }
            list.OrderByDescending(v => v.madoan);

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            IPagedList result = list.ToPagedList(pageNumber, pageSize);
            return View(result);
        }
        
        public ActionResult ChonLoai()
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
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Doan doan, HttpPostedFileBase file, string loai)
        {
            var urlhinh = "";
            try
            {
                FileStream stream;
                if (file.ContentLength > 0)
                {
                    string path = Path.Combine(Server.MapPath("~/Images/"), file.FileName);
                    file.SaveAs(path);
                    stream = new FileStream(Path.Combine(path), FileMode.Open);
                    await Task.Run(() => Upload(stream, file.FileName));
                    urlhinh = "gs://doancn-fb163.appspot.com/Images/" + file.FileName;
                    addDoAnToFirebase(doan, urlhinh, loai);
                    ModelState.AddModelError(string.Empty, "Added Successfully");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View();
        }

        private void addDoAnToFirebase(Doan doan, string urlhinh, string loai)
        {
            
            client = new FireSharp.FirebaseClient(config);
            var data = doan;
            PushResponse response = client.Push("Doans/", data);
            data.madoan = response.Result.name;
            doan.maloai = loai;
            data.anhdoanurl = urlhinh;
            SetResponse setResponse = client.Set("Doans/" + data.madoan, data);
        }


        public async void Upload(FileStream stream, string fileName)
        {
            var auth = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(ApiKey));
            var signin = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

            var cancellation = new CancellationTokenSource();

            var task = new FirebaseStorage(
                Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(signin.FirebaseToken),
                    ThrowOnCancel = true
                }
                ).Child("Images")
                .Child(fileName)
                .PutAsync(stream, cancellation.Token);

            try
            {
                string link = await task;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi", ex);
            }
        }

        [HttpGet]
        public ActionResult Detail(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Doans/" + id);
            Doan data = JsonConvert.DeserializeObject<Doan>(response.Body);
            return View(data);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Doans/" + id);
            Doan data = JsonConvert.DeserializeObject<Doan>(response.Body);
            return View(data);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Doan doan, HttpPostedFileBase file, string loai)
        {
            var urlhinh = "";
            try
            {
                FileStream stream;
                if (file != null)
                {
                    string path = Path.Combine(Server.MapPath("~/Images/"), file.FileName);
                    file.SaveAs(path);
                    stream = new FileStream(Path.Combine(path), FileMode.Open);
                    await Task.Run(() => Upload(stream, file.FileName));
                    urlhinh = "gs://doancn-fb163.appspot.com/Images/" + file.FileName;
                    client = new FireSharp.FirebaseClient(config);
                    doan.anhdoanurl = urlhinh;
                    doan.maloai = loai;
                    SetResponse response = client.Set("Doans/" + doan.madoan, doan);

                }
                else
                {
                    client = new FireSharp.FirebaseClient(config);
                    doan.maloai = loai;
                    SetResponse response = client.Set("Doans/" + doan.madoan, doan);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Doans/" + id);
            return RedirectToAction("Index");
        }
    }
}