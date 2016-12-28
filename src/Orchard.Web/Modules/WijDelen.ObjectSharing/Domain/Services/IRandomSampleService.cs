using System.Collections.Generic;
using Orchard;

namespace WijDelen.ObjectSharing.Domain.Services {
    /// <summary>
    /// A service that returns a random sample of the items in a given list.
    /// </summary>
    public interface IRandomSampleService : IDependency {
        IEnumerable<T> GetRandomSample<T>(IList<T> list, int sampleSize);
    }
}