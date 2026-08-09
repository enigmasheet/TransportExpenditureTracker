using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IExpenseService
{
    Task<List<ExpenseHeaderViewModel>> GetAllAsync();
    Task<ExpenseEntryViewModel?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(ExpenseEntryViewModel vm, string userId);
    Task<bool> DeleteAsync(int id, string userId);
    Task<bool> IsDuplicateInvoiceAsync(string invoiceNo, int supplierId, int fiscalYearId, int? excludeId = null);
    Task<ImportSummaryViewModel> ImportCsvAsync(List<CsvRowViewModel> rows, string userId, bool autoCreate);
    Task<ImportSummaryViewModel> BatchCreateAsync(ExpenseBatchViewModel vm, string userId);
}
