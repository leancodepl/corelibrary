using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using NSubstitute;
using Xunit;

namespace LeanCode.DomainModels.Tests
{
    public class IRepositoryExtensionsTests
    {
        private const int UserId = 1;
        private static readonly Id<DiscountCode> CodeId = Id<DiscountCode>.New();
        private static readonly DiscountCode Code = new DiscountCode
        {
            Id = CodeId,
        };
        private static readonly User User = new User
        {
            Id = UserId,
        };

        [Fact]
        public async Task Returns_entity_when_exists()
        {
            var repository = Substitute.For<IRepository<DiscountCode, Id<DiscountCode>>>();
            repository.FindAsync(CodeId).Returns(Code);

            var code = await repository.FindAndEnsureExistsAsync(CodeId);

            Assert.NotNull(code);
            Assert.Equal(CodeId, code.Id);
        }

        [Fact]
        public async Task Throws_when_entity_does_not_exist()
        {
            var repository = Substitute.For<IRepository<DiscountCode, Id<DiscountCode>>>();
            repository.FindAsync(CodeId).Returns(null as DiscountCode);

            await Assert.ThrowsAsync<EntityDoesNotExistException>(() => repository.FindAndEnsureExistsAsync(CodeId));
        }

        [Fact]
        public async Task Returns_entity_when_exists_with_int_as_id()
        {
            var repository = Substitute.For<IRepository<User, int>>();
            repository.FindAsync(UserId).Returns(User);

            var user = await repository.FindAndEnsureExistsAsync(UserId);

            Assert.NotNull(user);
            Assert.Equal(UserId, user.Id);
        }

        [Fact]
        public async Task Throws_when_entity_does_not_exist_with_int_as_id()
        {
            var repository = Substitute.For<IRepository<User, int>>();
            repository.FindAsync(UserId).Returns(null as User);

            await Assert.ThrowsAsync<EntityDoesNotExistException>(() => repository.FindAndEnsureExistsAsync(UserId));
        }
    }

    public class DiscountCode : IAggregateRoot<Id<DiscountCode>>
    {
        public Id<DiscountCode> Id { get; set; }

        DateTime IOptimisticConcurrency.DateModified { get; set; }
    }

    public class User : IAggregateRoot<int>
    {
        public int Id { get; set; }

        DateTime IOptimisticConcurrency.DateModified { get; set; }
    }
}
