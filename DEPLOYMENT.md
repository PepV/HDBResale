
### Required Tools
- AWS CLI (v2 or later)
- Terraform (v1.0 or later)
- Docker (v20.10 or later)
- .NET 8 SDK
- Node.js 18+
- Git

### AWS Resources Required
- AWS Account with admin access
- IAM user with programmatic access
- Route 53 hosted domain (for production)

## Local Deployment

### 1. Backend Setup
```bash
cd backend/HDBResale.API
dotnet restore
dotnet build
dotnet run --urls=http://localhost:5000


Front End:
cd frontend
npm install
npm run dev
