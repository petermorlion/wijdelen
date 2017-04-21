using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ObjectRequestAdminControllerTests {
        private IContainer _container;
        private ObjectRequestAdminController _controller;
        private Mock<IRepository<ObjectRequestRecord>> _repositoryMock;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            _repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();

            builder.RegisterInstance(_repositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterType<ObjectRequestAdminController>();

            _container = builder.Build();
            _controller = _container.Resolve<ObjectRequestAdminController>();
        }

        [Test]
        public void WhenGettingIndex() {
            var objectRequestRecord2 = new ObjectRequestRecord {
                CreatedDateTime = new DateTime(2017, 1, 1)
            };
            var objectRequestRecord1 = new ObjectRequestRecord{
                CreatedDateTime = new DateTime(2016, 1, 1)
            };
            var objectRequestRecord3 = new ObjectRequestRecord{
                CreatedDateTime = new DateTime(2018, 1, 1)
            };
            var records = new List<ObjectRequestRecord> {
                objectRequestRecord2,
                objectRequestRecord1,
                objectRequestRecord3
            };

            _repositoryMock.Setup(x => x.Table).Returns(records.AsQueryable());

            var result = _controller.Index();

            result.Should().BeOfType<ViewResult>();
            var model = result.As<ViewResult>().Model.As<IList<ObjectRequestRecord>>();
            model[0].Should().Be(objectRequestRecord3);
            model[1].Should().Be(objectRequestRecord2);
            model[2].Should().Be(objectRequestRecord1);
            model.Count.Should().Be(3);
        }

        [Test]
        public void WhenGettingIndexWithMoreThan50Requests() {
            var records = new List<ObjectRequestRecord>();
            for (var i = 0; i < 100; i++) {
                records.Add(new ObjectRequestRecord());
            }

            _repositoryMock.Setup(x => x.Table).Returns(records.AsQueryable());

            var result = _controller.Index();

            result.Should().BeOfType<ViewResult>();
            var model = result.As<ViewResult>().Model.As<IList<ObjectRequestRecord>>();
            model.Count.Should().Be(50);
        }

        [Test]
        public void WhenGettingIndexWithSkip() {
            var records = new List<ObjectRequestRecord>();
            for (var i = 0; i < 100; i++) {
                records.Add(new ObjectRequestRecord {
                    Id = i
                });
            }

            _repositoryMock.Setup(x => x.Table).Returns(records.AsQueryable());

            var result = _controller.Index(50);

            result.Should().BeOfType<ViewResult>();
            var model = result.As<ViewResult>().Model.As<IList<ObjectRequestRecord>>();
            model.Count.Should().Be(50);
            model.All(x => x.Id >= 50 && x.Id < 100).Should().BeTrue();
        }
    }
}