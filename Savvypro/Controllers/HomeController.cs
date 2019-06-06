using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.IO;

namespace Savvypro.Controllers
{
    public class HomeController : Controller
    {
        //string ocrResult = "causes within 7 calendar days from the date of adoption; either must be requested by adopter only.                             \r\nC.    To exchange or make refund on any animal that exhibits unusual or unpredictable behavioral tendencies within 7                             \r\ncalendar days from the date of adoption; either must requested by the adopter only.                                                              \r\nD.    The terms of this contract expires at 5:00 p.m. on (date) 12/03/07_ .                                                                      \r\nII.    Participating veterinary hospitals agree to provide the services stated herein for animals adopted from the Wilson County                 \r\nAnimal Shelter at no charge to the adopter and these services must be performed to validate the terms of this contract                           \r\nexcept                                                                                                                                           \r\nas pertains to death from natural causes. A free examination will be given to include a visual and physical examination                          \r\nincluding a check of temperature, pulse, respiration, eyes, gum coloring, ears, and palpating the abdomen. The                                   \r\nexamination                                                                                                                                      \r\nDOES NOT include any lab fees, vaccinations, or medications. These services included will be provided on a one time                              \r\nbasis                                                                                                                                            \r\nonly.                                                                                                                                            \r\nIII.    In return, as adopter, I Yan Yanovich__, agree to the following:                                                                         \r\nA.    To take the animal to a participating veterinary hospital within stated period of time, which is 7 calendar days, to                       \r\nreceive initial free examination. Form A must be completed by attending licensed veterinarian.                                                   \r\nB.    The animal will not be allowed to breed indiscriminately. As the adopter, I agree to provide adequate food, water,                         \r\nshelter, and exercise and agree to obey all applicable laws governing control and custody of the animal, to include,                             \r\nbut not limited to, the proper confinement laws and wearing of tags as applicable.                                                               \r\nC.    To provide with proper veterinary care as related to the specific type of animal. This is meant to include any yearly or                   \r\nother vaccinations, any needed medications or other special care as needed.                                                                      \r\nD.    To exchange an animal, I understand that I must follow these specific criteria:                                                            \r\n1.    I must have complied with Article A of this section.                                                                                       \r\n2.    The animal must be in such poor health that major medical treatment is necessary, unless #3 in this                                        \r\nsection                                                                                                                                          \r\napplies. Minor parasitical treatment is not considered a major medical problem.                                                                  \r\n3.    If the above free examination has been performed and the animal's behavior is unacceptable (this is not                                    \r\nto                                                                                                                                               \r\ncover housetraining or any other trainable characteristic) to your household or living conditions, a refund                                      \r\nmay be made with the proper completion of Form B.                                                                                                \r\n4.    The animal along with the veterinarians completion of Form A shall be brought to the Animal Shelter, or                                    \r\nin                                                                                                                                               \r\nthe event of the animal's death, the carcass and/or other satisfactory evidence of the death and date of                                         \r\ndeath must be presented. Terms of this contract will recognize death by natural causes only.                                                     \r\n5.    The request for a refund or exchange must be made by the date and time specified in Section I, Article D                                   \r\nof                                                                                                                                               \r\nthis contract.                                                                                                                                   \r\n6.    No refund or substitution shall be given for the animal that has been lost, stolen, or killed.                                             \r\nI understand the adoption contract requirements and further agree that failure to comply with this contract will result in the animal            \r\n";

        public ActionResult Index()
        {
            var mvcName = typeof(Controller).Assembly.GetName();
            var isMono = Type.GetType("Mono.Runtime") != null;

            ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
            ViewData["Runtime"] = isMono ? "Mono" : ".NET";

            return View();
        }
        public ActionResult Savvypro()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveImage(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + "-" + System.IO.Path.GetFileName(file.FileName);
                    if (!Directory.Exists(Server.MapPath("~/Content/Upload")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Upload"));
                    }

                    string path = Path.Combine(
                                           Server.MapPath("~/Content/Upload"), fileName);
                    file.SaveAs(path);

                    //OCR function
                    string ApplicationId = "Lei_test_app";
                    string Password = "WPu1KovrJdN7i37WCuOOppVx";
                    string FilePath = "~/Content/Upload/"+fileName;

                    OCR oCR = new OCR(ApplicationId, Password, FilePath);
                    string ocrResult = oCR.GetResult(FilePath, "English", "txt");


                    System.Threading.Thread.Sleep(1000);
                    return Json(new { success = true, responseText = "Successfully uploaded image file.", ocrResult = ocrResult }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("Cannot save file.");
                }


            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { error = -1, responseText = e.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
