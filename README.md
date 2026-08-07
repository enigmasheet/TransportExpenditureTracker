# 🚚 TransportExpenditureTracker

**Live Demo**: [https://transportexpense.azurewebsites.net/](https://transportexpense.azurewebsites.net/)

A web-based application designed to streamline the management of transport-related expenditures for the goods transport industry. Built with **ASP.NET Core MVC** and **Identity**, it offers robust features for invoice tracking, reporting, party and item management, and more.

---

## 📋 Features

### 🔐 User Authentication & Authorization
- **ASP.NET Core Identity**: Secure user registration and login.
- **Role-Based Access**:
  - **Admin**: Full control over invoices, users, roles, and system settings.
  - **User**: Can create and view their invoices only.
- **Automatic Role Assignment**: New users are assigned the `User` role by default.

### 🧾 Invoice Management
- **Full CRUD**: Create, read, update, and delete invoices.
- **Comprehensive Invoice Fields**:
  - Invoice Number
  - Nepali Miti (Date)
  - Party Information
  - Item Details
  - Quantity and Rate
  - Taxable Amount, VAT, and Total
  - Created/Updated Timestamps
- **Role-Based Restrictions**:
  - Admin: Full access
  - User: Limited to create/view

### 🧑‍💼 Party & Item Management
- Add, edit, and remove **Parties** and **Items**.
- Associate items and parties directly with invoices.

### 📊 Reporting
- Filterable reports by:
  - Date Range
  - Party
  - Item
- **Export Options**: PDF and Excel (planned in future releases).

### 🌐 Localization
- **Nepali Date Support**: Integrated Nepali Miti calendar for date fields.

### 🛠️ Admin Panel
- Manage all users and assign/remove roles.
- Configure core settings from the admin dashboard.

---

## 🚀 Getting Started

### ✅ Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or above
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or use SQLite with minor config changes)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/)

---

## 🛠️ Installation

### 1. Clone the Repository

```bash
git clone https://github.com/enigmasheet/TransportExpenditureTracker.git
cd TransportExpenditureTracker
```

### 2. Configure the Database

Edit `appsettings.json` and set your SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=TransportDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Apply Migrations

```bash
dotnet ef database update
```

This will create the database schema and seed roles (`Admin`, `User`) if configured.

### 4. Run the Application

```bash
dotnet run
```

### 5. Open in Browser

Visit [http://localhost:5000](http://localhost:5000) (or the port shown in your terminal).

---

## 🔑 Configuration via Environment Variables

Sensitive settings are **not** committed to source. Set them as environment
variables (or .NET user-secrets) to enable optional features:

| Feature | Variable | Purpose |
|---|---|---|
| SuperAdmin bootstrap | `SuperAdmin__Email`, `SuperAdmin__Password`, `SuperAdmin__FullName` | Seeds the first administrator on a fresh database |
| Email / exports | `Resend__ApiKey` | Enables confirmation emails and emailed report exports |

Without `Resend__ApiKey` the app runs fully offline; email features simply skip silently.

> **Security note:** The previously committed Resend API key and SuperAdmin
> password have been removed from `appsettings.json`. If you ever used the old
> key, rotate it in Resend immediately.

---

## 📂 Project Structure

```
TransportExpenditureTracker/
├── Controllers/         # MVC Controllers
├── Models/              # Data Models
├── Data/                # Database context, migrations
├── Services/            # Business logic/services
├── ViewModels/          # View Models for UI
├── Views/               # Razor views (UI)
├── wwwroot/             # Static files (CSS, JS, images)
└── ...
```

---

## 🧪 Testing

- Unit and integration tests are recommended for all business logic and controllers.
- Run tests (if present) with:
  ```bash
  dotnet test
  ```

---

## 🤝 Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## 🛡️ Security

- Do not store sensitive credentials in source control.
- All user input is validated and sanitized.
- For vulnerabilities, please open an issue or contact the maintainers.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🙋‍♂️ Support & Feedback

- For questions, open an issue or discussion in the repository.
- For bugs or feature requests, please file an issue with as much information as possible.

---

**Thank you for using and contributing to TransportExpenditureTracker!**
