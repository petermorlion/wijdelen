using System;
using System.Collections.Generic;
using Orchard;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    public interface IGroupDetailsQuery : IDependency {
        /// <summary>
        /// Gets a detailed report between two dates, for a given group. If null is given for the groupId,
        /// the report is returned for all groups.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="startDate"></param>
        /// <param name="stopDate"></param>
        /// <returns></returns>
        IEnumerable<GroupDetailsViewModel> GetResults(int? groupId, DateTime startDate, DateTime stopDate);
    }
}