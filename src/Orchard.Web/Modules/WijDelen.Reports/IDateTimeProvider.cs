using System;
using Orchard;

namespace WijDelen.Reports {
    public interface IDateTimeProvider : IDependency {
        DateTime UtcNow();
    }
}