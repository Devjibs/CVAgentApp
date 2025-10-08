using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CVAgentApp.API.Swagger;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile[]))
            .ToList();

        if (fileParams.Any())
        {
            // Check if this is a single file parameter (like AnalyzeCandidate)
            if (fileParams.Count == 1 && fileParams[0].ParameterType == typeof(IFormFile))
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["cvFile"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary",
                                        Description = "CV file (PDF, DOC, DOCX)"
                                    }
                                },
                                Required = new HashSet<string> { "cvFile" }
                            }
                        }
                    }
                };
            }
            else
            {
                // Multi-parameter form (like GenerateCV)
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["CVFile"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary",
                                        Description = "CV file (PDF, DOC, DOCX)"
                                    },
                                    ["JobPostingUrl"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "uri",
                                        Description = "Job posting URL"
                                    },
                                    ["CompanyName"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "Company name (optional)"
                                    }
                                },
                                Required = new HashSet<string> { "CVFile", "JobPostingUrl" }
                            }
                        }
                    }
                };
            }
        }
    }
}
