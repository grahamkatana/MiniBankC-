// Repositories/TransactionRepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using MiniBank.Api.Data;
using MiniBank.Api.Repository;
using MiniBank.Tests.Helpers;

namespace MiniBank.Tests.Repositories
{
    public class TransactionRepositoryTests : IDisposable
    {
        private readonly ApplicationDBContext _context;
        private readonly TransactionRepository _repository;

        public TransactionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);
            _repository = new TransactionRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTransaction_WhenExists()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            var transaction = TestDataHelper.CreateTestTransaction(account.Id);
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(transaction.id);

            // Assert
            result.Should().NotBeNull();
            result!.Amount.Should().Be(500.00m);
        }

        [Fact]
        public async Task GetByAccountIdAsync_ReturnsAllTransactionsForAccount()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            var transaction1 = TestDataHelper.CreateTestTransaction(account.Id, 1);
            var transaction2 = TestDataHelper.CreateTestTransaction(account.Id, 2);
            await _context.Transactions.AddRangeAsync(transaction1, transaction2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByAccountIdAsync(account.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(t => t.FromAccountId.Should().Be(account.Id));
        }

        [Fact]
        public async Task GetByDateRangeAsync_ReturnsTransactionsInRange()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            var transaction1 = TestDataHelper.CreateTestTransaction(account.Id, 1);
            transaction1.TransactionDate = new DateTime(2026, 1, 5);
            var transaction2 = TestDataHelper.CreateTestTransaction(account.Id, 2);
            transaction2.TransactionDate = new DateTime(2026, 1, 15);
            var transaction3 = TestDataHelper.CreateTestTransaction(account.Id, 3);
            transaction3.TransactionDate = new DateTime(2026, 2, 1);

            await _context.Transactions.AddRangeAsync(transaction1, transaction2, transaction3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByDateRangeAsync(
                account.Id,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 31)
            );

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(t => t.TransactionDate.Month.Should().Be(1));
        }

        [Fact]
        public async Task CreateAsync_AddsTransactionToDatabase()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            var transaction = TestDataHelper.CreateTestTransaction(account.Id);

            // Act
            var result = await _repository.CreateAsync(transaction);

            // Assert
            result.Should().NotBeNull();
            result.id.Should().BeGreaterThan(0);
            var savedTransaction = await _context.Transactions.FindAsync(result.id);
            savedTransaction.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateStatusAsync_ChangesTransactionStatus()
        {
            // Arrange
            var account = TestDataHelper.CreateTestAccount();
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            var transaction = TestDataHelper.CreateTestTransaction(account.Id);
            transaction.Status = "Pending";
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.UpdateStatusAsync(transaction.id, "Completed");

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be("Completed");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTransactions()
        {
            // Arrange
            var account1 = TestDataHelper.CreateTestAccount("user1", 1);
            var account2 = TestDataHelper.CreateTestAccount("user2", 2);
            await _context.Accounts.AddRangeAsync(account1, account2);
            await _context.SaveChangesAsync();

            var transaction1 = TestDataHelper.CreateTestTransaction(account1.Id, 1);
            var transaction2 = TestDataHelper.CreateTestTransaction(account2.Id, 2);
            await _context.Transactions.AddRangeAsync(transaction1, transaction2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}