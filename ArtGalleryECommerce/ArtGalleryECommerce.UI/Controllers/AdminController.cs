﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using ArtGalleryECommerce.Model.AdminDTO;
using ArtGalleryECommerce.Dal.Repository;
using ArtGalleryECommerce.Dal.Admin;
using ArtGalleryECommerce.UI.Models;
using ArtGalleryECommerce.UI.CustomFilter;
using System.Web.Security;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;


namespace ArtGalleryECommerce.UI.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(AdminLoginModel adminLoginModel)
        {
            if (!ModelState.IsValid)
            {
                return View(adminLoginModel);
            }
            AdminLoginDto admin = new AdminLoginDto() { UserName = adminLoginModel.UserName, Password = adminLoginModel.Password };
            admin = AdminAuthenticationRepository.GetAdminDetails(admin);
            if (admin.AdminId > 0)
            {
                FormsAuthentication.SetAuthCookie(adminLoginModel.UserName, false);
                var authTicket = new FormsAuthenticationTicket(1, admin.UserName, DateTime.Now, DateTime.Now.AddMinutes(20), false, admin.UserRole);
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                HttpContext.Response.Cookies.Add(authCookie);
                TempData["AdminDetails"] = admin;
                TempData.Keep();
                Session["AdminId"] = admin.AdminId;
                return RedirectToAction("DashBoard", "Admin");
            }

            else
            {
                ViewBag.LoginError = "Invalid Login Attempt";
                return View();
            }
        }
        [Authorize(Roles = "Admin")]
        [UserAuthenticationFilter]
        public ActionResult DashBoard()
        {
            ViewBag.AdminDetails = TempData["AdminDetails"];
            TempData.Keep();
            return View();
        }
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Admin");
        }
        [Authorize(Roles = "Admin")]
        [UserAuthenticationFilter]
        public ActionResult ItemGroup()
        {
            ViewBag.AdminDetails = TempData["AdminDetails"];
            TempData.Keep();
            return View();
        }
        [HttpPost]
        public ActionResult GetAllItemGroup()
        {
            ItemGroupDal itemGroupDal = new ItemGroupDal();
            List<ItemGroupDto> lstItemGroupDto = itemGroupDal.GetAndEditItemGroup(0, 1);
            return Json(lstItemGroupDto, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveItemGroup()
        {
            try
            {
                ItemGroupDal itemGroupDal = new ItemGroupDal();
                ItemGroupDto itemGroupDto = new ItemGroupDto();
                itemGroupDto.GroupId = Convert.ToInt32(System.Web.HttpContext.Current.Request["GroupId"] == "" ? "0" : System.Web.HttpContext.Current.Request["GroupId"]);
                string Message, fileName, actualFileName;
                Message = fileName = actualFileName = string.Empty;
                if (Request.Files.Count > 0)
                {
                    var fileContent = Request.Files[0];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        actualFileName = fileContent.FileName;
                        fileName = Guid.NewGuid() + Path.GetExtension(fileContent.FileName);
                        itemGroupDto.GroupImage = fileName;
                    }
                    fileContent.SaveAs(Path.Combine(Server.MapPath("~/UploadImages/"), fileName));
                }
                else
                {
                    fileName = "";
                    if (itemGroupDto.GroupId > 0)
                    {
                        dynamic imgFile = itemGroupDal.GetAndEditItemGroup(itemGroupDto.GroupId, 1);
                        itemGroupDto.GroupImage = Convert.ToString(imgFile[0].GroupImage);
                    }
                    else
                    {
                        itemGroupDto.GroupImage = fileName;
                    }
                }
                itemGroupDto.GroupName = System.Web.HttpContext.Current.Request["GroupName"];
                itemGroupDto.GroupDesc = System.Web.HttpContext.Current.Request["GroupDesc"];
                itemGroupDto.CreatedBy = Convert.ToInt32(Session["AdminId"]);
                itemGroupDto.ModifiedBy = Convert.ToInt32(Session["AdminId"]);
                itemGroupDto.IsActive = 1;

                int i = itemGroupDal.SaveItemGroup(itemGroupDto);
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult DeleteItemGroup(int GroupId)
        {
            ItemGroupDal itemGroupDal = new ItemGroupDal();
            int i = itemGroupDal.DeleteItemGroup(GroupId);
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        [UserAuthenticationFilter]
        public ActionResult ItemCategory()
        {
            ViewBag.AdminDetails = TempData["AdminDetails"];
            TempData.Keep();
            return View();
        }
        [HttpPost]
        public ActionResult GetAllItemCategory()
        {
            ItemCategoryDal itemCategoryDal = new ItemCategoryDal();
            List<ItemCategoryDto> lstItemCategoryDto = itemCategoryDal.GetAndEditItemCategory(0, 1);
            return Json(lstItemCategoryDto, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetItemCategoryByGroupId(int GroupId)
        {
            ItemCategoryDal itemCategoryDal = new ItemCategoryDal();
            List<ItemCategoryDto> lstItemCategoryDto = itemCategoryDal.GetItemCategoryByGroupId(GroupId);
            return Json(lstItemCategoryDto, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveItemCategory()
        {
            try
            {
                ItemCategoryDal itemCategoryDal = new ItemCategoryDal();
                ItemCategoryDto itemCategoryDto = new ItemCategoryDto();
                itemCategoryDto.CategoryId = Convert.ToInt32(System.Web.HttpContext.Current.Request["CategoryId"] == "" ? "0" : System.Web.HttpContext.Current.Request["CategoryId"]);
                string Message, fileName, actualFileName;
                Message = fileName = actualFileName = string.Empty;
                if (Request.Files.Count > 0)
                {
                    var fileContent = Request.Files[0];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        actualFileName = fileContent.FileName;
                        fileName = Guid.NewGuid() + Path.GetExtension(fileContent.FileName);
                        itemCategoryDto.CategoryImage = fileName;
                    }
                    fileContent.SaveAs(Path.Combine(Server.MapPath("~/UploadImages/"), fileName));
                }
                else
                {
                    fileName = "";
                    if (itemCategoryDto.CategoryId > 0)
                    {
                        dynamic imgFile = itemCategoryDal.GetAndEditItemCategory(itemCategoryDto.CategoryId, 1);
                        itemCategoryDto.CategoryImage = Convert.ToString(imgFile[0].CategoryImage);
                    }
                    else
                    {
                        itemCategoryDto.CategoryImage = fileName;
                    }
                }
                itemCategoryDto.GroupId = Convert.ToInt32(System.Web.HttpContext.Current.Request["GroupId"]);
                itemCategoryDto.CategoryName = System.Web.HttpContext.Current.Request["CategoryName"];
                itemCategoryDto.CategoryDesc = System.Web.HttpContext.Current.Request["CategoryDesc"];
                itemCategoryDto.CreatedBy = Convert.ToInt32(Session["AdminId"]);
                itemCategoryDto.ModifiedBy = Convert.ToInt32(Session["AdminId"]);
                itemCategoryDto.IsActive = 1;

                int i = itemCategoryDal.SaveItemCategory(itemCategoryDto);
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult DeleteItemCategory(int CategoryId)
        {
            ItemCategoryDal itemCategoryDal = new ItemCategoryDal();
            int i = itemCategoryDal.DeleteItemCategory(CategoryId);
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        [UserAuthenticationFilter]
        public ActionResult ItemMaster()
        {
            ViewBag.AdminDetails = TempData["AdminDetails"];
            TempData.Keep();
            return View();
        }
        [HttpPost]
        public ActionResult GetAllItemMaster()
        {
            ItemMasterDal itemMasterDal = new ItemMasterDal();
            List<ItemMasterDto> lstItemMasterDto = itemMasterDal.GetAndEditItemMaster(0, 1);
            return Json(lstItemMasterDto, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveItemMaster2()
        {
            string Message, fileName, actualFileName;
            Message = fileName = actualFileName = string.Empty;
            if (Request.Files.Count > 0)
            {
                var fileContent = Request.Files[0];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    actualFileName = fileContent.FileName;
                    fileName = Guid.NewGuid() + Path.GetExtension(fileContent.FileName);
                    using (Bitmap bmp = new Bitmap(fileContent.InputStream, false))
                    {
                        using (Graphics grp = Graphics.FromImage(bmp))
                        {
                            string filePath = Server.MapPath(Url.Content("~/fonts/watermark.png"));
                            Image logoImage = Image.FromFile(filePath);
                            Image TargetImg = Image.FromStream(fileContent.InputStream);
                            RectangleF rectf = grp.VisibleClipBounds;
                            var destX = (TargetImg.Width - logoImage.Width) - 30;
                            var destY = (TargetImg.Height - logoImage.Height) - 30;
                            float cxImage = grp.DpiX * logoImage.Width /
                                            logoImage.HorizontalResolution;
                            float cyImage = grp.DpiY * logoImage.Height /
                                                               logoImage.VerticalResolution;
                            grp.DrawImage(logoImage, (rectf.Width - cxImage) / 2,
                                (rectf.Height - cyImage) / 2);
                            // grp.DrawImage(logoImage, new Point(TargetImg.Width - logoImage.Width - 10, 10));
                            bmp.Save(Path.Combine(Server.MapPath("~/UploadImages/"), fileName));
                        }
                    }
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveItemMaster()
        {
            try
            {
                ItemMasterDal itemMasterDal = new ItemMasterDal();
                ItemMasterDto itemMasterDto = new ItemMasterDto();
                itemMasterDto.ItemId = Convert.ToInt32(System.Web.HttpContext.Current.Request["ItemId"] == "" ? "0" : System.Web.HttpContext.Current.Request["ItemId"]);
                string Message, fileName, actualFileName;
                Message = fileName = actualFileName = string.Empty;
                if (Request.Files.Count > 0)
                {
                    var fileContent = Request.Files[0];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        actualFileName = fileContent.FileName;
                        fileName = Guid.NewGuid() + Path.GetExtension(fileContent.FileName);
                        using (Bitmap bmp = new Bitmap(fileContent.InputStream, false))
                        {
                            using (Graphics grp = Graphics.FromImage(bmp))
                            {
                                string filePath = Server.MapPath(Url.Content("~/fonts/watermark.png"));
                                Image logoImage = Image.FromFile(filePath);
                                Image TargetImg = Image.FromStream(fileContent.InputStream);
                                RectangleF rectf = grp.VisibleClipBounds;
                                var destX = (TargetImg.Width - logoImage.Width) - 30;
                                var destY = (TargetImg.Height - logoImage.Height) - 30;
                                float cxImage = grp.DpiX * logoImage.Width /
                                                logoImage.HorizontalResolution;
                                float cyImage = grp.DpiY * logoImage.Height /
                                                                   logoImage.VerticalResolution;
                                grp.DrawImage(logoImage, (rectf.Width - cxImage) / 2,
                                    (rectf.Height - cyImage) / 2);
                                // grp.DrawImage(logoImage, new Point(TargetImg.Width - logoImage.Width - 10, 10));
                                bmp.Save(Path.Combine(Server.MapPath("~/UploadImages/"), fileName));
                            }
                        }
                        itemMasterDto.ItemImage = fileName;
                    }
                    //fileContent.SaveAs(Path.Combine(Server.MapPath("~/UploadImages/"), fileName));
                }
                else
                {
                    fileName = "";
                    if (itemMasterDto.ItemId > 0)
                    {
                        dynamic imgFile = itemMasterDal.GetAndEditItemMaster(itemMasterDto.ItemId, 1);
                        itemMasterDto.ItemImage = Convert.ToString(imgFile[0].ItemImage);
                    }
                    else
                    {
                        itemMasterDto.ItemImage = fileName;
                    }
                }
                itemMasterDto.GroupId = Convert.ToInt32(System.Web.HttpContext.Current.Request["GroupId"]);
                itemMasterDto.CategoryId = Convert.ToInt32(System.Web.HttpContext.Current.Request["CategoryId"]);
                itemMasterDto.ItemName = System.Web.HttpContext.Current.Request["ItemName"];
                itemMasterDto.ItemDesc = System.Web.HttpContext.Current.Request["ItemDesc"];
                itemMasterDto.CreatedBy = Convert.ToInt32(Session["AdminId"]);
                itemMasterDto.ModifiedBy = Convert.ToInt32(Session["AdminId"]);
                itemMasterDto.IsActive = 1;

                int i = itemMasterDal.SaveItemMaster(itemMasterDto);
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult DeleteItemMaster(int ItemId)
        {
            ItemMasterDal itemMasterDal = new ItemMasterDal();
            int i = itemMasterDal.DeleteItemMaster(ItemId);
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        [UserAuthenticationFilter]
        public ActionResult ItemDetails()
        {
            ViewBag.AdminDetails = TempData["AdminDetails"];
            TempData.Keep();
            return View();
        }
        [HttpPost]
        public ActionResult SaveItemDetails(ItemDetailsDto itemDetailsDto)
        {
            try
            {
                ItemDetailsDal itemDetailsDal = new ItemDetailsDal();
                itemDetailsDto.CreatedBy = Convert.ToInt32(Session["AdminId"]);
                itemDetailsDto.ModifiedBy = Convert.ToInt32(Session["AdminId"]);
                itemDetailsDto.IsActive = 1;
                int i = itemDetailsDal.SaveItemDetails(itemDetailsDto);
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult GetAllItemDetails()
        {
            ItemDetailsDal itemDetailsDal = new ItemDetailsDal();
            List<ItemDetailsDto> lstItemDetailsDto = itemDetailsDal.GetAndEditItemDetails(0, 1);
            return Json(lstItemDetailsDto, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteItemDetails(int ItemDetailsId)
        {
            ItemDetailsDal itemDetailsDal = new ItemDetailsDal();
            int i = itemDetailsDal.DeleteItemDetails(ItemDetailsId);
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        [UserAuthenticationFilter]
        public ActionResult StockMaster()
        {
            ViewBag.AdminDetails = TempData["AdminDetails"];
            TempData.Keep();
            return View();
        }
        [HttpPost]
        public ActionResult GetItemByCategoryId(int CategoryId)
        {
            ItemMasterDal itemMasterDal = new ItemMasterDal();
            List<ItemMasterDto> lstItemMasterDto = itemMasterDal.GetItemByCategoryId(CategoryId);
            return Json(lstItemMasterDto, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveStockMaster(StockMasterDto stockMasterDto)
        {
            StockMasterDal stockMasterDal = new StockMasterDal();
            stockMasterDto.CreatedBy = Convert.ToInt32(Session["AdminId"]);
            stockMasterDto.ModifiedBy = Convert.ToInt32(Session["AdminId"]);
            stockMasterDto.IsActive = 1;
            int i = stockMasterDal.SaveStockMaster(stockMasterDto);
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAllStockMaster()
        {
            StockMasterDal stockMasterDal = new StockMasterDal();
            List<StockMasterDto> lstStockMasterDto = stockMasterDal.GetAndEditStockMaster(0, 1);
            return Json(lstStockMasterDto, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteStockMaster(int StockId)
        {
            StockMasterDal stockMasterDal = new StockMasterDal();
            int i = stockMasterDal.DeleteStockMaster(StockId);
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        [UserAuthenticationFilter]
        public ActionResult MainBanner()
        {
            ViewBag.AdminDetails = TempData["AdminDetails"];
            TempData.Keep();
            return View();
        }
        [HttpPost]
        public ActionResult SaveMainBanner()
        {
            try
            {
                MainBannerDal mainBannerDal = new MainBannerDal();
                MainBannerDto mainBannerDto = new MainBannerDto();
                mainBannerDto.BannerId = Convert.ToInt32(System.Web.HttpContext.Current.Request["BannerId"] == "" ? "0" : System.Web.HttpContext.Current.Request["BannerId"]);
                string Message, fileName, actualFileName;
                Message = fileName = actualFileName = string.Empty;
                if (Request.Files.Count > 0)
                {
                    var fileContent = Request.Files[0];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        actualFileName = fileContent.FileName;
                        fileName = Guid.NewGuid() + Path.GetExtension(fileContent.FileName);
                        using (Bitmap bmp = new Bitmap(fileContent.InputStream, false))
                        {
                            using (Graphics grp = Graphics.FromImage(bmp))
                            {
                                string filePath = Server.MapPath(Url.Content("~/fonts/watermark.png"));
                                Image logoImage = Image.FromFile(filePath);
                                Image TargetImg = Image.FromStream(fileContent.InputStream);
                                RectangleF rectf = grp.VisibleClipBounds;
                                var destX = (TargetImg.Width - logoImage.Width) - 30;
                                var destY = (TargetImg.Height - logoImage.Height) - 30;
                                float cxImage = grp.DpiX * logoImage.Width /
                                                logoImage.HorizontalResolution;
                                float cyImage = grp.DpiY * logoImage.Height /
                                                                   logoImage.VerticalResolution;
                                grp.DrawImage(logoImage, (rectf.Width - cxImage) / 2,
                                    (rectf.Height - cyImage) / 2);
                                // grp.DrawImage(logoImage, new Point(TargetImg.Width - logoImage.Width - 10, 10));
                                bmp.Save(Path.Combine(Server.MapPath("~/UploadImages/"), fileName));
                            }
                        }
                        mainBannerDto.BannerImage = fileName;
                    }
                }
                else
                {
                    fileName = "";
                    if (mainBannerDto.BannerId > 0)
                    {
                        dynamic imgFile = mainBannerDal.GetAllMainBanner(mainBannerDto.BannerId, 1);
                        mainBannerDto.BannerImage = Convert.ToString(imgFile[0].BannerImage);
                    }
                    else
                    {
                        mainBannerDto.BannerImage = fileName;
                    }
                }
                mainBannerDto.ItemId = Convert.ToInt32(System.Web.HttpContext.Current.Request["ItemId"]);
                mainBannerDto.BannerName = System.Web.HttpContext.Current.Request["BannerName"];
                mainBannerDto.BannerDesc = System.Web.HttpContext.Current.Request["BannerDesc"];
                mainBannerDto.CreatedBy = Convert.ToInt32(Session["AdminId"]);
                mainBannerDto.ModifiedBy = Convert.ToInt32(Session["AdminId"]);
                mainBannerDto.IsActive = 1;

                int i = mainBannerDal.SaveAndUpdateMainBanner(mainBannerDto);
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult GetAllMainBanner()
        {
            MainBannerDal mainBannerDal = new MainBannerDal();
            List<MainBannerDto> lstMainBannerDto = mainBannerDal.GetAllMainBanner(0, 1);
            return Json(lstMainBannerDto, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteMainBanner(int BannerId)
        {
            MainBannerDal mainBannerDal = new MainBannerDal();
            int i = mainBannerDal.DeleteMainBanner(BannerId);
            return Json(i, JsonRequestBehavior.AllowGet);
        }
    }
}