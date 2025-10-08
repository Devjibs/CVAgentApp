# CV Agent Desktop - AI-Powered CV Generation

CV Agent Desktop is a standalone Windows application that uses OpenAI's AgentKit and Agents SDK to automatically generate tailored CVs and cover letters based on job postings. The application analyzes uploaded CVs, extracts job requirements from URLs, and creates personalized documents that match specific roles while maintaining factual accuracy.

## Features

- **AI-Powered CV Analysis**: Extracts structured information from uploaded CVs (PDF, DOCX)
- **Job Posting Analysis**: Automatically extracts requirements and company information from job URLs
- **Tailored Document Generation**: Creates CVs and cover letters that match specific job requirements
- **Company Research**: Uses web search to gather company information for personalized cover letters
- **Document Preview & Download**: Preview generated documents before downloading
- **Session Management**: Track generation progress and manage multiple sessions
- **Security & Compliance**: Implements guardrails to prevent hallucinations and ensure data privacy

## System Requirements

- Windows 10 or later
- .NET 9.0 Runtime (included in the application)
- Internet connection for AI processing
- Minimum 4GB RAM
- 500MB free disk space

## Installation

### Option 1: Pre-built Installer

1. Download the latest release from the releases page
2. Run `install.bat` to install the application
3. Double-click `CVAgentApp.Desktop.exe` to start the application

### Option 2: Build from Source

1. Clone the repository:

   ```bash
   git clone <repository-url>
   cd CV-Agent-App
   ```

2. Build the application:

   ```bash
   build.bat
   ```

3. The built application will be in the `dist` folder

## Usage

1. **Start the Application**: Double-click `CVAgentApp.Desktop.exe`
2. **Upload CV**: Select a PDF or Word document containing your CV
3. **Enter Job URL**: Provide the URL of the job posting you want to apply for
4. **Optional Company Name**: Specify the company name if not auto-detectable
5. **Generate**: Click "Generate Tailored CV & Cover Letter"
6. **Download**: Preview and download the generated documents

## Architecture

The application follows a clean architecture pattern with the following layers:

- **CVAgentApp.Desktop**: Windows desktop application with embedded web server
- **CVAgentApp.API**: ASP.NET Core Web API with controllers and services
- **CVAgentApp.Core**: Domain entities, DTOs, and interfaces
- **CVAgentApp.Infrastructure**: Data access, external services, and implementations

## Technology Stack

- **Desktop**: Windows Forms with embedded web server
- **Backend**: ASP.NET Core 9.0, Entity Framework Core, OpenAI API
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap 5
- **Database**: SQL Server LocalDB (embedded)
- **Storage**: Local file system for document storage
- **Document Processing**: iTextSharp (PDF), DocumentFormat.OpenXml (Word), QuestPDF
- **AI Services**: OpenAI GPT-4o for analysis and generation

## Configuration

The application uses the following configuration (in `appsettings.json`):

```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CVAgentAppDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## API Endpoints

The embedded web server provides the following endpoints:

### CV Generation

- `POST /api/CVGeneration/generate` - Generate tailored CV and cover letter
- `GET /api/CVGeneration/session/{sessionToken}` - Get session status
- `GET /api/CVGeneration/download/{documentId}` - Download generated document
- `DELETE /api/CVGeneration/session/{sessionToken}` - Delete session

### Analysis

- `POST /api/CVGeneration/analyze-candidate` - Analyze CV file
- `POST /api/CVGeneration/analyze-job` - Analyze job posting

## Security Features

- **Input Validation**: File type and size validation
- **Data Encryption**: Encrypted storage of sensitive documents
- **Session Management**: Secure session tokens with expiration
- **Guardrails**: AI safety checks to prevent hallucinations
- **Data Retention**: Automatic cleanup of expired sessions and documents

## Development

### Project Structure

```
src/
├── CVAgentApp.Desktop/        # Desktop application
│   ├── Services/              # Web server service
│   ├── UI/                    # Windows Forms UI
│   └── wwwroot/               # Web UI files
├── CVAgentApp.API/            # Web API project
│   ├── Controllers/           # API controllers
│   └── Services/              # Background services
├── CVAgentApp.Core/           # Domain layer
│   ├── Entities/              # Domain entities
│   ├── DTOs/                  # Data transfer objects
│   └── Interfaces/            # Service interfaces
└── CVAgentApp.Infrastructure/ # Infrastructure layer
    ├── Data/                  # Entity Framework context
    └── Services/              # Service implementations
```

### Building the Application

1. **Restore packages**:

   ```bash
   dotnet restore
   ```

2. **Build the solution**:

   ```bash
   dotnet build --configuration Release
   ```

3. **Publish the desktop application**:

   ```bash
   dotnet publish "src\CVAgentApp.Desktop\CVAgentApp.Desktop.csproj" --configuration Release --runtime win-x64 --self-contained true
   ```

4. **Run the build script**:
   ```bash
   build.bat
   ```

### Adding New Features

1. **Domain Changes**: Update entities in `CVAgentApp.Core`
2. **Database**: Create migrations for schema changes
3. **Services**: Implement business logic in `CVAgentApp.Infrastructure`
4. **API**: Add controllers and endpoints in `CVAgentApp.API`
5. **Desktop**: Update Windows Forms UI in `CVAgentApp.Desktop`

## Deployment

The application is designed to be distributed as a standalone executable:

1. **Build the application** using the build script
2. **Package the distribution** folder
3. **Distribute to users** who can install and run locally

No server hosting or cloud deployment is required.

## Monitoring and Logging

- **Serilog**: Structured logging with console and file outputs
- **Application Insights**: Optional Azure monitoring integration
- **Health Checks**: Built-in health check endpoints
- **Session Cleanup**: Automatic cleanup of expired sessions

## Troubleshooting

### Common Issues

1. **Application won't start**: Check that .NET 9.0 Runtime is installed
2. **AI processing fails**: Verify OpenAI API key is configured correctly
3. **Database errors**: Ensure SQL Server LocalDB is installed
4. **File upload issues**: Check file size and format requirements

### Logs

Application logs are stored in:

- Console output (for debugging)
- `logs/cv-agent-{date}.txt` (file logging)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:

- Create an issue in the repository
- Check the troubleshooting section
- Review the application logs

## Roadmap

- [ ] Multi-language support
- [ ] LinkedIn integration
- [ ] Interview preparation features
- [ ] ATS scoring simulation
- [ ] Analytics dashboard
- [ ] Mobile app support
- [ ] Cloud sync capabilities
