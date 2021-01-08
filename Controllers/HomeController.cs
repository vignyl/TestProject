using Stripe;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using TestProject.Models;
using TestProject.Models.FirstModel;
using ZXing;

namespace TestProject.Controllers
{
    public class HomeController : Controller
    {
        protected Model1 db1 = new Model1();
        protected TestProject.Models.SecondModel.Model1 db2 = new Models.SecondModel.Model1();
        public ActionResult Index()
        {
            ViewBag.stripePublishableKey = WebConfigurationManager.AppSettings["StripePublishableKey"];

            return View();
        }
         
        public ActionResult QR_Login()
        {
              
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QR_Login(QRCodeModel model)
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                { 
                    var filename = Path.GetFileName(file.FileName);
                    String[] IMAGES_EXTENSION = new[] { ".png", ".jpg", ".jpeg", ".gif" }; 
                    if (!IMAGES_EXTENSION.Contains(Path.GetExtension(filename)))
                    {
                        TempData["result_code"] = -1;
                        TempData["message"] = "Extension file not allowed";
                        TempData.Keep();
                        return View(model);
                    }
                    var formattedFileName = string.Format("{0}-{1}{2}"
                        , Path.GetFileNameWithoutExtension(filename)
                        , Guid.NewGuid().ToString("N")
                        , Path.GetExtension(filename));
                    var path = Path.Combine(Server.MapPath(QRCodeModel.SAVE_PATH), formattedFileName);
                    file.SaveAs(path);
                 
                    var readCode = ReadQRCode(formattedFileName);

                    var user = db2.User.FirstOrDefault(x => x.code == readCode.QRCodeText);
                    if (user != null)
                    {
                        FormsAuthentication.SetAuthCookie(user.id.ToString(), true);
                        Session.Clear();
                        User u = new User();
                        u.id = user.id;
                        u.full_name = user.full_name;
                        u.email = user.code;
                        u.password = user.password;
                        u.status = user.status;
                        u.isSecond = true;
                        u.reduction = user.reduction;
                        Session.Add("me", u);
                        return RedirectToAction("Index", "Home");
                    }
                    TempData["result_code"] = -1;
                    TempData["message"] = "Account not found. Please choose correct QR Code";
                    TempData.Keep();
                    return View();
                }
            }
            TempData["result_code"] = -1;
            TempData["message"] = "QR code not found, choose a QR code file";
            TempData.Keep(); 
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User item)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var user = db1.User.FirstOrDefault(x => x.email == item.email && x.password == item.password);
                    if (user != null)
                    {
                        FormsAuthentication.SetAuthCookie(user.id.ToString(), true);
                        Session.Clear();
                        Session.Add("me", user);
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception e)
                {
                    TempData["result_code"] = -1;
                    TempData["message"] = "An error occured";
                    TempData.Keep();
                    ModelState.AddModelError("Exception", e.Message);
                    return View(item);
                }
            }
            TempData["result_code"] = -1;
            TempData["message"] = "Account not found. Please create an account";
            TempData.Keep();
            ModelState.AddModelError("Exception", "Account not found. Please create an account");
            return View(item);
        }
        public ActionResult Register()
        {  
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User item)
        { 
            if (ModelState.IsValid)
            {
                try
                {
                    if (db1.User.Any(u => u.email == item.email))
                    {
                        ModelState.AddModelError("Email", "Email address already exists. Please change email address.");
                        return View(item);
                    }
                    var qRCodeModel = GenerateQRCode(item.email);

                    WebClient myWebClient = new WebClient(); 
                    try
                    {
                        string pdfPath = Server.MapPath(qRCodeModel.QRCodeImagePath);
                        Response.ContentType = "iamges/jpeg";
                        Response.AppendHeader("content-disposition", "attachment; filename=" + item.email+".jpg");
                        Response.TransmitFile(pdfPath);
                        Response.End();
                        //myWebClient.DownloadFile(Server.MapPath(qRCodeModel.QRCodeImagePath), Server.MapPath(qRCodeModel.QRCodeImagePath));

                    }
                    catch (Exception e)
                    {
                        TempData["result_code"] = -1;
                        TempData["message"] = "An error occured";
                        TempData.Keep();
                        ModelState.AddModelError("Exception", "QR code can't downloaded. Updade your browser before.");
                        return View(item);
                    }

                    db1.User.Add(item);
                    db1.SaveChanges();

                    Models.SecondModel.User user2 = new Models.SecondModel.User();
                    user2.full_name = item.full_name;
                    user2.status = true;
                    user2.password = item.password; 
                    user2.code = qRCodeModel.QRCodeText;
                    user2.reduction = Convert.ToDecimal(QRCodeModel.randomDouble(0, 1));
                    db2.User.Add(user2);
                    db2.SaveChanges();


                    TempData["result_code"] = 1;
                    TempData["message"] = "Account created successfully";
                    TempData.Keep(); 
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception e)
                {
                    TempData["result_code"] = -1;
                    TempData["message"] = "An error occured";
                    TempData.Keep();
                    ModelState.AddModelError("Exception", e.Message);
                    return View(item);
                }
            }
            TempData["result_code"] = -1;
            TempData["message"] = "Datas not valid";
            TempData.Keep();
            ModelState.AddModelError("Exception", "Datas not valid");
            return View(item);
        }
         
        public ActionResult Pay()
        {
            if (me == null) return RedirectToAction("Index");
            return View();
        }

        [HttpPost] 
        public ActionResult Pay(Payment item)
        {
            var me = (User)Session["me"];
            if (me == null) return RedirectToAction("Index");
            var stripeToken = Request.Form["stripeToken"];
            if (String.IsNullOrEmpty(stripeToken))
            { 
                TempData["result_code"] = -1;
                TempData["message"] = "Stripe set an error with your informations";
                TempData.Keep();
                return RedirectToAction("Index");
            }
            var Email = Request.Form["stripeEmail"];
            var stripeTokenType = Request.Form["stripeTokenType"];   
            var productService = new ProductService();
            var priceService = new PriceService();
            var invoiceItemService = new InvoiceItemService();
            var invoiceServices = new InvoiceService();
            var customerService = new CustomerService();
            var planService = new PlanService();
            var subscriptionService = new SubscriptionService();

            var original_amount = 500;
            var amount = Convert.ToInt32(me.reduction > 0 ? original_amount * me.reduction : original_amount);

            var product = productService.Create(new ProductCreateOptions
            {
                Name = "Name of Service", 
            });
            
            var price = priceService.Create(new PriceCreateOptions
            {
                Product = product.Id,
                UnitAmount = amount,
                Currency = "usd",
                Recurring = new PriceRecurringOptions
                {
                    Interval = "month",
                },
            });

            var customer = customerService.Create(new CustomerCreateOptions
            {
                Email = Email,
                Source = stripeToken,
            });
              


            var plan = planService.Create(new PlanCreateOptions
            { 
                Amount = amount,
                Currency = "usd",
                Interval = "month",
                IntervalCount = 1,
                Product = product.Id, // "prod_IinH4BV2oyao8L",  
            });
            var subscription = subscriptionService.Create(new SubscriptionCreateOptions
            {  
                Customer = customer.Id,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    { 
                        Plan = plan.Id,
                    },
                }, 
            });
             
            if(subscription.Status == "active")
            {
                item.original_amount = 500;
                item.amount = amount;
                item.code = QRCodeModel.GenerateRandomString();
                item.payer_id = me.id;
                db1.Payment.Add(item);
                db1.SaveChanges();
                TempData["result_code"] = 1;
                TempData["message"] = "Subscription done successfully";
                TempData.Keep();
                return RedirectToAction("Payments");
            } 
            TempData["result_code"] = -1;
            TempData["message"] = "An error occured during the payment";
            TempData.Keep();
            return RedirectToAction("Index");  
        }

        [Authorize]
        public ActionResult Payments()
        {
            if(me == null) return RedirectToAction("Index");
            var list = db1.Payment.ToList();
            return View(list); 
        }

        // GET: Roles/Enable/5 
        [Authorize]
        public ActionResult Enable()
        {
            if (me == null) return RedirectToAction("Index");
            var me = (User)Session["me"]; 
            if (me == null) return RedirectToAction("Index");
            me.status = !me.status;
            TestProject.Models.SecondModel.User u2 = db2.User.FirstOrDefault(x => x.code == me.email);
            u2.status = !u2.status; 
            db2.Entry(u2).State = EntityState.Modified;
            db2.SaveChanges();
            TempData["result_code"] = 1;
            TempData["message"] = "Status changed successfully";
            TempData.Keep(); 
            Session.Add("me", me); 
            return RedirectToAction("Index");
        }


        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear(); 
            return RedirectToAction("Index", "Home");
        }
        private QRCodeModel GenerateQRCode(string qrcodeText)
        {
            string folderPath = QRCodeModel.SAVE_PATH;
            string imagePath = folderPath + qrcodeText + ".jpg";
            // If the directory doesn't exist then create it.
            if (!Directory.Exists(Server.MapPath(folderPath)))
            {
                Directory.CreateDirectory(Server.MapPath(folderPath));
            }

            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            var result = barcodeWriter.Write(qrcodeText);

            string barcodePath = Server.MapPath(imagePath);
            var barcodeBitmap = new Bitmap(result);
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(barcodePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    barcodeBitmap.Save(memory, ImageFormat.Jpeg);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();
                    fs.Dispose();
                }
            }
            QRCodeModel qRCodeModel = new QRCodeModel();
            qRCodeModel.QRCodeText = qrcodeText;
            qRCodeModel.QRCodeImagePath = imagePath; 
            return qRCodeModel;
        }


        private QRCodeModel ReadQRCode(string imagePath)
        {
            QRCodeModel barcodeModel = new QRCodeModel();
            string barcodeText = "";
            //string imagePath = "~/Images/QrCode.jpg";
            string barcodePath = Server.MapPath(QRCodeModel.SAVE_PATH + imagePath);
            var barcodeReader = new BarcodeReader();

            var result = barcodeReader.Decode(new Bitmap(barcodePath));
            if (result != null)
            {
                barcodeText = result.Text;
            }
            return new QRCodeModel() { QRCodeText = barcodeText, QRCodeImagePath = imagePath };
        }

    }
}