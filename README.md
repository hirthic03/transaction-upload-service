# Transaction Upload Service

A robust .NET 8 Web API for uploading and querying transaction data from CSV and XML files with comprehensive validation, idempotency, and error handling.

## 🚀 Features

- **Multi-format Support**: Upload transactions via CSV or XML files
- **Comprehensive Validation**: All fields validated with detailed error reporting
- **Idempotent Uploads**: Duplicate files automatically detected via SHA-256 hashing
- **Flexible Querying**: Filter transactions by currency, status, and date range
- **Clean Architecture**: Domain-driven design with clear separation of concerns
- **Production Ready**: Docker support, health checks, logging, and CI/CD pipeline
- **Test Coverage**: Unit and integration tests included

## 📋 Requirements

- .NET 8 SDK
- SQL Server (LocalDB or Docker)
- Docker & Docker Compose (optional)

## 🛠️ Quick Start

### Option 1: Using LocalDB

1. **Clone the repository**
```bash
git clone <repository-url>
cd Transactions
```

2. **Update connection string** (if needed) in `src/Transactions.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TransactionsDb;Trusted_Connection=True;"
}
```

3. **Run migrations**
```bash
dotnet ef database update -p src/Transactions.Infrastructure -s src/Transactions.Api
```

4. **Run the API**
```bash
dotnet run --project src/Transactions.Api
```

5. **Access Swagger UI**
Open browser to: https://localhost:5001/swagger or http://localhost:5000/swagger

### Option 2: Using Docker

1. **Start all services**
```bash
docker-compose up -d
```

2. **Access the API**
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- SQL Server: localhost,1433 (sa/YourStrong@Password123)

## 📁 File Format Specifications

### CSV Format
```csv
"TransactionId","Amount","CurrencyCode","Date","Status"
"Invoice0000001","1,000.00", "USD", "20/02/2019 12:33:16", "Approved"
"Invoice0000002","300.00","USD","21/02/2019 02:04:59", "Failed"
```

- **Columns** (in order): Id, Amount, CurrencyCode, TransactionDate, Status
- **Date Format**: dd/MM/yyyy HH:mm:ss
- **Valid Statuses**: Approved, Failed, Finished

### XML Format
```xml
<Transactions>
  <Transaction id="Inv00001">
    <TransactionDate>2019-01-23T13:45:10</TransactionDate>
    <PaymentDetails>
      <Amount>200.00</Amount>
      <CurrencyCode>USD</CurrencyCode>
    </PaymentDetails>
    <Status>Done</Status>
  </Transaction>
</Transactions>
```

- **Date Format**: yyyy-MM-ddTHH:mm:ss
- **Valid Statuses**: Approved, Rejected, Done

## 🔄 Status Mappings

| CSV Status | XML Status | API Output |
|------------|------------|------------|
| Approved   | Approved   | A          |
| Failed     | Rejected   | R          |
| Finished   | Done       | D          |

## 📡 API Endpoints

### Upload Transactions
```bash
POST /api/v1/uploads

# Success Response (200 OK)
{
  "importId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "imported": 5
}

# Validation Error Response (400 Bad Request)
{
  "errors": [
    {
      "row": 3,
      "field": "currencycode",
      "message": "Invalid ISO4217 currency code"
    }
  ]
}

# Unknown Format (400 Bad Request)
{
  "error": "Unknown format"
}

# File Too Large (413 Payload Too Large)
{
  "error": "File too large (max 1 MB)"
}
```

### Query Transactions
```bash
GET /api/v1/transactions?currency=USD&status=A&from=2019-01-01&to=2019-12-31

# Response (200 OK)
[
  {
    "id": "Invoice0000001",
    "payment": "1000.00 USD",
    "status": "A"
  }
]
```

## 💻 cURL Examples

### Upload CSV File
```bash
# Success case
curl -X POST http://localhost:5000/api/v1/uploads \
  -F "file=@samples/valid.csv"

# Invalid CSV (validation errors)
curl -X POST http://localhost:5000/api/v1/uploads \
  -F "file=@samples/invalid.csv"
```

### Upload XML File
```bash
# Success case
curl -X POST http://localhost:5000/api/v1/uploads \
  -F "file=@samples/valid.xml"

# Invalid XML (validation errors)
curl -X POST http://localhost:5000/api/v1/uploads \
  -F "file=@samples/invalid.xml"
```

### Query Transactions
```bash
# All transactions
curl http://localhost:5000/api/v1/transactions

# Filter by currency
curl http://localhost:5000/api/v1/transactions?currency=USD

# Filter by status (A=Approved, R=Rejected, D=Done)
curl http://localhost:5000/api/v1/transactions?status=A

# Filter by date range
curl "http://localhost:5000/api/v1/transactions?from=2019-01-01&to=2019-12-31"

# Combined filters
curl "http://localhost:5000/api/v1/transactions?currency=USD&status=A&from=2019-01-01"
```

### Test Idempotency
```bash
# Upload the same file twice
curl -X POST http://localhost:5000/api/v1/uploads -F "file=@samples/valid.csv"
curl -X POST http://localhost:5000/api/v1/uploads -F "file=@samples/valid.csv"
# Second upload returns same importId without re-processing
```

### Health Check
```bash
curl http://localhost:5000/health
# Response: {"status":"healthy","timestamp":"2024-01-01T12:00:00Z"}

curl http://localhost:5000/metrics
# Response: {"transactions_total":10,"imports_total":3,"timestamp":"2024-01-01T12:00:00Z"}
```

## 🧪 Running Tests

### Unit Tests
```bash
dotnet test tests/Transactions.Tests --filter "FullyQualifiedName~Unit"
```

### Integration Tests
```bash
dotnet test tests/Transactions.Tests --filter "FullyQualifiedName~Integration"
```

### All Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## 🎨 Frontend UI

A simple HTML interface is provided for testing:

1. **Open in browser**: `samples/frontend.html`
2. **Features**:
   - Drag & drop file upload
   - Real-time validation feedback
   - Transaction search with filters
   - Responsive design

## 🏗️ Project Structure
```
/
├── src/
│   ├── Transactions.Api/           # Web API layer
│   │   ├── Controllers/            # API endpoints
│   │   ├── DTOs/                   # Data transfer objects
│   │   └── Middleware/             # Custom middleware
│   ├── Transactions.Domain/        # Business logic
│   │   ├── Entities/               # Domain models
│   │   ├── Services/               # Business services
│   │   ├── Parsers/                # File parsers
│   │   └── Validators/             # FluentValidation rules
│   └── Transactions.Infrastructure/# Data access
│       ├── Data/                   # DbContext & configs
│       └── Repositories/           # Repository implementations
├── tests/
│   └── Transactions.Tests/         # Unit & integration tests
├── samples/                        # Sample files & frontend
├── docker-compose.yml              # Docker orchestration
└── README.md                       # This file
```

## 🔒 Security Features

- Input validation on all fields
- SQL injection prevention via EF Core parameterized queries
- File size limits (1 MB max)
- Correlation IDs for request tracking
- CORS configuration with explicit origins
- No sensitive data in logs

## 📊 Database Schema
```sql
-- transactions table
id nvarchar(50) PK
amount decimal(18,2)
currency_code char(3)
transaction_date datetime2
status_code char(1)
source_format varchar(10)
import_id uniqueidentifier FK

-- imports table
import_id uniqueidentifier PK
received_at datetime2
source_format varchar(10)
sha256 nvarchar(100) UNIQUE
record_count int
status nvarchar(20)

-- import_errors table
id int PK IDENTITY
import_id uniqueidentifier FK
row_number int
field nvarchar(50)
message nvarchar(500)
```

## 🚢 Production Deployment

1. **Update connection string** for production SQL Server
2. **Set environment variables**:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__DefaultConnection=<production-connection-string>`
3. **Run migrations**: `dotnet ef database update`
4. **Deploy using Docker** or your preferred hosting platform

## 📝 License

This project is provided as-is for the technical assessment.

## 🤝 Support

For issues or questions, please check the sample files in `/samples` directory for valid/invalid format examples.