using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Nimbus;
using Nimbus.Handlers;
using Nimbus.MessageContracts;
using NimbusPOC.Web.DependencyResolution;

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

    public class FoundThemViewModel
    {
        public IEnumerable<string> Thems { get; set; }
    }

    public class FoundHandler : IHandleCommand<FoundCommand>
    {
        private readonly ThemsModel _model;

        public FoundHandler(ThemsModel model)
        {
            _model = model;
        }

        public Task Handle(FoundCommand busCommand)
        {
            return Task.Factory.StartNew(() => _model.Add(busCommand.Who));
        }
    }

    public class FoundCommand : IBusCommand
    {
        public string Who { get; set; }
    }

    public class FoundWhoViewModel
    {
        public string Who { get; set; }
    }

    public class IndexViewModel
    {
    }
}