using System;
using System.Collections.Generic;

namespace WijDelen.ObjectSharing.Domain.Services {
    public class RandomSampleService : IRandomSampleService {
        public IEnumerable<T> GetRandomSample<T>(IList<T> list, int sampleSize) {
            if (list == null) {
                throw new ArgumentNullException(nameof(list));
            }

            if (sampleSize > list.Count) {
                return list;
            }

            var indices = new Dictionary<int, int>();

            var result = new List<T>();
            var rnd = new Random();

            for (var i = 0; i < sampleSize; i++) {
                var j = rnd.Next(i, list.Count);
                int index;

                if (!indices.TryGetValue(j, out index)) {
                    index = j;
                }

                result.Add(list[index]);

                if (!indices.TryGetValue(i, out index)) {
                    index = i;
                }

                indices[j] = index;
            }

            return result;
        }
    }
}