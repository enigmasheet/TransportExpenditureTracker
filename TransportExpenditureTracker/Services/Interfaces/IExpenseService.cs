using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IExpenseService
{
    Task<List<ExpenseHeaderViewModel>> GetAllAsync();
    Task<ExpenseEntryViewModel?> GetByIdAsync(int id);
    Task AddAsync(ExpenseEntryViewModel vm, string userId);
    Task UpdateAsync(ExpenseEntryViewModel vm, string userId);
    Task DeleteAsync(int id, string userId);
    Task<bool> IsDuplicateInvoiceAsync(string invoiceNo, int supplierId);
    Task<ImportSummaryViewModel> ImportCsvAsync(List<CsvRowViewModel> rows, string userId, bool autoCreate);
}
