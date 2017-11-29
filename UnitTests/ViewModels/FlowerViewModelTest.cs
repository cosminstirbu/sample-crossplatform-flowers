using System;
using System.Net.Http;

using Flowers.ViewModel;
using Flowers.Model;

using NUnit.Framework;
using NSubstitute;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using FluentAssertions;
using GalaSoft.MvvmLight.Ioc;
using UnitTests.Http;

namespace UnitTests.ViewModels
{
    [TestFixture]
    public class FlowerViewModelTest
    {
        [SetUp]
        public void Setup()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register(()=> Substitute.For<INavigationService>());
            SimpleIoc.Default.Register(() => Substitute.For<IDialogService>());
            SimpleIoc.Default.Register(() => Substitute.For<IClock>());
        }

        [TearDown]
        public void TearDown()
        {
            SimpleIoc.Default.Reset();
        }

        [Test]
        public void SaveComment_BuildsRequest()
        {
            // Given
            var httpServer = GivenAHttpServer();
            var clock = GivenAClock();
            var flowerViewModel = GivenAFlowerViewModel(httpServer);

            var comment = "My Comment";
            var expectedEncodedComment = "My+Comment";

            long expectedTicks = 123456789;
            clock.Now().Returns(new DateTime(expectedTicks));

            // When
            var task = flowerViewModel.SaveComment(comment);

            // Then
            var request = httpServer.RequestsQueue.Peek().HttpRequest;

            request.RequestUri.AbsoluteUri.Should().Be("http://www.galasoft.ch/labs/Flowers/FlowersService.ashx?key=CB6344C121DB4099B484008370211418&action=save&ticks=123456789");
            request.Method.Should().Be(HttpMethod.Post);
            request.Content.ReadAsStringAsync().Result.Should().Contain(expectedEncodedComment);
        }

        [Test]
        public void SaveComment_IsSavingTrue_WhileRequestInProgress()
        {
            // Given
            var httpServer = GivenAHttpServer();
            var flowerViewModel = GivenAFlowerViewModel(httpServer);

            // When
            var task = flowerViewModel.SaveComment("Any Random String");

            // Then
            flowerViewModel.IsSaving.Should().BeTrue();
        }

        [Test]
        public void SaveComment_IsSavingFalse_WhenRequestFinishesSuccessfully()
        {
            // Given
            var httpServer = GivenAHttpServer();
            httpServer.ResponsesQueue.Enqueue(HttpServer.ResponseWithException.OK());

            var flowerViewModel = GivenAFlowerViewModel(httpServer);

            // When
            var task = flowerViewModel.SaveComment("Any Random String");
            httpServer.ConsumeRequests();
            task.Wait();

            // Then
            flowerViewModel.IsSaving.Should().BeFalse();
        }

        [Test]
        public void SaveComment_IsSavingFalse_WhenRequestThrowsException()
        {
            // Given
            var httpServer = GivenAHttpServer();
            httpServer.ResponsesQueue.Enqueue(HttpServer.ResponseWithException.WithException());

            var flowerViewModel = GivenAFlowerViewModel(httpServer);

            // When
            var task = flowerViewModel.SaveComment("Any Random String");
            httpServer.ConsumeRequests();
            task.Wait();

            // Then
            flowerViewModel.IsSaving.Should().BeFalse();
        }

        [Test]
        public void SaveComment_TriggersBackNavigation_WhenRequestFinishesSuccessfully()
        {
            // Given
            var navigationService = GivenANavigationService();
            var httpServer = GivenAHttpServer();
            httpServer.ResponsesQueue.Enqueue(HttpServer.ResponseWithException.OK());

            var flowerViewModel = GivenAFlowerViewModel(httpServer);

            // When
            var task = flowerViewModel.SaveComment("Any Random String");
            httpServer.ConsumeRequests();
            task.Wait();

            // Then
            navigationService.Received().GoBack();
        }

        [Test]
        public void SaveComment_DisplaysError_WhenRequestThrowsException()
        {
            // Given
            var dialogService = GivenADialogService();
            var httpServer = GivenAHttpServer();
            httpServer.ResponsesQueue.Enqueue(HttpServer.ResponseWithException.WithException());

            var flowerViewModel = GivenAFlowerViewModel(httpServer);

            // When
            var task = flowerViewModel.SaveComment("Any Random String");
            httpServer.ConsumeRequests();
            task.Wait();

            // Then
            dialogService.Received().ShowError("Error when saving, your comment was not saved", "Error", "OK", null);
        }

        private INavigationService GivenANavigationService()
        {
            return ServiceLocator.Current.GetInstance<INavigationService>();
        }

        private IDialogService GivenADialogService()
        {
            return ServiceLocator.Current.GetInstance<IDialogService>();
        }

        private HttpServer GivenAHttpServer()
        {
            return new HttpServer();
        }

        private Flower GivenAFlower()
        {
            return new Flower();
        }

        private IClock GivenAClock()
        {
            return ServiceLocator.Current.GetInstance<IClock>();
        }

        private FlowerViewModel GivenAFlowerViewModel(HttpServer server, Flower flower = null, IClock clock = null)
        {
            flower = flower ?? GivenAFlower();
            clock = clock ?? GivenAClock();

            return new FlowerViewModel(new FlowersService(new HttpClient(server), clock), flower);
        }
    }
}
