using Common.Log;
using Core.Models;
using Core.Repositories;
using Core.Services;
using Core.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringService.Test
{
    [TestClass]
    public class MonitiringJobTest
    {
        Mock<IMonitoringService> _mockMonitoringService;
        Mock<IBaseSettings> _mockBaseSettings;
        Mock<ISlackNotifier> _mockSlackNotifier;
        Mock<IApiMonitoringObjectRepository> _mockApiMonitoringObjectRepository;
        Mock<IApiHealthCheckErrorRepository> _mockApiHealthCheckErrorRepository;
        Mock<IIsAliveService> _mockIsAliveService;
        public int ExpirationDateInSeconds = 60;
        Mock<ILog> _mockLogger;

        [TestInitialize]
        public void Init()
        {
            _mockMonitoringService = new Mock<IMonitoringService>();
            _mockBaseSettings = new Mock<IBaseSettings>();
            _mockSlackNotifier = new Mock<ISlackNotifier>();
            _mockApiMonitoringObjectRepository = new Mock<IApiMonitoringObjectRepository>();
            _mockApiHealthCheckErrorRepository = new Mock<IApiHealthCheckErrorRepository>();
            _mockIsAliveService = new Mock<IIsAliveService>();
            _mockLogger = new Mock<ILog>();
        }

        [TestMethod]
        public async Task ExecuteJob_FiresNotificationForFailedJobs()
        {
            #region Arrange
            var repository = new List<IMonitoringObject>()
            {
                new MonitoringObject()
                {
                    ServiceName = "TestName1",
                    LastTime = DateTime.UtcNow.AddSeconds(-ExpirationDateInSeconds),
                    Version = "TestVersion"
                },
            };

            #region SetUpMocks

            _mockMonitoringService.Setup(x => x.GetCurrentSnapshot()).Returns(Task.FromResult((IEnumerable<IMonitoringObject>)repository));
            _mockBaseSettings.Setup(x => x.MaxTimeDifferenceInSeconds).Returns(ExpirationDateInSeconds);
            //_mockIsAliveService.Setup(x => x.GetStatusAsync(url, It.IsAny<CancellationToken>()))
            //    .Returns(Task.FromException<IApiStatusObject>(new OperationCanceledException()));

            var monitoringJob = GetMonitorJob();
            #endregion SetUpMocks

            #endregion Arrange

            #region Act

            await monitoringJob.CheckAPIs();
            await monitoringJob.CheckJobs();

            #endregion Act

            #region Assert

            _mockSlackNotifier.Verify(x => x.SendMonitorMsgAsync(It.IsAny<string>()));

            #endregion Assert
        }

        [TestMethod]
        public async Task ExecuteJob_FiresNotificationForFailedApis()
        {
            #region Arrange
            string url = "https://lykke.some-test.com/isalive";
            var repository = new List<IMonitoringObject>()
            {
                new MonitoringObject()
                {
                    ServiceName = "TestName1",
                    LastTime = DateTime.UtcNow.AddSeconds(-ExpirationDateInSeconds),
                    Url = url,
                    Version = "TestVersion"
                },
            };

            #region SetUpMocks

            _mockMonitoringService.Setup(x => x.GetCurrentSnapshot()).Returns(Task.FromResult((IEnumerable<IMonitoringObject>)repository));
            _mockBaseSettings.Setup(x => x.MaxTimeDifferenceInSeconds).Returns(ExpirationDateInSeconds);
            _mockIsAliveService.Setup(x => x.GetStatusAsync(url, It.IsAny<CancellationToken>()))
                .Returns(Task.FromException<IApiStatusObject>(new OperationCanceledException()));
            var monitoringJob = GetMonitorJob();

            #endregion SetUpMocks

            #endregion Arrange

            #region Act

            await monitoringJob.CheckAPIs();
            await monitoringJob.CheckJobs();

            #endregion Act

            #region Assert

            _mockSlackNotifier.Verify(x => x.SendMonitorMsgAsync(It.IsAny<string>()));

            #endregion Assert
        }

        [TestMethod]
        public async Task ExecuteJob_FiresNotificationForBothApisAndJobs()
        {
            #region Arrange
            string url = "https://lykke.some-test.com/isalive";
            var repository = new List<IMonitoringObject>()
            {
                new MonitoringObject()
                {
                    ServiceName = "TestName2",
                    LastTime = DateTime.UtcNow.AddSeconds(-ExpirationDateInSeconds),
                    Url = url,
                    Version = "TestVersion",
                },
                new MonitoringObject()
                {
                    ServiceName = "TestName1",
                    LastTime = DateTime.UtcNow.AddSeconds(-ExpirationDateInSeconds),
                    Version = "TestVersion",
                },
            };

            #region SetUpMocks

            _mockMonitoringService.Setup(x => x.GetCurrentSnapshot()).Returns(Task.FromResult((IEnumerable<IMonitoringObject>)repository));
            _mockBaseSettings.Setup(x => x.MaxTimeDifferenceInSeconds).Returns(ExpirationDateInSeconds);
            _mockIsAliveService.Setup(x => x.GetStatusAsync(url, It.IsAny<CancellationToken>()))
                .Returns(Task.FromException<IApiStatusObject>(new OperationCanceledException()));
            var monitoringJob = GetMonitorJob();

            #endregion SetUpMocks
            #endregion Arrange

            #region Act

            await monitoringJob.CheckAPIs();
            await monitoringJob.CheckJobs();

            #endregion Act

            #region Assert

            _mockSlackNotifier.Verify(x => x.SendMonitorMsgAsync(It.IsAny<string>()), Times.Exactly(2));

            #endregion Assert
        }

        [TestMethod]
        public async Task ExecuteJob_SkipMutedServices()
        {
            #region Arrange
            string url = "https://lykke.some-test.com/isalive";
            var repository = new List<IMonitoringObject>()
            {
                new MonitoringObject()
                {
                    ServiceName = "TestName2",
                    LastTime = DateTime.UtcNow.AddSeconds(-ExpirationDateInSeconds),
                    Url = url,
                    Version = "TestVersion",
                     SkipCheckUntil = DateTime.UtcNow.AddDays(1)
                },
                new MonitoringObject()
                {
                    ServiceName = "TestName1",
                    LastTime = DateTime.UtcNow.AddSeconds(-ExpirationDateInSeconds),
                    Version = "TestVersion",
                    SkipCheckUntil = DateTime.UtcNow.AddDays(1)
                },
            };

            #region SetUpMocks

            _mockMonitoringService.Setup(x => x.GetCurrentSnapshot()).Returns(Task.FromResult((IEnumerable<IMonitoringObject>)repository));
            _mockBaseSettings.Setup(x => x.MaxTimeDifferenceInSeconds).Returns(ExpirationDateInSeconds);
            var monitoringJob = GetMonitorJob();
            #endregion SetUpMocks

            #endregion Arrange

            #region Act

            await monitoringJob.CheckAPIs();
            await monitoringJob.CheckJobs();

            #endregion Act

            #region Assert

            _mockSlackNotifier.Verify(x => x.SendMonitorMsgAsync(It.IsAny<string>()), Times.Never);

            #endregion Assert
        }

        private MonitoringJob GetMonitorJob()
        {
            return new MonitoringJob(
                _mockMonitoringService.Object,
                _mockBaseSettings.Object,
                _mockSlackNotifier.Object,
                _mockApiMonitoringObjectRepository.Object,
                _mockApiHealthCheckErrorRepository.Object,
                _mockIsAliveService.Object,
                _mockLogger.Object);
        }
    }
}
