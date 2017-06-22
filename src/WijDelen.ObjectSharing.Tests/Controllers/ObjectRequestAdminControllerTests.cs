using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels.Admin;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ObjectRequestAdminControllerTests {
        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            _repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            _commandHandler = new Mock<ICommandHandler<UnblockObjectRequests>>();
            _notifierMock = new Mock<INotifier>();

            builder.RegisterInstance(_repositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterInstance(_commandHandler.Object).As<ICommandHandler<UnblockObjectRequests>>();
            builder.RegisterInstance(_notifierMock.Object).As<INotifier>();
            builder.RegisterType<ObjectRequestAdminController>();

            var container = builder.Build();
            _controller = container.Resolve<ObjectRequestAdminController>();

            _controller.T = NullLocalizer.Instance;
        }

        private ObjectRequestAdminController _controller;
        private Mock<IRepository<ObjectRequestRecord>> _repositoryMock;
        private Mock<ICommandHandler<UnblockObjectRequests>> _commandHandler;
        private Mock<INotifier> _notifierMock;

        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a
        /// runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var localizer = NullLocalizer.Instance;

            _controller.T = localizer;

            Assert.AreEqual(localizer, _controller.T);
        }

        [Test]
        public void WhenGettingIndex() {
            var objectRequestRecord1 = new ObjectRequestRecord {
                AggregateId = Guid.NewGuid(),
                GroupName = "The Simpsons",
                Description = "Sneakers",
                Status = "None",
                CreatedDateTime = new DateTime(2016, 1, 1)
            };
            var objectRequestRecord2 = new ObjectRequestRecord {
                AggregateId = Guid.NewGuid(),
                GroupName = "The Flintstones",
                Description = "A rock",
                Status = "BlockedForForbiddenWords",
                CreatedDateTime = new DateTime(2017, 1, 1)
            };
            var records = new List<ObjectRequestRecord> {
                objectRequestRecord2,
                objectRequestRecord1
            };

            _repositoryMock.Setup(x => x.Table).Returns(records.AsQueryable());

            var result = _controller.Index();

            result.Should().BeOfType<ViewResult>();
            var model = result.As<ViewResult>().Model.As<ObjectRequestAdminViewModel>();
            model.Page.Should().Be(1);
            model.ObjectRequestsCount.Should().Be(2);
            model.HasPreviousPage.Should().Be(false);
            model.HasNextPage.Should().Be(false);
            model.TotalPages.Should().Be(1);
            var recordViewModels = model.ObjectRequests;
            recordViewModels[0].ShouldBeEquivalentTo(new ObjectRequestRecordViewModel
            {
                AggregateId = objectRequestRecord2.AggregateId,
                GroupName = "The Flintstones",
                Description = "A rock",
                IsSelected = false,
                Status = "Blocked",
                CreatedDateTime = new DateTime(2017, 1, 1)
            });
            recordViewModels[1].ShouldBeEquivalentTo(new ObjectRequestRecordViewModel {
                AggregateId = objectRequestRecord1.AggregateId,
                GroupName = "The Simpsons",
                Description = "Sneakers",
                IsSelected = false,
                Status = "",
                CreatedDateTime = new DateTime(2016, 1, 1)
            });
            
            recordViewModels.Count.Should().Be(2);
        }

        [Test]
        public void WhenGettingIndexWithMoreThan50Requests() {
            var records = new List<ObjectRequestRecord>();
            for (var i = 0; i < 100; i++)
                records.Add(new ObjectRequestRecord());

            _repositoryMock.Setup(x => x.Table).Returns(records.AsQueryable());

            var result = _controller.Index();

            result.Should().BeOfType<ViewResult>();
            var model = result.As<ViewResult>().Model.As<ObjectRequestAdminViewModel>();
            model.Page.Should().Be(1);
            model.ObjectRequestsCount.Should().Be(100);
            model.HasPreviousPage.Should().Be(false);
            model.HasNextPage.Should().Be(true);
            model.TotalPages.Should().Be(2);
            var recordViewModels = model.ObjectRequests;
            recordViewModels.Count.Should().Be(50);
        }

        [Test]
        public void WhenGettingIndexWithPage() {
            var records = new List<ObjectRequestRecord>();
            for (var i = 0; i < 50; i++)
                records.Add(new ObjectRequestRecord {
                    Id = i
                });

            for (var i = 50; i < 100; i++)
                records.Add(new ObjectRequestRecord {
                    Id = i,
                    Description = "Second half"
                });

            _repositoryMock.Setup(x => x.Table).Returns(records.AsQueryable());

            var result = _controller.Index(2);

            result.Should().BeOfType<ViewResult>();
            var model = result.As<ViewResult>().Model.As<ObjectRequestAdminViewModel>();
            model.Page.Should().Be(2);
            model.ObjectRequestsCount.Should().Be(100);
            model.HasPreviousPage.Should().Be(true);
            model.HasNextPage.Should().Be(false);
            model.TotalPages.Should().Be(2);
            var recordViewModels = model.ObjectRequests;
            recordViewModels.Count.Should().Be(50);
            recordViewModels.All(x => x.Description == "Second half").Should().BeTrue();
        }

        [Test]
        public void WhenGettingIndexWithPageTooFarOff() {
            var records = new List<ObjectRequestRecord>();
            for (var i = 0; i < 50; i++)
                records.Add(new ObjectRequestRecord {
                    Id = i
                });

            for (var i = 50; i < 100; i++)
                records.Add(new ObjectRequestRecord {
                    Id = i,
                    Description = "Second half"
                });

            _repositoryMock.Setup(x => x.Table).Returns(records.AsQueryable());

            var result = _controller.Index(200);

            result.Should().BeOfType<RedirectToRouteResult>();
            ((RedirectToRouteResult) result).RouteValues["action"].Should().Be("Index");
            ((RedirectToRouteResult) result).RouteValues["page"].Should().Be(2);
        }

        [Test]
        public void WhenPostingIndex() {
            var aggregateId1 = Guid.NewGuid();
            var aggregateId2 = Guid.NewGuid();
            var recordViewModels = new List<ObjectRequestRecordViewModel> {
                new ObjectRequestRecordViewModel {
                    AggregateId = aggregateId1,
                    IsSelected = true
                },
                new ObjectRequestRecordViewModel {
                    AggregateId = aggregateId2,
                    IsSelected = true
                },
                new ObjectRequestRecordViewModel {
                    AggregateId = Guid.NewGuid(),
                    IsSelected = false
                }
            };

            UnblockObjectRequests command = null;
            _commandHandler
                .Setup(x => x.Handle(It.IsAny<UnblockObjectRequests>()))
                .Callback((UnblockObjectRequests cmd) => command = cmd);

            var viewModel = new ObjectRequestAdminViewModel {
                ObjectRequests = recordViewModels,
                Page = 2
            };

            var result = _controller.Index(viewModel);

            command.ObjectRequestIds.ShouldBeEquivalentTo(new List<Guid> {
                aggregateId1,
                aggregateId2
            });

            result.Should().BeOfType<RedirectToRouteResult>();
            ((RedirectToRouteResult) result).RouteValues["action"].Should().Be("Index");
            ((RedirectToRouteResult) result).RouteValues["page"].Should().Be(2);
            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("The selected requests were unblocked and mails have been sent to the users.")));
        }

        [Test]
        public void WhenPostingIndexWithoutSelection() {
            var aggregateId1 = Guid.NewGuid();
            var aggregateId2 = Guid.NewGuid();
            var recordViewModels = new List<ObjectRequestRecordViewModel> {
                new ObjectRequestRecordViewModel {
                    AggregateId = aggregateId1,
                    IsSelected = false
                },
                new ObjectRequestRecordViewModel {
                    AggregateId = aggregateId2,
                    IsSelected = false
                },
                new ObjectRequestRecordViewModel {
                    AggregateId = Guid.NewGuid(),
                    IsSelected = false
                }
            };

            var viewModel = new ObjectRequestAdminViewModel {
                ObjectRequests = recordViewModels,
                Page = 2
            };

            var result = _controller.Index(viewModel);

            result.Should().BeOfType<RedirectToRouteResult>();
            ((RedirectToRouteResult) result).RouteValues["action"].Should().Be("Index");
            ((RedirectToRouteResult) result).RouteValues["page"].Should().Be(2);
            _commandHandler.Verify(x => x.Handle(It.IsAny<UnblockObjectRequests>()), Times.Never);
            _notifierMock.Verify(x => x.Add(NotifyType.Warning, new LocalizedString("Please select at least one request to unblock.")));
        }
    }
}