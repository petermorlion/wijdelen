using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using WijDelen.Mailgun.Models;

namespace WijDelen.Mailgun.Handlers {
    public class MailgunSettingsPartHandler : ContentHandler {
        public MailgunSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<MailgunSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<MailgunSettingsPart>("MailgunSettings", "Parts/Mailgun.Settings", "Mailgun"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Mailgun")));
        }
    }
}