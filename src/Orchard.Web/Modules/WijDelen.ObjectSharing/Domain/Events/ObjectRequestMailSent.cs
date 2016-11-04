using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    /// <summary>
    /// Indicates that there was a request to send the mail of an object request. Doesn't mean
    /// the mails have been sent yet (because the mail service might be down for example).
    /// </summary>
    public class ObjectRequestMailSent : VersionedEvent {
        
    }
}