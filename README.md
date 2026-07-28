
## Technology Stack

### Frontend
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **Charts**: Recharts
- **Routing**: React Router v6
- **HTTP Client**: Axios

### Backend
- **Framework**: .NET 8 Web API
- **Architecture**: Clean Architecture
- **Authentication**: JWT
- **Logging**: Serilog
- **Caching**: MemoryCache
- **Validation**: FluentValidation

### Infrastructure (AWS)
- **Container Orchestration**: ECS Fargate
- **Load Balancing**: Application Load Balancer
- **CDN**: CloudFront
- **Container Registry**: ECR
- **Logging**: CloudWatch
- **CI/CD**: GitHub Actions
- **IaC**: Terraform

## Data Flow

1. **Frontend Request Flow**
   - User interacts with React UI
   - API calls are made to backend through proxy
   - JWT token is included in Authorization header

2. **Backend Processing**
   - Requests are authenticated via JWT
   - Data is fetched from data.gov.sg API
   - Results are cached for 30 minutes
   - Statistics are computed and returned

3. **Data Sources**
   - Primary: data.gov.sg API
   - Datasets used:
     - Resale Flat Prices (1990-1999): d_ebc5ab87086db484f88045b47411ebc5
     - Resale Flat Prices (2015-2016): d_ea9ed51da2787afaf8e51f827c304208
     - Resale Flat Prices (2017+): d_8b84c4ee58e3cfc0ece0d773c8ca6abc
     - HDB Property Information: d_17f5382f26140b1fdae0ba2ef6239d2f
     - Price Range Offered: d_2d493bdcc1d9a44828b6e71cb095b88d

## Security Architecture

### Authentication & Authorization
- JWT-based authentication
- Role-based access control (Admin/User)
- Tokens expire after 60 minutes
- HTTPS enforced in production

### Security Measures
- CORS configuration for allowed origins
- Input validation on all endpoints
- No sensitive data in logs
- Secure headers (HSTS, XSS Protection)


