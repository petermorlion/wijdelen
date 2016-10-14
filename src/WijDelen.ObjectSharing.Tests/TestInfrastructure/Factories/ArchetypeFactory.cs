using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Records;
using Orchard.Core.Title.Models;

namespace WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories {
    public class ArchetypeFactory {
        private int _nextId = 1;

        /// <summary>
        /// Creates a new ContentItem with the given title in a TitlePart and an increasing Id (starting with 1).
        /// </summary>
        public ContentItem Create(string title) {
            var result = new ContentItem {
                VersionRecord = new ContentItemVersionRecord {
                    ContentItemRecord = new ContentItemRecord {
                        Id = _nextId++
                    }
                }
            };

            var titlePart = new TitlePart { Record = new TitlePartRecord() };
            result.Weld(titlePart);

            var infosetPart = new InfosetPart();
            result.Weld(infosetPart);

            titlePart.Title = title;
            return result;
        }
    }
}