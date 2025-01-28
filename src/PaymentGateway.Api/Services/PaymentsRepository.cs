using System.Linq.Expressions;
using PaymentGateway.Api.Data.Context;
using PaymentGateway.Api.Data.Models;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository(DatabaseContext context) : IPaymentsRepository
{
    // public List<PostPaymentResponse> Payments = new();
    
    public async Task<Transaction> Add(Transaction transaction)
    {
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        return transaction;
    }

    public async Task Update(Transaction entity)
    {
        context.Transactions.Update(entity);
        await context.SaveChangesAsync();
    }

    public IEnumerable<Transaction> Find(Expression<Func<Transaction, bool>> expression)
    {
        return context.Transactions.Where(expression);
    }

    public Transaction Get(Guid id)
    {
        return context.Transactions.FirstOrDefault(p => p.Id == id)!;
    }
}