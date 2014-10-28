using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.Infrastructure;
using Nimbus.MessageContracts;
using StructureMap;
using StructureMap.Pipeline;

namespace NimbusPOC.Web.DependencyResolution {
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;
	
    public class DefaultRegistry : Registry {
        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                    scan.ConnectImplementationsToTypesClosing(typeof (IHandleCommand<>));
                    scan.ConnectImplementationsToTypesClosing(typeof (IHandleRequest<,>));
                    scan.ConnectImplementationsToTypesClosing(typeof (IHandleMulticastEvent<>));
                    scan.ConnectImplementationsToTypesClosing(typeof (IHandleCompetingEvent<>));
                });
            For<ThemsModel>().Use(new ThemsModel());
            For<IBus>().Use(context => context.GetInstance<BusFactory>().Create()).LifecycleIs<SingletonLifecycle>();
        }
    }

    public class ThemsModel
    {
        readonly ConcurrentBag<string> _db = new ConcurrentBag<string>();

        public void Add(string s)
        {
            _db.Add(s);
        }

        public IList<string> GetAll()
        {
            return _db.ToList();
        }
    }

    public class BusFactory
    {
        private readonly StructureMapHandlerFactory _factory;

        public BusFactory(StructureMapHandlerFactory factory)
        {
            _factory = factory;
        }

        public IBus Create()
        {
            var connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];

            // This is how you tell Nimbus where to find all your message types and handlers.
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());
            
            var bus = new BusBuilder().Configure()
                                        .WithNames("NimbusPOC", Environment.MachineName)
                                        .WithConnectionString(connectionString)
                                        .WithTypesFrom(typeProvider)
                                        .WithCommandHandlerFactory(_factory)
                                        .WithRequestHandlerFactory(_factory)
                                        .WithMulticastRequestHandlerFactory(_factory)
                                        .WithMulticastEventHandlerFactory(_factory)
                                        .WithCompetingEventHandlerFactory(_factory)
                                        .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                        .Build();
            bus.Start();
            return bus;
        }
    }

    public class StructureMapHandlerFactory : ICommandHandlerFactory, IRequestHandlerFactory, IMulticastRequestHandlerFactory, IMulticastEventHandlerFactory, ICompetingEventHandlerFactory
    {
        private readonly IContainer _container;

        public StructureMapHandlerFactory(IContainer container)
        {
            _container = container;
        }

        public OwnedComponent<IHandleCommand<TBusCommand>> GetHandler<TBusCommand>() where TBusCommand : IBusCommand
        {
            return new OwnedComponent<IHandleCommand<TBusCommand>>(_container.GetInstance<IHandleCommand<TBusCommand>>());
        }

        public OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>> GetHandler<TBusRequest, TBusResponse>() where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
        {
            return new OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>>(_container.GetInstance<IHandleRequest<TBusRequest, TBusResponse>>());
        }

        public OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>> GetHandlers<TBusRequest, TBusResponse>() where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
        {
            return new OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>(_container.GetInstance<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>>());
        }

        OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>> IMulticastEventHandlerFactory.GetHandlers<TBusEvent>()
        {
            return new OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>>(_container.GetInstance<IEnumerable<IHandleMulticastEvent<TBusEvent>>>());
        }

        OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> ICompetingEventHandlerFactory.GetHandlers<TBusEvent>()
        {
            return new OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>>(_container.GetInstance<IEnumerable<IHandleCompetingEvent<TBusEvent>>>());
        }
    }
}