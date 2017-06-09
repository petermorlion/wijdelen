using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.Security;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories
{
    public class UserFactory
    {
        private int _nextId = 1;

        public IUser Create(string userName, string email, string firstName, string lastName, bool receiveMails = true, GroupMembershipStatus groupMembershipStatus = GroupMembershipStatus.Approved)
        {
            var contentItem = new ContentItem {
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
            userDetailsPart.ReceiveMails = receiveMails;

            groupMembershipPart.GroupMembershipStatus = groupMembershipStatus;

            var user = new FakeUser {
                Id = _nextId,
                ContentItem = contentItem,
                Email = email,
                UserName = userName
            };

            return user;
        }
    }
}