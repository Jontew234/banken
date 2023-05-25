using Platinum.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;
using Platinum.Areas.Identity.Data;
using System.Reflection.Emit;
using System.Xml;

namespace Platinum.Areas.Identity.Data;

public class MinDbContext : IdentityDbContext<Customer>
{
    public MinDbContext(DbContextOptions<MinDbContext> options)
        : base(options)
    {
    }
    public DbSet<Customer> customers { get; set; }
    public DbSet<BankAccount> bankAccounts { get; set; }
    public DbSet<Tran> transactions { get; set; }
    public DbSet<Loan> loans { get; set; }
    public DbSet<Message> messages { get; set; }

    public DbSet<Card> cards { get; set; }

    public DbSet<CardAccount> CardAccounts { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<LoanTerms> LoanTerms { get; set; }
    public DbSet<InvestmentAccount> InvestmentAccounts { get; set; }

    public DbSet<InvestmentAccountTransactions> InvestmentsAccountTransactions { get; set; }

    public DbSet<CurrentlyOwnedAssets> OwnedAssets { get; set; }

    public DbSet<BusinessAccount> BusinessAccounts { get; set; }

    public DbSet<Invoice> Invoices { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<InvestmentAccount>()
    .HasBaseType<BankAccount>();

        builder.Entity<Loan>()
        .HasOne(l => l.BankAccount)
        .WithMany(ba => ba.Loans)
        .HasForeignKey(l => l.BankAccountId)
        .OnDelete(DeleteBehavior.Restrict);

        // Configure one-to-many relationship between BankAccount and Loan (To)
        builder.Entity<Loan>()
            .HasOne(l => l.ToAccount)
            .WithMany(ba => ba.PayTo)
            .HasForeignKey(l => l.BankAccIdTo)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<InvestmentAccountTransactions>()
           .HasOne(x => x.InvestmentAccount)
           .WithMany()
           .HasForeignKey(x => x.InvestmentAccountId);



        builder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId);

        builder.Entity<Invoice>()
          .HasOne(i => i.Loan)
          .WithMany(c => c.Invoice)
          .HasForeignKey(i => i.LoansId);

        builder.Entity<InvestmentAccountTransactions>()
            .HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId);

        // denna är okej
        builder.Entity<CurrentlyOwnedAssets>()
          .HasKey(t => new { t.InvestmentAccountId, t.AssetId });



        builder.Entity<BankAccount>()
          .HasIndex(e => e.AccountNumber)
          .IsUnique();

        builder.Entity<Asset>()
          .HasIndex(e => e.Name)
          .IsUnique();

        builder.Entity<Card>()
        .HasIndex(e => e.CVV)
        .IsUnique();

        builder.Entity<Card>()
          .HasIndex(e => e.CardNumber)
          .IsUnique();

        builder.Entity<Customer>()
         .HasIndex(e => e.Email)
         .IsUnique();

        builder.Entity<CardAccount>()
      .HasKey(ca => new { ca.CardId, ca.BankAccountId });

        builder.Entity<CardAccount>()
            .HasOne(ca => ca.Card)
            .WithMany(c => c.CardAccounts)
            .HasForeignKey(ca => ca.CardId);

        builder.Entity<CardAccount>()
            .HasOne(ca => ca.BankAccount)
            .WithMany(b => b.CardAccounts)
            .HasForeignKey(ca => ca.BankAccountId).OnDelete(DeleteBehavior.NoAction);




        builder.Entity<Tran>()
    .HasOne(t => t.FromBankAccount)
    .WithMany()
    .HasForeignKey(t => t.FromBankAccountId)
    .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Tran>()
            .HasOne(t => t.ToBankAccount)
            .WithMany()
            .HasForeignKey(t => t.ToBankAccountId)
            .OnDelete(DeleteBehavior.NoAction);


        builder.Entity<Card>().HasOne(c => c.Customer)
            .WithMany(c => c.Cards)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);



        builder.Entity<BankAccount>()
            .HasOne(c => c.Customer)
            .WithMany(a => a.BankAccounts)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Loan>()
           .HasOne(c => c.User)
           .WithMany(a => a.Loans)
           .HasForeignKey(c => c.CustomerId)
           .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(c => c.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(c => c.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }


}
