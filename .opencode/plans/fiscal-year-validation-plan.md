# Implementation Plan: Fiscal Year Scoped Invoice Validation + Enhanced Validation

## Overview
Add fiscal-year-scoped duplicate invoice validation, server-side VAT/Total calculation, enhanced CSV import validation, single-file .exe publish, and Dockerfile containerization.

---

## Step 1: FiscalYearHelper
**New file:** `Helper/FiscalYearHelper.cs`

```csharp
namespace TransportExpenditureTracker.Helper;

public static class FiscalYearHelper
{
    // Nepal FY: Shrawan (month 4) to Ashad (month 3)
    // Months 4-12  → FY "N/N+1"  (e.g., 2082/83)
    // Months 1-3   → FY "N-1/N"  (e.g., 2081/82)
    public static FiscalYear? GetFiscalYear(string miti, List<FiscalYear> fiscalYears)
    {
        var parts = miti.Split('/');
        if (parts.Length != 3) return null;
        var nepaliYear = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[0]));
        var nepaliMonth = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[1]));
        string fyName = nepaliMonth >= 4 ? $"{nepaliYear}/{nepaliYear+1}" : $"{nepaliYear-1}/{nepaliYear}";
        return fiscalYears.FirstOrDefault(f => f.Name == fyName);
    }

    public static string GetFiscalYearName(string miti)
    {
        var parts = miti.Split('/');
        if (parts.Length != 3) return string.Empty;
        var nepaliYear = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[0]));
        var nepaliMonth = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[1]));
        return nepaliMonth >= 4 ? $"{nepaliYear}/{nepaliYear+1}" : $"{nepaliYear-1}/{nepaliYear}";
    }
}
```

---

## Step 2: Add FiscalYearId FK to ExpenseHeader
**File:** `Models/ExpenseHeader.cs`

Add:
```csharp
public int FiscalYearId { get; set; }
public FiscalYear FiscalYear { get; set; } = null!;
```

---

## Step 3: Update ApplicationDbContext
**File:** `Data/ApplicationDbContext.cs`

Changes:
1. Add FK config: `ExpenseHeader → FiscalYear` with `OnDelete(DeleteBehavior.NoAction)`
2. Update unique index from `(InvoiceNo, SupplierId)` to `(InvoiceNo, SupplierId, FiscalYearId)`

```csharp
builder.Entity<ExpenseHeader>()
    .HasIndex(e => new { e.InvoiceNo, e.SupplierId, e.FiscalYearId })
    .IsUnique();

builder.Entity<ExpenseHeader>()
    .HasOne(e => e.FiscalYear)
    .WithMany()
    .HasForeignKey(e => e.FiscalYearId)
    .OnDelete(DeleteBehavior.NoAction)
    .IsRequired();
```

---

## Step 4: Update ExpenseEntryViewModel
**File:** `ViewModels/ExpenseEntryViewModel.cs`

Add:
```csharp
[Required(ErrorMessage = "Fiscal year is required")]
public int FiscalYearId { get; set; }
```

---

## Step 5: Update ExpenseDetailViewModel
**File:** `ViewModels/ExpenseDetailViewModel.cs`

No structural change needed, but server-side will recalculate and validate these fields.

---

## Step 6: Update CsvRowViewModel
**File:** `ViewModels/CsvRowViewModel.cs`

Add:
```csharp
public int FiscalYearId { get; set; }
public string? FiscalYearName { get; set; }
public bool IsVatMismatch { get; set; }
public bool IsTotalMismatch { get; set; }
public bool IsCrossRowDuplicate { get; set; }
```

---

## Step 7: Update IExpenseService
**File:** `Services/Interfaces/IExpenseService.cs`

Change signature:
```csharp
Task<bool> IsDuplicateInvoiceAsync(string invoiceNo, int supplierId, int fiscalYearId, int? excludeId = null);
```

---

## Step 8: Update ExpenseService
**File:** `Services/ExpenseService.cs`

### 8a. Update `IsDuplicateInvoiceAsync`
```csharp
public async Task<bool> IsDuplicateInvoiceAsync(string invoiceNo, int supplierId, int fiscalYearId, int? excludeId = null)
{
    var query = _db.ExpenseHeaders.AsNoTracking()
        .Where(h => h.InvoiceNo == invoiceNo && h.SupplierId == supplierId && h.FiscalYearId == fiscalYearId);
    if (excludeId.HasValue)
        query = query.Where(h => h.ExpenseId != excludeId.Value);
    return await query.AnyAsync();
}
```

### 8b. Server-side VAT/Total calculation in `AddAsync` and `UpdateAsync`
After mapping ViewModel → Entity, recalculate and validate:
```csharp
foreach (var d in vm.Details)
{
    // Recalculate from qty * rate if taxable amount is 0
    var taxable = d.TaxableAmount > 0 ? d.TaxableAmount : d.Quantity * d.Rate;
    var vat = taxable * 0.13m;
    var total = taxable + vat;

    // Validate against submitted values (tolerance of 1.0)
    if (Math.Abs(taxable - d.TaxableAmount) > 0.5m || Math.Abs(vat - d.VatAmount) > 0.5m || Math.Abs(total - d.TotalAmount) > 0.5m)
    {
        // Use computed values as authoritative
        // Log warning or adjust silently
    }

    detail.TaxableAmount = Math.Round(taxable, 2);
    detail.VatAmount = Math.Round(vat, 2);
    detail.TotalAmount = Math.Round(total, 2);
}
```

### 8c. Update `ImportCsvAsync` to handle FiscalYearId
After looking up Supplier/Item, also look up FiscalYear:
```csharp
var fiscalYearName = FiscalYearHelper.GetFiscalYearName(row.Miti);
var fiscalYear = await _db.FiscalYears.FirstOrDefaultAsync(f => f.Name == fiscalYearName);
if (fiscalYear == null) { /* skip row */ }
```

Remove raw `FiscalYear` string assignment in header creation, add:
```csharp
FiscalYearId = fiscalYear.Id
```

---

## Step 9: Rewrite CsvImportService
**File:** `Services/CsvImportService.cs`

### 9a. Enhanced Preview Validation
Add per-row validations in `PreviewAsync`:

```csharp
// After basic required-field checks...
if (string.IsNullOrEmpty(row.ValidationError))
{
    // 1. Parse fiscal year from Miti
    var fiscalYearName = FiscalYearHelper.GetFiscalYearName(row.Miti);
    if (string.IsNullOrEmpty(fiscalYearName))
        row.ValidationError = "Invalid Miti format";
    else
    {
        var fiscalYear = await _db.FiscalYears.FirstOrDefaultAsync(f => f.Name == fiscalYearName);
        if (fiscalYear is null)
            row.ValidationError = $"Fiscal year '{fiscalYearName}' not found in system";
        else
        {
            row.FiscalYearId = fiscalYear.Id;
            row.FiscalYearName = fiscalYear.Name;

            // 2. Check duplicate in DB (existing)
            var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.SupplierName == row.SupplierName);
            if (supplier is not null)
                row.IsDuplicate = await _expenseService.IsDuplicateInvoiceAsync(row.InvoiceNo, supplier.SupplierId, fiscalYear.Id);
        }
    }

    // 3. VAT mismatch check (warning, not hard error)
    if (row.TaxableAmount > 0 && row.VatAmount > 0)
    {
        var expectedVat = row.TaxableAmount * 0.13m;
        if (Math.Abs(row.VatAmount - expectedVat) > 1.0m)
            row.IsVatMismatch = true;
    }

    // 4. Total mismatch check
    if (row.TaxableAmount > 0 && row.VatAmount > 0 && row.TotalAmount > 0)
    {
        var expectedTotal = row.TaxableAmount + row.VatAmount;
        if (Math.Abs(row.TotalAmount - expectedTotal) > 1.0m)
            row.IsTotalMismatch = true;
    }
}

// 5. Cross-row duplicate check within the same CSV file
var seenKeys = new HashSet<string>();
foreach (var row in rows)
{
    if (string.IsNullOrEmpty(row.ValidationError))
    {
        var key = $"{row.InvoiceNo}|{row.SupplierName}|{row.FiscalYearName}";
        if (!seenKeys.Add(key))
        {
            row.IsCrossRowDuplicate = true;
            row.ValidationError = "Duplicate invoice in same import file";
        }
    }
}
```

### 9b. Update `ImportAsync` to include warnings in result
```csharp
summary.WarningCount = rows.Count(r => r.IsVatMismatch || r.IsTotalMismatch);
```

---

## Step 10: Update ImportSummaryViewModel
**File:** `ViewModels/ImportSummaryViewModel.cs`

Add:
```csharp
public int WarningCount { get; set; }
public List<string> Warnings { get; set; } = [];
```

---

## Step 11: Update DropdownHelper
**File:** `Helper/DropdownHelper.cs` (read first to see existing pattern)

Add a method to load fiscal years:
```csharp
public static void LoadFiscalYears(ApplicationDbContext ctx, ViewDataDictionary viewData, int? selectedId = null)
{
    var fiscalYears = ctx.FiscalYears.OrderByDescending(f => f.Name).ToList();
    viewData["FiscalYears"] = new SelectList(fiscalYears, "Id", "Name", selectedId);
}
```

---

## Step 12: Update ExpensesController
**File:** `Controllers/ExpensesController.cs`

### 12a. Create (GET) — load fiscal years
```csharp
public IActionResult Create()
{
    DropdownHelper.LoadFiscalYears(_ctx, ViewData); // ADD
    DropdownHelper.LoadSuppliers(_ctx, ViewData);
    // ...rest unchanged
}
```

### 12b. Create (POST) — pass FiscalYearId to duplicate check
```csharp
if (await _expenseService.IsDuplicateInvoiceAsync(vm.InvoiceNo, vm.SupplierId, vm.FiscalYearId))
{
    ModelState.AddModelError(nameof(vm.InvoiceNo), "An expense with this invoice number already exists for the selected supplier in this fiscal year.");
}
```

### 12c. Edit (GET) — load fiscal years
```csharp
DropdownHelper.LoadFiscalYears(_ctx, ViewData, entryVm.FiscalYearId);
```

### 12d. Edit (POST) — pass FiscalYearId to duplicate check
```csharp
if (await _expenseService.IsDuplicateInvoiceAsync(vm.InvoiceNo, vm.SupplierId, vm.FiscalYearId, vm.ExpenseId))
```

### 12e. Import POST — preserve warnings data
```csharp
TempData["HasWarnings"] = preview.Rows.Any(r => r.IsVatMismatch || r.IsTotalMismatch);
TempData["Warnings"] = JsonSerializer.Serialize(preview.Rows.Where(r => r.IsVatMismatch || r.IsTotalMismatch).Select(r => $"Row {r.RowIndex}: {(r.IsVatMismatch ? "VAT mismatch" : "")} {(r.IsTotalMismatch ? "Total mismatch" : "")}"));
```

---

## Step 13: Update Views

### 13a. `Views/Expenses/Create.cshtml`
Add Fiscal Year dropdown after Remarks field:
```html
<div class="mb-3">
    <label asp-for="FiscalYearId" class="form-label"></label>
    <select asp-for="FiscalYearId" asp-items="ViewData["FiscalYears"] as SelectList" class="form-select">
        <option value="">-- Select Fiscal Year --</option>
    </select>
    <span asp-validation-for="FiscalYearId" class="text-danger"></span>
</div>
```

### 13b. `Views/Expenses/Edit.cshtml`
Same as Create.

### 13c. `Views/Expenses/Import.cshtml`
Add fiscal year column in preview table:
```html <td>@r.FiscalYearName</td>
```
Add warning badges for VAT/Total mismatch:
```html
@if (r.IsVatMismatch) { <span class="badge bg-warning">VAT</span> }
@if (r.IsTotalMismatch) { <span class="badge bg-warning">Total</span> }
```
Add warning alert box before import button.

---

## Step 14: Update .csproj
**File:** `TransportExpenditureTracker.csproj`

Add under `<PropertyGroup>`:
```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
```

---

## Step 15: Create Dockerfile
**New file:** `Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o /app

FROM mcr.microsoft.com/windows/nanoserver:ltsc2022 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["TransportExpenditureTracker.exe"]
```

---

## Step 16: Create EF Migration
```bash
dotnet ef migrations add AddFiscalYearIdToExpenseHeader
```

Then update `Up` method to backfill existing records:
```csharp
migrationBuilder.Sql(@"
    UPDATE ExpenseHeaders 
    SET FiscalYearId = (
        SELECT Id FROM FiscalYears 
        WHERE FiscalYears.Name = CASE 
            WHEN CAST(substr(Miti, 4, 2) AS INTEGER) >= 4 
            THEN printf('%d/%d', CAST(substr(Miti, 1, 4) AS INTEGER), CAST(substr(Miti, 1, 4) AS INTEGER) + 1)
            ELSE printf('%d/%d', CAST(substr(Miti, 1, 4) AS INTEGER) - 1, CAST(substr(Miti, 1, 4) AS INTEGER))
        END
    )
    WHERE FiscalYearId = 0;
");
```

---

## Step 17: Build Verification
```bash
dotnet build
```

---

## Summary of All Changes

| # | File | Action | Summary |
|---|------|--------|---------|
| 1 | `Helper/FiscalYearHelper.cs` | **CREATE** | Map Miti → FiscalYear (Nepal standard: months 4-3 boundary) |
| 2 | `Models/ExpenseHeader.cs` | EDIT | Add `FiscalYearId` FK + nav property |
| 3 | `Data/ApplicationDbContext.cs` | EDIT | Add FK config, update unique index |
| 4 | `ViewModels/ExpenseEntryViewModel.cs` | EDIT | Add `FiscalYearId` with Required attribute |
| 5 | `ViewModels/CsvRowViewModel.cs` | EDIT | Add `FiscalYearId, FiscalYearName, IsVatMismatch, IsTotalMismatch, IsCrossRowDuplicate` |
| 6 | `ViewModels/ImportSummaryViewModel.cs` | EDIT | Add `WarningCount, Warnings` |
| 7 | `Services/Interfaces/IExpenseService.cs` | EDIT | Update `IsDuplicateInvoiceAsync` sig to include `fiscalYearId` |
| 8 | `Services/ExpenseService.cs` | EDIT | Fiscal-year-scoped duplicate check, server-side VAT/Total calc, FiscalYearId in import |
| 9 | `Services/CsvImportService.cs` | EDIT | Enhanced validation, cross-row dup check, VAT/Total mismatch |
| 10 | `Helper/DropdownHelper.cs` | EDIT | Add `LoadFiscalYears()` method |
| 11 | `Controllers/ExpensesController.cs` | EDIT | Load fiscal years, pass to duplicate check |
| 12 | `Views/Expenses/Create.cshtml` | EDIT | Add Fiscal Year dropdown |
| 13 | `Views/Expenses/Edit.cshtml` | EDIT | Add Fiscal Year dropdown |
| 14 | `Views/Expenses/Import.cshtml` | EDIT | Add FY column + warning badges |
| 15 | `TransportExpenditureTracker.csproj` | EDIT | Single-file self-contained config |
| 16 | `Dockerfile` | **CREATE** | Multi-stage Docker build |
| 17 | DB Migration | RUN | `AddFiscalYearIdToExpenseHeader` |