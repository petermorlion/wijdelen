using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.UI.Notify;
using WijDelen.Contact.Controllers;
using WijDelen.Contact.Models;
using WijDelen.Contact.Services;

namespace WijDelen.Contact.Tests.Controllers {
    [TestFixture]
    public class ContactControllerTests
    {
        private ContactController _controller;
        private Mock<IMailService> _mailServiceMock;
        private Mock<INotifier> _notifierMock;
        private Mock<IRecaptchaService> _recaptchaServiceMock;

        [SetUp]
        public void Init() {
            _mailServiceMock = new Mock<IMailService>();
            _notifierMock = new Mock<INotifier>();
            _recaptchaServiceMock = new Mock<IRecaptchaService>();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<ContactController>();
            containerBuilder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();
            containerBuilder.RegisterInstance(_notifierMock.Object).As<INotifier>();
            containerBuilder.RegisterInstance(_recaptchaServiceMock.Object).As<IRecaptchaService>();

            var container = containerBuilder.Build();

            _controller = container.Resolve<ContactController>();
        }

        [Test]
        public void TestPost() {
            var viewModel = new ContactViewModel {
                Name = "Peter",
                Email = "peter@example.com",
                Subject = "A test",
                Text = "This is just a test"
            };

            _recaptchaServiceMock.Setup(x => x.Validates()).Returns(true);

            var result = _controller.Index(viewModel);

            _mailServiceMock.Verify(x => x.SendContactMails("Peter", "peter@example.com", "A test", "This is just a test"));
            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Thank you for your message. We will contact you as soon as possible.")));

            result.Should().BeOfType<RedirectToRouteResult>();
        }

        [Test]
        public void TestPostWithoutValues() {
            var viewModel = new ContactViewModel();

            var result = _controller.Index(viewModel);

            _mailServiceMock.Verify(x => x.SendContactMails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _notifierMock.Verify(x => x.Add(NotifyType.Success, It.IsAny<LocalizedString>()), Times.Never);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            viewResult.ViewData.ModelState["Name"].Errors.Single().ErrorMessage.Should().Be("Name is required.");
            viewResult.ViewData.ModelState["Email"].Errors.Single().ErrorMessage.Should().Be("Email is required.");
            viewResult.ViewData.ModelState["Subject"].Errors.Single().ErrorMessage.Should().Be("Subject is required.");
            viewResult.ViewData.ModelState["Text"].Errors.Single().ErrorMessage.Should().Be("Text is required.");
            viewResult.ViewData.ModelState["Recaptcha"].Errors.Single().ErrorMessage.Should().Be("Please prove you are not a bot.");
        }

        [Test]
        public void TestPostWithInvalidEmail() {
            var viewModel = new ContactViewModel {
                Name = "Peter",
                Email = "peter",
                Subject = "A test",
                Text = "This is just a test"
            };

            var result = _controller.Index(viewModel);

            _mailServiceMock.Verify(x => x.SendContactMails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _notifierMock.Verify(x => x.Add(NotifyType.Success, It.IsAny<LocalizedString>()), Times.Never);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)result;
            viewResult.ViewData.ModelState["Email"].Errors.Single().ErrorMessage.Should().Be("Please provide a valid email address.");
        }

        [Test]
        public void TestPostWithTooLongValues() {
            var longString = string.Join("", Enumerable.Repeat("a", 301));
            var veryLongString = string.Join("", Enumerable.Repeat("e", 3001));
            var viewModel = new ContactViewModel {
                Name = longString,
                Email = "peter@" + longString + ".com",
                Subject = longString,
                Text = veryLongString
            };

            var result = _controller.Index(viewModel);

            _mailServiceMock.Verify(x => x.SendContactMails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _notifierMock.Verify(x => x.Add(NotifyType.Success, It.IsAny<LocalizedString>()), Times.Never);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)result;
            viewResult.ViewData.ModelState["Name"].Errors.Single().ErrorMessage.Should().Be("Name is too long.");
            viewResult.ViewData.ModelState["Email"].Errors.Single().ErrorMessage.Should().Be("Email is too long.");
            viewResult.ViewData.ModelState["Subject"].Errors.Single().ErrorMessage.Should().Be("Subject is too long.");
            viewResult.ViewData.ModelState["Text"].Errors.Single().ErrorMessage.Should().Be("Text is too long.");
        }
    }
}