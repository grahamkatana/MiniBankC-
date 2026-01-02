// Repositories/AccountRepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using MiniBank.Api.Data;
using MiniBank.Api.Repository;
using MiniBank.Tests.Helpers;

namespace MiniBank.Tests.Repositories
{
    public class AccountRepositoryTests : IDisposable
    {
        private readonly ApplicationDBContext _context;
        private readonly AccountRepository _repository;

        public AccountRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);
            _repository = new AccountRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsAccount_WhenExists()
        {
            // Arrange
            var user = TestDataHelper.CreateTestUser();
            await _context.Users.AddAsync(user);
            
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(account.Id);

            // Assert
            result.Should().NotBeNull();
            result!.AccountNumber.Should().Be(account.AccountNumber);
            result.Balance.Should().Be(account.Balance);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByAccountNumberAsync_ReturnsAccount_WhenExists()
        {
            // Arrange
            var user = TestDataHelper.CreateTestUser();
            await _context.Users.AddAsync(user);
            
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByAccountNumberAsync(account.AccountNumber);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(account.Id);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsAllUserAccounts()
        {
            // Arrange
            var user1 = TestDataHelper.CreateTestUser("test-user");
            var user2 = TestDataHelper.CreateTestUser("other-user", "other@example.com");
            await _context.Users.AddRangeAsync(user1, user2);
            
            var account1 = TestDataHelper.CreateTestAccount("test-user", 1);
            var account2 = TestDataHelper.CreateTestAccount("test-user", 2);
            var account3 = TestDataHelper.CreateTestAccount("other-user", 3);

            await _context.Accounts.AddRangeAsync(account1, account2, account3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync("test-user");

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(a => a.UserId.Should().Be("test-user"));
        }

        [Fact]
        public async Task CreateAsync_AddsAccountToDatabase()
        {
            // Arrange
            var user = TestDataHelper.CreateTestUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            var account = TestDataHelper.CreateTestAccount();

            // Act
            var result = await _repository.CreateAsync(account);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            var savedAccount = await _context.Accounts.FindAsync(result.Id);
            savedAccount.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_ModifiesAccount()
        {
            // Arrange
            var user = TestDataHelper.CreateTestUser();
            await _context.Users.AddAsync(user);
            
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            account.Balance = 2000.00m;
            account.AccountType = "Savings";

            // Act
            var result = await _repository.UpdateAsync(account.Id, account);

            // Assert
            result.Should().NotBeNull();
            result!.Balance.Should().Be(2000.00m);
            result.AccountType.Should().Be("Savings");
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenAccountDoesNotExist()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();

            // Act
            var result = await _repository.UpdateAsync(999, account);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_RemovesAccount()
        {
            // Arrange
            var user = TestDataHelper.CreateTestUser();
            await _context.Users.AddAsync(user);
            
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(account.Id);

            // Assert
            result.Should().NotBeNull();
            var deletedAccount = await _context.Accounts.FindAsync(account.Id);
            deletedAccount.Should().BeNull();
        }

        [Fact]
        public async Task AccountExistsAsync_ReturnsTrue_WhenExists()
        {
            // Arrange
            var user = TestDataHelper.CreateTestUser();
            await _context.Users.AddAsync(user);
            
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AccountExistsAsync(account.AccountNumber);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AccountExistsAsync_ReturnsFalse_WhenDoesNotExist()
        {
            // Act
            var result = await _repository.AccountExistsAsync("NONEXISTENT");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetBalanceAsync_ReturnsBalance()
        {
            // Arrange
            var user = TestDataHelper.CreateTestUser();
            await _context.Users.AddAsync(user);
            
            var account = TestDataHelper.CreateTestAccount();
            account.Balance = 2500.00m;
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBalanceAsync(account.Id);

            // Assert
            result.Should().Be(2500.00m);
        }

        [Fact]
        public async Task GetBalanceAsync_ReturnsZero_WhenAccountDoesNotExist()
        {
            // Act
            var result = await _repository.GetBalanceAsync(999);

            // Assert
            result.Should().Be(0);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}