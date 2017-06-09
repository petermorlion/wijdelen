using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.Security;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Tests.Mocks {
    public class UserMockFactory {
        private int _nextId = 1;

        public IUser Create(string userName, string email, string firstName, string lastName, string culture, GroupMembershipStatus groupMembershipStatus)
        {
            var contentItem = new ContentItem
            {
                VersionRecord = new ContentItemVersionRecord
                {
                    Id = _nextId++
                }
            };

            var userDetailsPart = new UserDetailsPart { Record = new UserDetailsPartRecord() };
            userDetailsPart.TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition("UserDetailsPart"), new SettingsDictionary());
            contentItem.Weld(userDetailsPart);

            var infosetPart = new InfosetPart();
            infosetPart.TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition("InfosetPart"), new SettingsDictionary());
            contentItem.Weld(infosetPart);

            var groupMembershipPart = new GroupMembershipPart { Record = new GroupMembershipPartRecord() };
            groupMembershipPart.TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition("GroupMembershipPart"), new SettingsDictionary());
            contentItem.Weld(groupMembershipPart);

            userDetailsPart.FirstName = firstName;
            userDetailsPart.LastName = lastName;
            userDetailsPart.Culture = culture;
            userDetailsPart.ReceiveMails = true;

            groupMembershipPart.GroupMembershipStatus = groupMembershipStatus;

            var user = new UserMock
            {
                Id = _nextId,
                ContentItem = contentItem,
                Email = email,
                UserName = userName
            };

            return user;
        }
    }
}