using Nimbus.MessageContracts;

namespace NimbusPOC.Web.Messaging
{
    public class FoundCommand : IBusCommand
    {
        public string Who { get; set; }
    }
}