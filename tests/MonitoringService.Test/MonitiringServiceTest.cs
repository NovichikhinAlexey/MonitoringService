using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Repositories;
using Lykke.MonitoringServiceApiCaller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MonitoringService.Test
{
    [TestClass]
    public class MonitiringServiceTest
    {
        Mock<IMonitoringObjectRepository> _mockMonitoringObjectRepository;
        Mock<IApiMonitoringObjectRepository> _mockApiMonitoringObjectRepository;

        public int ExpirationDateInSeconds = 60;

        [TestInitialize]
        public void Init()
        {
            _mockMonitoringObjectRepository = new Mock<IMonitoringObjectRepository>();
            _mockApiMonitoringObjectRepository = new Mock<IApiMonitoringObjectRepository>();
        }

        [TestMethod]
        public async Task ServiceTest()
        {
            #region Arrange

            string serviceName = "TestName1";

            IEnumerable<IMonitoringObject> repository = new List<IMonitoringObject>()
            {
                new MonitoringObject()
                {
                    ServiceName = serviceName,
                    LastTime = DateTime.UtcNow.AddSeconds(-ExpirationDateInSeconds),
                    Version = "TestVersion"
                },
            };

            #region SetUpMocks
            _mockMonitoringObjectRepository.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(repository));
            var monitoringService = new Services.MonitoringService(_mockMonitoringObjectRepository.Object, _mockApiMonitoringObjectRepository.Object);
            #endregion SetUpMocks

            #endregion Arrange

            #region Act

            var objs = await monitoringService.GetCurrentSnapshotAsync();

            #endregion Act

            #region Assert

            Assert.IsNotNull(objs);
            Assert.AreEqual(1, objs.Count());
            Assert.IsTrue(objs.Any(o => o.ServiceName == serviceName));

            #endregion Assert
        }

        [TestMethod]
        [Ignore("Integration")]
        public async Task IntegrationTest()
        {
            var facade = new MonitoringServiceFacade("http://monitoring-service.lykke-service.svc.cluster.local");
            var objs = await facade.GetAll();
            Assert.IsNotNull(objs);
        }
    }
}
