using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Nimbus;
using NimbusPOC.Web.DependencyResolution;
using NimbusPOC.Web.Messaging;
using NimbusPOC.Web.ViewModels;

namespace NimbusPOC.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBus _bus;
        private readonly ThemsModel _themsModel;

        public HomeController(IBus bus, ThemsModel themsModel)
        {
            _bus = bus;
            _themsModel = themsModel;
        }

        public ActionResult Index()
        {
            return View(new IndexViewModel());
        }

        [HttpPost]
        public ActionResult Index(IndexViewModel model)
        {
            return View(model);
        }

        public ActionResult FoundWho()
        {
            return PartialView(new FoundWhoViewModel());
        }

        [HttpPost]
        public async Task<ActionResult> FoundWho(FoundWhoViewModel model)
        {
            await _bus.Send(new FoundCommand {Who = model.Who});
            return Redirect("/");
        }

        public ActionResult FoundThem()
        {
            var model = new FoundThemViewModel
                        {
                            Thems = _themsModel.GetAll(),
                        };
            return PartialView(model);
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