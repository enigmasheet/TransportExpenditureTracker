# 🚚 TransportExpenditureTracker

**Live Demo**: [https://transportexpense.azurewebsites.net/](https://transportexpense.azurewebsites.net/)

A web-based application designed to streamline the management of transport-related expenditures. Built using **ASP.NET Core MVC** and **Identity**, it offers robust features for invoice tracking, user management, and role-based access control.

---

## 📋 Features

### 🔐 User Authentication & Authorization
- **ASP.NET Core Identity Integration**: Secure user registration and login.
- **Role-Based Access Control**:
  - **Admin**: Full access to invoices, users, roles, and system settings.
  - **User**: Can create and view invoices only.
- **Automatic Role Assignment**: New users are automatically assigned the `User` role.

### 🧾 Invoice Management
- **Full CRUD**: Create, read, update, and delete invoices.
- **Detailed Invoice Fields**:
  - Invoice Number
  - Nepali Miti (Date)
  - Party Information
  - Item Details
  - Quantity and Rate
  - Taxable Amount, VAT, and Total
  - Created and Updated Timestamps
- **Role-Based Restrictions**:
  - Admin: Full access
  - User: Limited to create/view only

### 🧑‍💼 Party & Item Management
- Add, edit, and remove **Parties** and **Items**.
- Associate items and parties directly with invoices.

### 📊 Reporting
- Filterable reports by:
  - Date Range
  - Party
  - Item
- **Export Options**: PDF and Excel (future enhancement).

### 🌐 Localization
- **Nepali Date Support**: Integrated Nepali Miti for date fields.

### 🛠️ Admin Panel
- View all users and assign/remove roles.
- Configure core settings from the admin dashboard.

---

## 🚀 Getting Started

### ✅ Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or use SQLite with minor changes)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/)

---

## 🛠️ Installation

### 1. Clone the Repository

```bash
git clone https://github.com/enigmasheet/TransportExpenditureTracker.git
cd TransportExpenditureTracker
```
2. Configure the Database
Open appsettings.json and set your SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=TransportDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
3. Apply Migrations
```bash
dotnet ef database update
```
This will create the database schema and seed roles (Admin, User) if implemented in the Startup.cs or through a seed method.

4. Run the Application
```bash
dotnet run
```
5. Open in Browser
