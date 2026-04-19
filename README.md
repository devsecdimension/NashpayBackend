<div align="center">
  <img src="https://img.icons8.com/external-flat-juicy-fish/100/external-payment-fintech-flat-juicy-fish.png" width="100" height="100" />
  <h1>NashPay Backend API WORK</h1>
  <p><b>A Secure, Scalable, and High-Performance Fintech Payment Gateway Solution</b></p>

  <img src="https://img.shields.io/badge/.NET%2010-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" />
  <img src="https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white" />
  <img src="https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=json-web-tokens&logoColor=white" />
  <img src="https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black" />
</div>

---

## 📖 Overview
NashPay is a modern digital payment ecosystem designed to empower merchants with a seamless checkout experience. It features high-concurrency transaction logging, HMAC-secured webhooks, and a robust JWT-based authentication layer.

---

## 🚀 Tech Stack

| Technology | Logo | Category |
| :--- | :---: | :--- |
| **ASP.NET Core 10** | <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/dotnetcore/dotnetcore-original.svg" width="25"> | Backend Framework |
| **C# 13** | <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/csharp/csharp-original.svg" width="25"> | Programming Language |
| **MS SQL Server** | <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/microsoftsqlserver/microsoftsqlserver-plain-wordmark.svg" width="25"> | Primary Database |
| **Entity Framework** | <img src="https://img.icons8.com/color/48/null/visual-studio.png" width="25"> | ORM Layer |
| **Newtonsoft JSON** | <img src="https://www.newtonsoft.com/favicon.ico" width="25"> | Serialization |

---

## Key Features

- ✅ **Checkout Sessions:** Lifecycle management (Initiate ➔ Verify ➔ Complete).
- ✅ **Secure Webhooks:** Event-driven notifications with `HMACSHA256` signatures.
- ✅ **Advanced Auth:** Role-based access control with **JWT Bearer**.
- ✅ **API Documentation:** Interactive **Swagger UI** for testing.
- ✅ **Clean Architecture:** Separation of concerns using Services and DTOs.

---

## 🛠 Local Setup Instructions

### 1. Prerequisites
Ensure you have the following installed:
* **.NET 10 SDK**
* **SQL Server Express**
* **EF Core Tools:** `dotnet tool install --global dotnet-ef`

### 2. Configuration
Clone the repo and update your `appsettings.json`:
```bash
git clone [https://github.com/devsecdimension/NashpayBackend.git](https://github.com/devsecdimension/NashpayBackend.git)
cd NashpayBackend/NashPay.API

==============
Update Connection String:

JSON

"DefaultConnection": "Server=YOUR_SERVER;Database=NashPayDb;Trusted_Connection=True;"

3. Database Migration & Execution

===========
dotnet ef database update
dotnet run
