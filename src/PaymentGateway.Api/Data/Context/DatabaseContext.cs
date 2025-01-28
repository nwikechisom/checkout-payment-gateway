using Microsoft.EntityFrameworkCore;

using PaymentGateway.Api.Data.Models;

namespace PaymentGateway.Api.Data.Context;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    #region DbSets
    
    public DbSet<Transaction> Transactions { get; init; }
    
    #endregion
}