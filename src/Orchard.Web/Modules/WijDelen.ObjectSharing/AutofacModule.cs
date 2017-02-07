using Autofac;
using WijDelen.ObjectSharing.Domain.CommandHandlers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure;

namespace WijDelen.ObjectSharing {
    /// <summary>
    /// Registers certain dependencies manually, instead of letting them implement IDependency. This is necessary because we're using generics 
    /// (see http://orchard.codeplex.com/discussions/280097).
    /// </summary>
    public class AutofacModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterGeneric(typeof(OrchardEventSourcedRepository<>)).As(typeof(IEventSourcedRepository<>)).InstancePerLifetimeScope();

            builder.RegisterType<ObjectRequestCommandHandler>()
                .As<ICommandHandler<RequestObject>>()
                .As<ICommandHandler<ConfirmObjectRequest>>()
                .As<ICommandHandler<DenyObjectRequest>>()
                .As<ICommandHandler<DenyObjectRequestForNow>>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ChatCommandHandler>()
                .As<ICommandHandler<AddChatMessage>>()
                .InstancePerLifetimeScope();
        }
    }
}