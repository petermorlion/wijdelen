using System;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    public interface ICommand
    {
        Guid Id { get; }
    }
}