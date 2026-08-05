using MediatR;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Features.Accounting.Commands.RecordCashTransaction;

public class RecordCashTransactionCommandHandler : IRequestHandler<RecordCashTransactionCommand, Unit>
{
    private readonly IRepository<CashTransaction> _transactionRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IAccountingService _accountingService;
    private readonly IUnitOfWork _unitOfWork;

    public RecordCashTransactionCommandHandler(
        IRepository<CashTransaction> transactionRepository,
        IRepository<Account> accountRepository,
        IAccountingService accountingService,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _accountingService = accountingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RecordCashTransactionCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.EntityId, cancellationToken);
        if (account is null)
            throw new InvalidOperationException($"Account with Id {request.EntityId} not found.");

        CashTransaction transaction;

        if (request.Type == TransactionType.Deposit)
        {
            transaction = CashTransaction.Deposit(request.Amount, request.EntityId, request.UserId);
        }
        else
        {
            transaction = CashTransaction.Withdraw(request.Amount, request.EntityId, request.UserId);
        }

        transaction.TransactionDate = request.TransactionDate;

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _accountingService.RecalculateNetProfitAsync(request.EntityId, cancellationToken);

        return Unit.Value;
    }
}
