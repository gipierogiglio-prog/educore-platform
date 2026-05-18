using Xunit;
using FluentAssertions;
using Giglio.EduCore.Financial.Domain.Entities;
using Giglio.EduCore.Financial.Domain.Enums;
using Giglio.EduCore.Financial.Infrastructure.Persistence.Repositories;

namespace EduCore.Financial.IntegrationTests;

public class ExpenseTests : TestBase
{
    [Fact]
    public async Task CreateExpenseCategory_ShouldPersist()
    {
        var repo = new ExpenseCategoryRepository(DbContext);
        var category = new ExpenseCategory("Água", "Conta de água mensal");
        await repo.AddAsync(category);

        var saved = await repo.GetByIdAsync(category.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Água");
        saved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateCategory_ShouldSetInactive()
    {
        var repo = new ExpenseCategoryRepository(DbContext);
        var category = new ExpenseCategory("Luz");
        await repo.AddAsync(category);

        category.Deactivate();
        await repo.UpdateAsync(category);

        var saved = await repo.GetByIdAsync(category.Id);
        saved!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateExpense_ShouldPersist()
    {
        var catRepo = new ExpenseCategoryRepository(DbContext);
        var expRepo = new ExpenseRepository(DbContext);

        var category = new ExpenseCategory("Internet");
        await catRepo.AddAsync(category);

        var expense = new Expense(category.Id, "Internet do mês", 129.90m, new DateTime(2026, 5, 15), "Provedor X");
        await expRepo.AddAsync(expense);

        var saved = await expRepo.GetByIdAsync(expense.Id);
        saved.Should().NotBeNull();
        saved!.Value.Should().Be(129.90m);
        saved.Status.Should().Be(ExpenseStatus.Pending);
        saved.ProviderName.Should().Be("Provedor X");
    }

    [Fact]
    public async Task PayExpense_ShouldUpdateStatus()
    {
        var catRepo = new ExpenseCategoryRepository(DbContext);
        var expRepo = new ExpenseRepository(DbContext);

        var category = new ExpenseCategory("Aluguel");
        await catRepo.AddAsync(category);

        var expense = new Expense(category.Id, "Aluguel do escritório", 3500.00m, new DateTime(2026, 5, 5));
        await expRepo.AddAsync(expense);

        expense.Pay(new DateTime(2026, 5, 3));
        await expRepo.UpdateAsync(expense);

        var saved = await expRepo.GetByIdAsync(expense.Id);
        saved!.Status.Should().Be(ExpenseStatus.Paid);
        saved.PaymentDate.Should().Be(new DateTime(2026, 5, 3));
    }

    [Fact]
    public async Task CancelExpense_ShouldUpdateStatus()
    {
        var catRepo = new ExpenseCategoryRepository(DbContext);
        var expRepo = new ExpenseRepository(DbContext);

        var category = new ExpenseCategory("Material");
        await catRepo.AddAsync(category);

        var expense = new Expense(category.Id, "Material de escritório", 250.00m, new DateTime(2026, 6, 1));
        await expRepo.AddAsync(expense);

        expense.Cancel();
        await expRepo.UpdateAsync(expense);

        var saved = await expRepo.GetByIdAsync(expense.Id);
        saved!.Status.Should().Be(ExpenseStatus.Cancelled);
    }

    [Fact]
    public async Task PayAlreadyPaidExpense_ShouldThrow()
    {
        var catRepo = new ExpenseCategoryRepository(DbContext);
        var expRepo = new ExpenseRepository(DbContext);

        var category = new ExpenseCategory("Seguro");
        await catRepo.AddAsync(category);

        var expense = new Expense(category.Id, "Seguro do prédio", 500.00m, new DateTime(2026, 7, 1));
        await expRepo.AddAsync(expense);

        expense.Pay(DateTime.UtcNow);
        await expRepo.UpdateAsync(expense);

        var saved = await expRepo.GetByIdAsync(expense.Id);
        var act = () => saved!.Pay(DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelPaidExpense_ShouldThrow()
    {
        var catRepo = new ExpenseCategoryRepository(DbContext);
        var expRepo = new ExpenseRepository(DbContext);

        var category = new ExpenseCategory("Marketing");
        await catRepo.AddAsync(category);

        var expense = new Expense(category.Id, "Campanha digital", 2000.00m, new DateTime(2026, 5, 1));
        await expRepo.AddAsync(expense);

        expense.Pay(DateTime.UtcNow);
        await expRepo.UpdateAsync(expense);

        var saved = await expRepo.GetByIdAsync(expense.Id);
        var act = () => saved!.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task MarkOverdue_ShouldUpdateStatus_WhenPending()
    {
        var catRepo = new ExpenseCategoryRepository(DbContext);
        var expRepo = new ExpenseRepository(DbContext);

        var category = new ExpenseCategory("Impostos");
        await catRepo.AddAsync(category);

        var expense = new Expense(category.Id, "Imposto mensal", 1500.00m, new DateTime(2026, 1, 15));
        await expRepo.AddAsync(expense);

        expense.MarkOverdue();
        await expRepo.UpdateAsync(expense);

        var saved = await expRepo.GetByIdAsync(expense.Id);
        saved!.Status.Should().Be(ExpenseStatus.Overdue);
    }

    [Fact]
    public async Task FilterExpenses_ByCategoryAndStatus()
    {
        var catRepo = new ExpenseCategoryRepository(DbContext);
        var expRepo = new ExpenseRepository(DbContext);

        var cat1 = new ExpenseCategory("Cat1");
        var cat2 = new ExpenseCategory("Cat2");
        await catRepo.AddAsync(cat1);
        await catRepo.AddAsync(cat2);

        var e1 = new Expense(cat1.Id, "Despesa 1", 100, new DateTime(2026, 5, 1));
        var e2 = new Expense(cat1.Id, "Despesa 2", 200, new DateTime(2026, 5, 15));
        var e3 = new Expense(cat2.Id, "Despesa 3", 300, new DateTime(2026, 5, 20));
        await expRepo.AddAsync(e1);
        await expRepo.AddAsync(e2);
        await expRepo.AddAsync(e3);

        // Pay e2
        e2.Pay(new DateTime(2026, 5, 10));
        await expRepo.UpdateAsync(e2);

        var cat1Expenses = await expRepo.GetAllAsync(categoryId: cat1.Id);
        cat1Expenses.Should().HaveCount(2);

        var paidExpenses = await expRepo.GetAllAsync(status: "Paid");
        paidExpenses.Should().HaveCount(1);
    }
}
