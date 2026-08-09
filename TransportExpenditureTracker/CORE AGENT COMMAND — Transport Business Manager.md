# CORE AGENT COMMAND — Transport Business Manager

Build a production-quality **portable Windows desktop application** named **Transport Business Manager** for a Nepal-based goods transport business.

## 1. Technology Requirements

Use:

- .NET 10
- WPF
- MVVM
- Clean Architecture
- Entity Framework Core 10
- SQLite
- Dependency Injection
- `Microsoft.Extensions.Hosting`
- FluentValidation
- Serilog
- ClosedXML for Excel export
- QuestPDF or an equivalent PDF library
- Windows DPAPI / Windows Credential Manager for secrets

The application must be **offline-first**.

Do NOT introduce:
- SQL Server
- PostgreSQL
- Redis
- Docker
- Cloud database
- Web API
- External server dependency

The application must work without internet except for email backup functionality.

---

# 2. Solution Structure

Create:

```text
TransportBusinessManager.sln

src/
├── TransportBusinessManager.App
├── TransportBusinessManager.Domain
├── TransportBusinessManager.Application
├── TransportBusinessManager.Infrastructure
└── TransportBusinessManager.Tests
```

Follow Clean Architecture dependency rules:

```text
App
 ↓
Application
 ↓
Domain

Infrastructure → Application + Domain
```

Domain must not depend on WPF, EF Core, SQLite, SMTP, or infrastructure implementations.

Use interfaces in Application and implementations in Infrastructure.

---

# 3. Application Purpose

The application manages:

- Company information
- Vehicles
- Drivers
- Customers
- Petrol pumps
- Fuel purchases
- General business expenses
- Business income
- VAT information
- Bills
- Attachments
- Nepal fiscal-year reporting
- Monthly reports
- Quarterly reports
- Fiscal-year reports
- Backup and restore
- Email backups
- Application settings
- Secure credentials
- Audit logs

Keep the system simple and business-focused. Do not turn it into a full accounting ERP.

---

# 4. Company Setup

The application must support first-run company setup.

Company fields:

```text
Id
Name
ShortName
RegistrationNumber
PANNumber
VATNumber
Address
Province
District
Municipality
Ward
Phone
Email
Website
LogoPath
FiscalYearStartMonth
Currency
CurrencySymbol
CreatedAt
UpdatedAt
```

The company name must appear throughout the application:

- Main window title
- Dashboard
- Reports
- PDF exports
- Excel exports
- Print layouts
- Backup metadata

Display the current fiscal year in the application header.

Example:

```text
ABC Goods Transport Pvt. Ltd.
Fiscal Year 2083/84 B.S.
```

---

# 5. First-Run Setup Wizard

On first application startup:

1. Create application directories.
2. Create SQLite database.
3. Run EF Core migrations.
4. Create default expense categories.
5. Create default income categories.
6. Create default payment methods.
7. Ask for company information.
8. Configure fiscal year.
9. Optionally configure email backup.
10. Test database.
11. Show dashboard.

Do not allow normal transaction entry until company setup is completed.

---

# 6. Database

Use SQLite with EF Core.

Store the database under:

```text
Data/transport.db
```

Recommended directories:

```text
TransportBusinessManager/
├── TransportBusinessManager.exe
├── Data/
│   └── transport.db
├── Documents/
├── Backups/
├── Logs/
└── Config/
```

Do not store passwords or sensitive credentials in plain text inside SQLite.

---

# 7. Core Entities

Implement at minimum:

```text
Company
FiscalYear

Vehicle
Driver
Customer
PetrolPump

ExpenseCategory
Expense
FuelExpense

IncomeCategory
Income

Attachment
PaymentMethod

BackupHistory
AuditLog

AppSetting
```

Use proper relationships, foreign keys, indexes, timestamps, and soft-delete where appropriate.

---

# 8. Vehicle

Fields:

```text
Id
CompanyId
VehicleNumber
VehicleType
Make
Model
DriverId
IsActive
Notes
CreatedAt
UpdatedAt
```

Support vehicle types such as:

- Truck
- Mini Truck
- Pickup
- Van
- Other

---

# 9. Driver

Fields:

```text
Id
CompanyId
Name
Phone
LicenseNumber
LicenseExpiryDate
Address
IsActive
Notes
CreatedAt
UpdatedAt
```

---

# 10. Petrol Pump

Fields:

```text
Id
CompanyId
Name
VATNumber
PANNumber
Address
Phone
Email
IsActive
Notes
CreatedAt
UpdatedAt
```

When selecting a petrol pump during fuel entry, automatically display its VAT number.

---

# 11. General Expenses

Create a generic `Expense` entity.

Fields:

```text
Id
CompanyId

ExpenseDate
ExpenseCategoryId

Description

VendorName
VendorVATNumber
BillNumber

SubTotal
DiscountAmount
VATAmount
TotalAmount

PaymentMethodId

VehicleId
DriverId

IsVatBill

Notes

CreatedAt
UpdatedAt
```

Expense categories should include:

```text
Fuel
Vehicle Repair
Maintenance
Spare Parts
Tyres
Insurance
Road/Toll
Driver Salary
Salary
Rent
Electricity
Office
Bank Charges
Tax
Other
```

Allow users to create custom categories.

---

# 12. Fuel Expense

Fuel is a specialized expense.

Fields:

```text
Id
ExpenseId
PetrolPumpId
FuelType
Quantity
RatePerUnit
OdometerReading
```

Fuel types:

```text
Diesel
Petrol
Other
```

Automatically calculate:

```text
SubTotal = Quantity × RatePerUnit
VAT = SubTotal × VAT Rate
Total = SubTotal + VAT - Discount
```

Allow manual adjustment only where appropriate and maintain validation.

---

# 13. VAT

Default VAT rate:

```text
13%
```

But make the VAT rate configurable.

Every VAT bill should support:

```text
VendorVATNumber
BillNumber
SubTotal
VATAmount
TotalAmount
```

VAT should be separately reportable.

---

# 14. Critical Duplicate Bill Rule

Prevent duplicate bills using:

```text
CompanyId
VendorVATNumber
BillNumber
```

The same bill number may exist for different VAT numbers.

Example:

```text
VAT 123456789 + Bill 1001 = allowed

VAT 123456789 + Bill 1001 = duplicate

VAT 987654321 + Bill 1001 = allowed
```

Enforce this at:

1. Application validation level.
2. SQLite unique index/database level.

Do not rely only on UI validation.

Provide a clear error:

```text
Duplicate Bill

Bill number '1001' already exists for VAT
number '123456789'.

Please verify the bill before saving.
```

---

# 15. Income

Implement:

```text
Income
```

Fields:

```text
Id
CompanyId
IncomeDate
IncomeCategoryId
CustomerId
InvoiceNumber
Description
SubTotal
VATAmount
TotalAmount
PaymentMethodId
PaymentStatus
VehicleId
Notes
CreatedAt
UpdatedAt
```

Income categories may include:

```text
Transport Service
Goods Delivery
Freight
Other
```

---

# 16. Fiscal Year — Nepal

The application must support Nepal's Bikram Sambat calendar for business reporting.

Display month names in English:

```text
Baisakh
Jestha
Ashadh
Shrawan
Bhadra
Ashwin
Kartik
Mangsir
Poush
Magh
Falgun
Chaitra
```

Nepal fiscal year:

```text
Shrawan 1 → Ashadh end
```

Example:

```text
2083/84 B.S.

Start:
Shrawan 2083

End:
Ashadh 2084
```

Quarters:

```text
Q1 = Shrawan, Bhadra, Ashwin
Q2 = Kartik, Mangsir, Poush
Q3 = Magh, Falgun, Chaitra
Q4 = Baisakh, Jestha, Ashadh
```

Do not hard-code these rules throughout the application.

Create:

```text
INepaliCalendarService
```

Responsibilities:

```text
Convert Gregorian → Nepali
Convert Nepali → Gregorian
Get Nepali Year
Get Nepali Month
Get Fiscal Year
Get Quarter
Get Fiscal Period
```

Store transaction dates using a reliable canonical date representation and derive fiscal periods for reporting.

---

# 17. Reporting

Implement a reusable reporting architecture.

Create:

```text
IReportService
```

Reports:

```text
Monthly Expense Report
Quarterly Expense Report
Fiscal Year Expense Report

Monthly Income Report
Quarterly Income Report
Fiscal Year Income Report

Profit/Loss Report
VAT Summary
Vehicle Expense Report
Petrol Pump Report
Expense Category Report
```

Filters:

```text
Fiscal Year
Month
Quarter
Start Date
End Date
Vehicle
Expense Category
Petrol Pump
```

---

# 18. Monthly Report

Display:

```text
Income
Expenses
Net Result

Fuel
Repair
Maintenance
Spare Parts
Tyres
Salary
Toll
Insurance
Office
Other

Total VAT
```

Example:

```text
Bhadra 2083

Income          Rs. X
Expenses        Rs. X
Net             Rs. X

Fuel            Rs. X
Repair          Rs. X
Maintenance     Rs. X
Other           Rs. X

Input VAT       Rs. X
```

---

# 19. Quarterly Report

Support:

```text
Q1
Q2
Q3
Q4
```

Example:

```text
Fiscal Year: 2083/84

Q1
Shrawan - Bhadra - Ashwin
```

Show:

```text
Total Income
Total Expenses
Net Result
VAT
Expense breakdown
```

---

# 20. Fiscal Year Report

Example:

```text
Fiscal Year 2083/84

Total Income
Total Expenses
Net Result

Fuel
Repair
Maintenance
Spare Parts
Tyres
Salary
Toll
Insurance
Office
Other

Total VAT
```

Also show monthly breakdown:

```text
Month       Income      Expense      Net
Baisakh
Jestha
Ashadh
Shrawan
...
```

---

# 21. Dashboard

Create a professional but simple dashboard.

Show:

```text
Current Month
Current Quarter
Current Fiscal Year
```

Cards:

```text
Total Income
Total Expenses
Net Result
VAT
Fuel Expense
Number of Bills
```

Show expense category breakdown.

Show recent transactions.

Show backup status.

Show current company and fiscal year.

---

# 22. Search and Filtering

Transactions must support searching by:

```text
Bill Number
VAT Number
Vendor
Petrol Pump
Vehicle Number
Customer
Date
Expense Category
```

Use pagination for large lists.

Support sorting.

Do not load thousands of records unnecessarily into the UI.

Use database-side filtering.

---

# 23. Attachments

Allow users to attach:

- Bill images
- PDFs
- Supporting documents

Do not store large files directly in SQLite.

Store files under:

```text
Documents/
```

and store metadata/path in SQLite.

Attachment fields:

```text
Id
ExpenseId
IncomeId
FileName
RelativePath
ContentType
FileSize
Hash
CreatedAt
```

---

# 24. Backup

Implement automatic local backup.

Default:

```text
Daily
30-day retention
```

Backup must include:

```text
SQLite database
Documents
Attachments
Company metadata
```

Create:

```text
IBackupService
```

Methods:

```text
CreateBackup()
ValidateBackup()
RestoreBackup()
DeleteOldBackups()
GetBackupHistory()
```

Backup flow:

```text
SQLite
↓
Safe database backup
↓
Add Documents
↓
ZIP
↓
Encrypt
↓
Store locally
↓
Optionally email
```

Never overwrite the original database during backup.

---

# 25. Email Backup

Email backup must be configurable.

Settings:

```text
SMTP Host
SMTP Port
SMTP Username
SMTP Password
Use SSL/TLS
From Email
Backup Email
```

Provide:

```text
[Test Email]
```

and:

```text
[Test Backup]
```

Email failure must never prevent normal application operation.

If email fails:

```text
Local backup successful
Email backup failed
```

Record this in `BackupHistory`.

---

# 26. Credentials

Never hard-code credentials.

Never commit credentials to source control.

Never store SMTP passwords as plain text in:

```text
appsettings.json
```

Use Windows secure credential storage / DPAPI.

Create:

```text
ICredentialService
```

Responsibilities:

```text
SaveSecret()
GetSecret()
DeleteSecret()
```

Secrets may include:

```text
SMTP password
Backup encryption key
Application security secrets
```

---

# 27. Configuration

Use `appsettings.json` for technical defaults:

```json
{
  "Application": {
    "Name": "Transport Business Manager"
  },

  "Database": {
    "FileName": "transport.db"
  },

  "Storage": {
    "DataDirectory": "Data",
    "DocumentDirectory": "Documents",
    "BackupDirectory": "Backups",
    "LogDirectory": "Logs"
  },

  "Backup": {
    "Enabled": true,
    "Frequency": "Daily",
    "RetentionDays": 30,
    "Compress": true,
    "Encrypt": true
  },

  "Email": {
    "Enabled": false,
    "SmtpHost": "",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "",
    "FromEmail": ""
  },

  "Vat": {
    "DefaultRate": 13
  },

  "FiscalYear": {
    "Calendar": "NepalBS",
    "StartMonth": "Shrawan"
  }
}
```

Business configuration should be stored in SQLite.

Credentials must use secure Windows storage.

---

# 28. Audit Logging

Track important actions:

```text
Create
Update
Delete
Restore
Backup
Login
Settings change
```

Create:

```text
AuditLog
```

Fields:

```text
Id
CompanyId
Action
EntityName
EntityId
Description
Timestamp
```

Do not log passwords, SMTP secrets, encryption keys, or sensitive credentials.

---

# 29. Application Security

Support:

```text
Application Password
Auto Lock
Manual Lock
```

Allow configurable timeout:

```text
5 minutes
10 minutes
15 minutes
30 minutes
Never
```

Do not implement unnecessary complex authentication for the first version.

---

# 30. UI Requirements

Use WPF MVVM.

Create a consistent navigation system:

```text
Dashboard

Transactions
├── Income
├── Expenses
└── Fuel

Transport
├── Vehicles
├── Drivers
└── Petrol Pumps

Master Data
├── Customers
├── Expense Categories
└── Income Categories

Reports
├── Monthly
├── Quarterly
├── Fiscal Year
├── VAT
├── Vehicle
└── Profit/Loss

Backup

Settings
├── Company
├── Fiscal Year
├── VAT
├── Email
├── Backup
├── Security
└── Storage
```

Use reusable controls for:

- Data grids
- Search
- Filters
- Date selection
- Currency display
- Validation messages
- Confirmation dialogs

---

# 31. UX Requirements

The application is for business users, not developers.

Prioritize:

- Simple forms
- Keyboard navigation
- Clear validation
- Confirmation before destructive actions
- Search
- Fast data entry
- Minimal clicks
- Clear totals
- Nepali fiscal-year terminology
- English month names

When saving a transaction, show a clear success notification.

When validation fails, show the problem beside the relevant field.

---

# 32. Data Integrity

Use:

- Foreign keys
- Unique constraints
- Required fields
- Numeric precision
- Transactions where multiple database changes must succeed together
- Concurrency-safe operations where appropriate
- Database indexes
- Soft deletion where historical records must remain traceable

Never silently swallow exceptions.

---

# 33. Logging

Use Serilog.

Log:

```text
Application startup
Application shutdown
Database errors
Backup operations
Email operations
Restore operations
Unexpected exceptions
```

Do not log:

```text
Passwords
SMTP credentials
Encryption keys
Sensitive secrets
```

Write logs to:

```text
Logs/
```

Use rolling files.

---

# 34. Error Handling

Create centralized error handling.

User-facing errors must be understandable.

Bad:

```text
SQLiteException: constraint failed...
```

Good:

```text
Unable to save the expense.

The bill number already exists for this VAT number.
Please verify the bill.
```

Log the technical exception separately.

---

# 35. Database migrations

Use EF Core migrations.

Development:

```text
dotnet ef migrations add InitialCreate
```

Production:

```text
Database.Migrate()
```

Run migrations safely during application startup.

Never automatically delete/recreate the production database.

---

# 36. Testing

Create unit tests for:

```text
VAT calculation
Expense calculation
Duplicate bill validation
Fiscal year calculation
Quarter calculation
Nepali date conversion
Report totals
Backup validation
```

Integration tests:

```text
SQLite database
EF Core mappings
Unique constraints
Backup/restore
```

Critical business rules must have tests.

---

# 37. Important calculation rules

Use decimal for money.

Never use `double` for financial amounts.

Example:

```text
decimal SubTotal
decimal VATAmount
decimal TotalAmount
decimal Rate
decimal Quantity
```

Currency calculations must be deterministic and appropriately rounded.

---

# 38. Backup restore safety

Before restoring:

```text
1. Validate selected backup.
2. Verify checksum.
3. Create current database safety backup.
4. Close database connections.
5. Restore backup.
6. Run database validation.
7. Restart/reload application.
```

Never destroy the current database before a valid backup has been created.

---

# 39. Portable publishing

Publish as self-contained Windows x64.

Target:

```text
win-x64
```

Prefer:

```text
Self-contained
Single-file
```

The application should not require:

- SQL Server
- .NET runtime installation
- IIS
- Web server
- Docker

Document the portable deployment process.

Do not recommend running the active SQLite database directly from a USB drive. Use local storage and USB for backup/restore.

---

# 40. Coding Standards

Follow:

- Nullable reference types
- Async/await where applicable
- Dependency injection
- SOLID principles
- Clean Architecture
- MVVM
- Small services
- No god classes
- No business logic in code-behind
- No hard-coded credentials
- No hard-coded company information
- No hard-coded fiscal-year logic throughout the application
- No duplicated calculation logic

Use meaningful names.

Prefer immutable DTOs where practical.

---

# 41. Implementation Order

Implement in this exact order:

### Phase 1

```text
Solution
Projects
DI
Configuration
Logging
SQLite
EF Core
Migrations
```

### Phase 2

```text
Company setup
Fiscal year
Settings
First-run wizard
```

### Phase 3

```text
Vehicles
Drivers
Customers
Petrol Pumps
Categories
Payment Methods
```

### Phase 4

```text
Expenses
Fuel
Income
VAT
Duplicate bill prevention
```

### Phase 5

```text
Dashboard
Search
Filters
Monthly reports
Quarterly reports
Fiscal-year reports
```

### Phase 6

```text
Attachments
Excel
PDF
Printing
```

### Phase 7

```text
Local backup
Encrypted backup
Email backup
Restore
Backup history
```

### Phase 8

```text
Security
Audit logs
Application lock
Testing
Performance
Packaging
```

Do not jump to Phase 7 before the core transaction and database functionality is stable.

---

# 42. Definition of Done

The application is considered complete only when:

- It starts without internet.
- Company setup works.
- SQLite database is created automatically.
- EF Core migrations work.
- Company name is displayed throughout the UI.
- Nepal B.S. fiscal year works.
- English Nepali month names work.
- Monthly reports work.
- Quarterly reports work.
- Fiscal-year reports work.
- Fuel expenses work.
- General expenses work.
- Income works.
- VAT calculations work.
- Duplicate VAT + Bill Number is prevented.
- Petrol pumps work.
- Vehicles work.
- Drivers work.
- Attachments work.
- Search works.
- Excel export works.
- PDF export works.
- Local backup works.
- Email backup works.
- Restore works.
- Backup history works.
- SMTP credentials are securely stored.
- No secrets are committed to source control.
- Application logs work.
- Critical business rules have tests.
- Application can be published as a portable Windows application.

---

# 43. Agent Behavior

When implementing:

1. Inspect the existing project before changing anything.
2. Do not rewrite working code unnecessarily.
3. Implement one phase at a time.
4. Compile after meaningful changes.
5. Run tests after database/business-rule changes.
6. Fix compilation errors before continuing.
7. Create migrations whenever the EF model changes.
8. Never fake successful implementation.
9. Never hard-code credentials.
10. Never remove existing data to fix a migration problem.
11. Prefer backward-compatible database changes.
12. Explain architectural decisions briefly when they affect future development.
13. Keep UI, business logic, and infrastructure separated.
14. Use interfaces for external services.
15. Treat financial calculations and duplicate-bill prevention as critical business rules.
16. Do not add unnecessary cloud infrastructure.
17. Keep the application lightweight and portable.
18. Preserve offline functionality.
19. Before implementing a new feature, check whether an existing service/entity can be extended instead of creating duplicate functionality.
20. Keep the code production-ready rather than writing prototype-only code.

Start by implementing **Phase 1 — solution structure, .NET 10 WPF setup, dependency injection, configuration, logging, SQLite, EF Core, and initial migrations**. Do not implement all features at once.