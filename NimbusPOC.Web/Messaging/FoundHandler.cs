using System.Threading.Tasks;
using Nimbus.Handlers;
using NimbusPOC.Web.DependencyResolution;

namespace NimbusPOC.Web.Messaging
{
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
}