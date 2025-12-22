using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BookShop.API.Infrastructure.Persistence;

namespace BookShop.Tests.Unit.MongoConnection
{
    public class MongoDbConnectionTest
    {
        private readonly Mock<ILogger<MongoDbConnectionTest>> _mockLogger = new Mock<ILogger<MongoDbConnectionTest>>();

        #region Constructor Tests

        [Fact]
        public void MongoDbConnection_ValidSettings_ShouldInitializeSuccessfully()
        {
            // Arrange
            var settings = Options.Create(LoadSettingsFromUserSecrets());

            MongoDbContext context = new MongoDbContext(settings, _mockLogger.Object);
        }


        #endregion Constructor Tests


    }
}
