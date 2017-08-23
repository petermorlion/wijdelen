using System;
using System.Collections.Generic;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    public class RequestsDetailsQuery : IRequestsDetailsQuery {
        public IEnumerable<RequestsDetailsViewModel> GetResults(int groupId, DateTime startDate, DateTime stopDate) {
            return new List<RequestsDetailsViewModel> {
                new RequestsDetailsViewModel {
                    CreatedDateTime = new DateTime(2015, 1, 1),
                    Description = "Item 1",
                    Email = "peter.morlion@live.be",
                    MailCount = 1,
                    YesCount = 2,
                    NoCount = 3,
                    NotNowCount = 4
                },
                new RequestsDetailsViewModel {
                    CreatedDateTime = new DateTime(2016, 1, 1),
                    Description = "Item 2",
                    Email = "peter.morlion@live.be",
                    MailCount = 3,
                    YesCount = 24,
                    NoCount = 13,
                    NotNowCount = 54
                }
            };
        }
    }
}