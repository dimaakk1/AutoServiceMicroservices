using AutoServiceCatalog.DAL.Repositories.Intarfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoServiceCatalog.DAL.UOW;
using AutoServiceCatalog.DAL.Entities;

namespace AutoserviceCatalog.Tests.Helpers
{
    public static class MockUnitOfWorkFactory
    {
        public static (
            Mock<IUnitOfWork> UoW,
            Mock<IServiceRepository> Services
        ) Create()
        {
            var mockRepo = new Mock<IServiceRepository>();
            var mockUoW = new Mock<IUnitOfWork>();

            mockUoW.Setup(u => u.Services).Returns(mockRepo.Object);
            mockUoW.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            return (mockUoW, mockRepo);
        }
    }
}
