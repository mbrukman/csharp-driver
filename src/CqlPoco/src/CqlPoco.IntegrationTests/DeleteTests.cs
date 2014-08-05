using System;
using System.Threading.Tasks;
using CqlPoco.IntegrationTests.Pocos;
using CqlPoco.IntegrationTests.TestData;
using FluentAssertions;
using NUnit.Framework;

namespace CqlPoco.IntegrationTests
{
    [TestFixture]
    public class DeleteTests : IntegrationTestBase
    {
        [Test]
        public async void Delete_Poco()
        {
            // Get an existing user from the DB
            Guid userId = TestDataHelper.Users[0].UserId;
            var userToDelete = await CqlClient.SingleAsync<UserWithPrimaryKeyDecoration>("WHERE userid = ?", userId);
            userToDelete.Should().NotBeNull();

            // Delete
            await CqlClient.DeleteAsync(userToDelete);

            // Verify user is gone
            var foundUser = await CqlClient.SingleOrDefaultAsync<UserWithPrimaryKeyDecoration>("WHERE userid = ?", userId);
            foundUser.Should().BeNull();
        }

        [Test]
        public async void Delete_Poco_NoPrimaryKey()
        {
            // Get an existing user from the DB that doesn't have a PK attribute defined on the POCO (i.e. InsertUser class)
            Guid userId = TestDataHelper.Users[1].UserId;
            var userToDelete = await CqlClient.SingleAsync<InsertUser>("WHERE userid = ?", userId);
            userToDelete.Should().NotBeNull();

            Func<Task> deleteUser = async () =>
            {
                await CqlClient.DeleteAsync(userToDelete);
            };

            deleteUser.ShouldThrow<InvalidOperationException>("no PK was specified and the assumed PK of 'id' is not a column on the POCO");
        }

        [Test]
        public async void Delete_Poco_WithCql()
        {
            // Pick an existing user from the DB and verify they currently exist (sanity check)
            Guid userId = TestDataHelper.Users[2].UserId;
            var userToDelete = await CqlClient.SingleAsync<UserWithPrimaryKeyDecoration>("WHERE userid = ?", userId);
            userToDelete.Should().NotBeNull();

            // Delete using CQL string (no POCO passed here, just CQL + params)
            await CqlClient.DeleteAsync<UserWithPrimaryKeyDecoration>("WHERE userid = ?", userId);

            // Verify user is actually deleted
            var foundUser = await CqlClient.SingleOrDefaultAsync<UserWithPrimaryKeyDecoration>("WHERE userid = ?", userId);
            foundUser.Should().BeNull();
        }
    }
}