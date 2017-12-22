//using Common.Log;
//using Core.Models;
//using Core.Repositories;
//using Core.Services;
//using Core.Settings;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Services;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace MonitoringService.Test
//{
//    [TestClass]
//    public class MonitiringServiceTest
//    {
//        Mock<IBaseSettings> _mockBaseSettings;
//        Mock<IMonitoringObjectRepository> _mockMonitoringObjectRepository;
//        Mock<IApiMonitoringObjectRepository> _mockApiMonitoringObjectRepository;
//        public int ExpirationDateInSeconds = 60;
//        Mock<ILog> _mockLogger;

//        [TestInitialize]
//        public void Init()
//        {
//            _mockBaseSettings = new Mock<IBaseSettings>();
//            _mockMonitoringObjectRepository = new Mock<IMonitoringObjectRepository>();
//            _mockApiMonitoringObjectRepository = new Mock<IApiMonitoringObjectRepository>();
//            _mockLogger = new Mock<ILog>();
//        }

//        [TestMethod]
//        public async Task MonitiringServiceTest()
//        {
//            #region Arrange
//            IEnumerable<IMonitoringObject> repository = new List<IMonitoringObject>()
//            {
//                new MonitoringObject()
//                {
//                    ServiceName = "TestName1",
//                    LastTime = DateTime.UtcNow.AddSeconds(-ExpirationDateInSeconds),
//                    Version = "TestVersion"
//                },
//            };

//            #region SetUpMocks
//            _mockMonitoringObjectRepository.Setup(x => x.GetAll()).Returns(Task.FromResult(repository));
//            var monitoringService = GetMonitoringService();
//            #endregion SetUpMocks

//            #endregion Arrange

//            #region Act

//            await monitoringService.();

//            #endregion Act

//            #region Assert

//            _mockSlackNotifier.Verify(x => x.ErrorAsync(It.IsAny<string>()));

//            #endregion Assert
//        }


//        private Services.MonitoringService GetMonitoringService()
//        {
//            return new Services.MonitoringService(_mockMonitoringObjectRepository.Object,
//                _mockApiMonitoringObjectRepository.Object);
//        }
//    }
//}
