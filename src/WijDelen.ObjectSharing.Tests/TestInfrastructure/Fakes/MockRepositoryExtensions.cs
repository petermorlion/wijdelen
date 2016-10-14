using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using Orchard.Data;

namespace WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes {
    public static class MockRepositoryExtensions {
        /// <summary>
        /// Sets up the mock repository to behave as though the given records are the records in the database.
        /// This removes the need for complicated .Setup and .Returns calls with Expressions.
        /// </summary>
        public static void SetRecords<T>(this Mock<IRepository<T>> repositoryMock, IEnumerable<T> records) {
            repositoryMock
                .Setup(x => x.Fetch(It.IsAny<Expression<Func<T, bool>>>()))
                .Returns((Expression<Func<T, bool>> expression) => {
                    var func = expression.Compile();
                    return records.Where(func).ToList();
                });

            repositoryMock
                .Setup(x => x.Table)
                .Returns(records.AsQueryable());
        }
    }
}