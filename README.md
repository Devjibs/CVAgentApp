# CV Agent App - AI-Powered CV Generation System

A comprehensive .NET application that uses OpenAI's agentic platform to automatically generate tailored CVs and cover letters based on job postings. The system implements a multi-agent architecture with comprehensive guardrails for safety and truthfulness.

## 🚀 Features

### Core Functionality
- **Multi-Agent Workflow**: Orchestrated agents for CV parsing, job extraction, matching, and generation
- **AI-Powered Analysis**: Uses OpenAI's Responses API with web search capabilities
- **Document Processing**: Supports PDF and Word document parsing and generation
- **Tailored Output**: Generates job-specific CVs and cover letters
- **Session Management**: Tracks workflow progress and maintains state

### Safety & Compliance
- **Input Guardrails**: Validates job URLs, CV files, and content
- **Output Guardrails**: Ensures truthfulness, quality, and compliance
- **Discrimination Prevention**: Prevents bias and protected attribute inclusion
- **ATS Compatibility**: Optimizes documents for Applicant Tracking Systems

### Monitoring & Evaluation
- **Performance Metrics**: Tracks agent execution times and success rates
- **Quality Evaluation**: Automated assessment of generated documents
- **System Health**: Comprehensive health checks and monitoring
- **Error Reporting**: Detailed error tracking and analysis

## 🏗️ Architecture

### Multi-Agent System
The application implements a sophisticated multi-agent architecture following OpenAI's Agents SDK patterns:

1. **CV Parsing Agent**: Extracts and structures candidate information
2. **Job Extraction Agent**: Fetches and analyzes job postings with web search
3. **Matching Agent**: Compares candidate profile with job requirements
4. **CV Generation Agent**: Creates tailored CVs and cover letters
5. **Review Agent**: Validates generated documents for quality and truthfulness
6. **Multi-Agent Orchestrator**: Coordinates the entire workflow

### Guardrail System
Comprehensive safety system with multiple guardrail types:

- **Job Posting Guardrail**: Validates job URLs and content
- **CV Content Guardrail**: Ensures CV files and content quality
- **Truthfulness Guardrail**: Prevents hallucination and fabricated content
- **Document Quality Guardrail**: Validates output quality and ATS compatibility
- **Compliance Guardrail**: Ensures discrimination law compliance

### Technology Stack
- **Backend**: ASP.NET Core 9.0 with Entity Framework Core
- **Database**: SQL Server with comprehensive entity relationships
- **AI Integration**: OpenAI Responses API with built-in tools
- **Document Processing**: PDF and Word document handling
- **File Storage**: Local file system (configurable for cloud storage)
- **Monitoring**: Comprehensive logging and metrics collection

## 📁 Project Structure

```
CV-Agent-App/
├── src/
│   ├── CVAgentApp.API/                 # Web API layer
│   │   ├── Controllers/               # API controllers
│   │   ├── Program.cs                 # Application configuration
│   │   └── appsettings.json          # Configuration settings
│   ├── CVAgentApp.Core/              # Core business logic
│   │   ├── Entities/                 # Domain entities
│   │   ├── DTOs/                     # Data transfer objects
│   │   └── Interfaces/               # Service contracts
│   ├── CVAgentApp.Infrastructure/    # Infrastructure layer
│   │   ├── Agents/                   # Agent implementations
│   │   ├── Guardrails/              # Guardrail implementations
│   │   ├── Services/                # Service implementations
│   │   └── Data/                    # Data access layer
│   └── CVAgentApp.Desktop/          # Desktop application
└── README.md                        # This file
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)
- OpenAI API Key
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd CV-Agent-App
   ```

2. **Configure the application**
   ```json
   // appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CVAgentAppDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     },
     "OpenAI": {
       "ApiKey": "YOUR_OPENAI_API_KEY_HERE"
     }
   }
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Create and migrate database**
   ```bash
   dotnet ef database update --project src/CVAgentApp.Infrastructure --startup-project src/CVAgentApp.API
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/CVAgentApp.API
   ```

### API Endpoints

#### CV Generation
- `POST /api/cvagent/generate` - Generate tailored CV and cover letter
- `GET /api/cvagent/status/{sessionToken}` - Get workflow status
- `POST /api/cvagent/cancel/{sessionToken}` - Cancel workflow
- `GET /api/cvagent/download/{documentId}` - Download generated document

#### Analysis
- `POST /api/cvagent/analyze-job` - Analyze job posting
- `POST /api/cvagent/analyze-candidate` - Analyze candidate CV

#### Evaluation & Monitoring
- `POST /api/evaluation/workflow/{sessionId}` - Evaluate workflow
- `POST /api/evaluation/document-quality` - Evaluate document quality
- `POST /api/evaluation/truthfulness` - Evaluate truthfulness
- `GET /api/evaluation/health` - Get system health
- `GET /api/evaluation/metrics` - Get performance metrics

## 🔧 Configuration

### OpenAI Configuration
```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here",
    "Model": "gpt-4o",
    "Temperature": 0.7,
    "MaxTokens": 4000
  }
}
```

### Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CVAgentAppDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### File Storage Configuration
```json
{
  "AzureStorage": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "ContainerName": "cv-agent-documents"
  }
}
```

## 🛡️ Security & Compliance

### Data Privacy
- Encrypted file storage for uploaded CVs
- Automatic data purging after generation
- No logging of sensitive personal information
- GDPR-compliant data handling

### Guardrails
- **Input Validation**: Comprehensive validation of all inputs
- **Output Verification**: Multi-layer output validation
- **Truthfulness**: Prevents fabrication of skills or experience
- **Compliance**: Ensures adherence to discrimination laws
- **Quality Assurance**: Maintains professional document standards

### Error Handling
- Comprehensive error logging and monitoring
- Graceful failure handling with user feedback
- Automatic retry mechanisms for transient failures
- Detailed error reporting for debugging

## 📊 Monitoring & Evaluation

### Performance Metrics
- Agent execution times
- Success/failure rates
- Guardrail trigger frequency
- System resource utilization

### Quality Evaluation
- Document quality scoring
- Truthfulness verification
- ATS compatibility assessment
- Professional language analysis

### Health Monitoring
- Database connectivity
- OpenAI API status
- File storage availability
- Agent operational status

## 🔄 Workflow Process

1. **Upload CV**: User uploads their CV (PDF/Word)
2. **Provide Job URL**: User provides job posting URL
3. **CV Parsing**: Agent extracts candidate information
4. **Job Analysis**: Agent fetches and analyzes job requirements
5. **Matching**: Agent compares candidate with job requirements
6. **Generation**: Agent creates tailored CV and cover letter
7. **Review**: Agent validates output for quality and truthfulness
8. **Delivery**: User receives generated documents

## 🧪 Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests
```bash
dotnet test --filter Category=Integration
```

### Load Testing
```bash
# Use tools like Apache Bench or Artillery
ab -n 100 -c 10 http://localhost:5000/api/cvagent/generate
```

## 🚀 Deployment

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "CVAgentApp.API.dll"]
```

### Azure Deployment
```bash
az webapp deployment source config-zip --resource-group myResourceGroup --name myAppName --src myapp.zip
```

### Environment Variables
```bash
export OpenAI__ApiKey="your-api-key"
export ConnectionStrings__DefaultConnection="your-connection-string"
export AzureStorage__ConnectionString="your-storage-connection"
```

## 📈 Performance Optimization

### Caching
- Redis caching for frequently accessed data
- Response caching for static content
- Session state management

### Scaling
- Horizontal scaling with load balancers
- Database connection pooling
- Async/await patterns throughout

### Monitoring
- Application Insights integration
- Custom metrics collection
- Performance profiling

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

## 🔮 Future Enhancements

- Multi-language support
- LinkedIn integration
- Interview preparation features
- ATS scoring simulation
- Analytics dashboard
- Mobile application
- Advanced AI models integration

---

**Built with ❤️ using .NET 9.0 and OpenAI's Agentic Platform**