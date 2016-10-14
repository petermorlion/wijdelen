using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Core.Title.Models;

namespace WijDelen.ObjectSharing.Tests.Controllers.Builders {
    public class ArchetypeFactory {
        public ContentItem Create(string title) {
            var result = new ContentItem();

            var titlePart = new TitlePart { Record = new TitlePartRecord() };
            result.Weld(titlePart);

            var infosetPart = new InfosetPart();
            result.Weld(infosetPart);

            titlePart.Title = title;
            return result;
        }
    }
}