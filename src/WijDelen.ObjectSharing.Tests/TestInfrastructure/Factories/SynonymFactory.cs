using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Title.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories {
    public class SynonymFactory
    {
        private int _nextId = 1;

        /// <summary>
        /// Creates a new ContentItem with the given title in a TitlePart, an increasing Id (starting with 1), and an optional link to an Archetype.
        /// </summary>
        public ContentItem Create(string title, int? archetypeId = null)
        {
            var result = new ContentItem
            {
                VersionRecord = new ContentItemVersionRecord
                {
                    Id = _nextId++
                }
            };

            var titlePart = new TitlePart { Record = new TitlePartRecord() };
            titlePart.TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition("TitlePart"), new SettingsDictionary());
            result.Weld(titlePart);

            var infosetPart = new InfosetPart();
            infosetPart.TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition("InfosetPart"), new SettingsDictionary());
            result.Weld(infosetPart);

            var contentPickerFieldStorage = new FakeFieldStorage();
            var contentPickerField = new ContentPickerField { Storage = contentPickerFieldStorage, PartFieldDefinition = new ContentPartFieldDefinition("Archetype")};
            var archetypePart = new ContentPart();
            archetypePart.TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition("Synonym"), new SettingsDictionary());

            archetypePart.Weld(contentPickerField);
            result.Weld(archetypePart);

            titlePart.Title = title;
            contentPickerField.Ids = archetypeId.HasValue ? new[] { archetypeId.Value } : new int[0];
            return result;
        }
    }
}