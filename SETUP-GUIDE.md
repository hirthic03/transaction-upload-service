\# Setup Guide for Reviewers



\## Quick Start (5 minutes)



\### Prerequisites

\- .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

\- SQL Server LocalDB (included with Visual Studio) or Docker



\### Setup Steps



1\. \*\*Clone the repository\*\*

```bash

&nbsp;  git clone https://github.com/YOUR-USERNAME/transaction-upload-service.git

&nbsp;  cd transaction-upload-service

```



2\. \*\*Setup database\*\*

```bash

&nbsp;  dotnet ef database update --project src/Transactions.Infrastructure --startup-project src/Transactions.Api

```



3\. \*\*Run the application\*\*

```bash

&nbsp;  cd src/Transactions.Api

&nbsp;  dotnet run

```



4\. \*\*Test the application\*\*

&nbsp;  - Swagger UI: http://localhost:5000/swagger

&nbsp;  - Frontend: Open `samples/frontend.html` in browser



\### Quick Test



\#### Upload a file via Swagger:

1\. Navigate to http://localhost:5000/swagger

2\. Click `POST /api/v1/uploads`

3\. Click "Try it out"

4\. Upload `samples/valid.csv`

5\. Click "Execute"

6\. Should return HTTP 200 with importId



\#### Query transactions:

1\. Click `GET /api/v1/transactions`

2\. Click "Try it out"

3\. Click "Execute"

4\. Should return all uploaded transactions



\### Sample Files

\- `samples/valid.csv` - Valid CSV format

\- `samples/valid.xml` - Valid XML format

\- `samples/invalid.csv` - For testing validation errors

\- `samples/frontend.html` - Web interface for testing



\### Database Setup (Alternative - SQL Script)

If you prefer to use the SQL script directly:

```bash

sqlcmd -S (localdb)\\mssqllocaldb -i database/schema.sql

```



\### Troubleshooting



\*\*Issue: "dotnet ef not found"\*\*

```bash

dotnet tool install --global dotnet-ef

```



\*\*Issue: "Cannot connect to database"\*\*

\- Ensure SQL Server LocalDB is running

\- Or use Docker: `docker-compose up -d`



\*\*Issue: "Port 5000 already in use"\*\*

\- Change port in `appsettings.json` or stop other applications using port 5000



\### Features Demonstrated

✅ CSV and XML file upload support

✅ Comprehensive validation with detailed error messages

✅ Idempotent uploads (duplicate detection via SHA-256)

✅ Query by currency, status, and date range

✅ Status mapping (CSV/XML → Unified format)

✅ Clean architecture with separation of concerns

✅ Error handling and logging

✅ RESTful API design

✅ Swagger documentation



\### Architecture

\- \*\*API Layer\*\*: ASP.NET Core Web API with Swagger

\- \*\*Domain Layer\*\*: Business logic, parsers, validators

\- \*\*Infrastructure Layer\*\*: EF Core, repositories, database

\- \*\*Testing\*\*: Unit and integration tests



For detailed documentation, see README.md

