using System.Linq.Expressions;

using PaymentGateway.Api.Data.Models;

namespace PaymentGateway.Api.Services;

public interface IPaymentsRepository
{
    Task<Transaction> Add(Transaction entity);
    Task Update(Transaction entity);
    IEnumerable<Transaction> Find(Expression<Func<Transaction, bool>> expression);
    Transaction Get(Guid id);
}