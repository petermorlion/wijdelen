using Orchard;

namespace WijDelen.Contact.Services {
    public interface IRecaptchaService : IDependency {
        bool Validates();
    }
}