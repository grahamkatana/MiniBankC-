using MiniBank.Api.Dtos.Transaction;

namespace MiniBank.Api.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionDto?> GetByIdAsync(int id);
        Task<List<TransactionDto>> GetByAccountNumberAsync(string accountNumber);
        Task<List<TransactionDto>> GetByDateRangeAsync(string accountNumber, DateTime startDate, DateTime endDate);
        Task<TransactionDto> DepositAsync(DepositDto depositDto);
        Task<TransactionDto> WithdrawAsync(WithdrawDto withdrawDto);
        Task<TransactionDto> TransferAsync(TransferDto transferDto);
        Task<List<TransactionDto>> GetAllAsync();
    }
}