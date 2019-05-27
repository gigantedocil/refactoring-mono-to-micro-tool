using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeAnalysisApp.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisApp.Refactorings.Concrete
{
    public class LocalCallToRemoteSynchronousCallRefactoring : IRefactoringStrategy
    {
        private ISet<DocumentAnalyzerAggregate> documentsRegistry;

        private ISet<DocumentAnalyzerAggregate> documentsToCopy = new HashSet<DocumentAnalyzerAggregate>();

        private DocumentAnalyzerAggregate startup;

        private DocumentAnalyzerAggregate invokedMethodDocument;

        private string invokedMethodName;

        private ParameterListSyntax invokedMethodParameters;

        private readonly string ProjectName = "MonolithDemo";

        private readonly string ClassName = "RoomsService.cs";

        private readonly string MethodName = "CalculatePrice";

        private readonly string MicroserviceDirectoryPath = @"C:\Users\Me\Desktop";

        public async Task ApplyRefactoring(Solution solution)
        {
            documentsRegistry = await InitializeDocumentRegistry(solution);

            invokedMethodDocument = await GetInvokationMethodType(solution);

            documentsToCopy.Add(invokedMethodDocument);

            await RecursiveMethod(invokedMethodDocument);

            var invokedMethodProjectName = invokedMethodDocument.DocumentTypeFullName.Split('.').FirstOrDefault();

            startup = documentsRegistry.FirstOrDefault(x => x.DocumentTypeFullName.Contains(invokedMethodProjectName + ".Startup"));

            CreateMicroserviceDirectory();
        }

        private string sourcePath;

        private string microserviceName;

        private string microserviceSourceNamespace;

        private void CreateMicroserviceDirectory()
        {
            microserviceName = MethodName + "Microservice";

            microserviceSourceNamespace = microserviceName + ".Source";

            var basePath = MicroserviceDirectoryPath + "\\" + microserviceName;
            sourcePath = basePath + "\\Source";

            if (!Directory.Exists(sourcePath))
            {
                ExecuteCommand(@"cd " + MicroserviceDirectoryPath + " && dotnet new webapi -n " + MethodName + "Microservice && exit");
            }

            Directory.CreateDirectory(sourcePath);

            if (Directory.Exists(sourcePath))
            {
                foreach (var document in documentsToCopy)
                {
                    var path = document.Document.FilePath;

                    var fileName = document.Document.FilePath.Split('\\').LastOrDefault();

                    var newPath = sourcePath + "\\" + fileName;

                    if (File.Exists(path) && !File.Exists(newPath))
                    {
                        File.Copy(path, newPath);
                    }
                }
            }

            var startupPath = basePath + "\\Startup.cs";

            if (File.Exists(startupPath))
            {
                File.Delete(basePath + "\\Startup.cs");
            }

            File.Copy(startup.Document.FilePath, basePath + "\\Startup.cs");

            // Copy this inside the AddController method.
            var controllerName = invokedMethodDocument.Document.Name.First() == 'I' ?
                invokedMethodDocument.Document.Name.Split('I').ElementAt(1).Replace("Service", "").Split('.').FirstOrDefault() :
                invokedMethodDocument.Document.Name.Replace("Service", "").Split('.').FirstOrDefault();

            AddController(
                invokedMethodDocument.DocumentTypeFullName,
                MethodName + "Microservice",
                controllerName,
                invokedMethodDocument.Document.Name.Split('.').FirstOrDefault(),
                invokedMethodDocument.Document.Name.Split('.').FirstOrDefault().ToLowerInvariant(),
                invokedMethodDocument.Document.Name,
                invokedMethodName,
                invokedMethodParameters.ToString(),
                invokedMethodName,
                basePath
            );

            ReplaceNamespace();
        }

        private void ReplaceNamespace()
        {
            if (Directory.Exists(sourcePath))
            {
                var files = Directory.GetFiles(sourcePath);

                foreach (var filePath in files)
                {
                    var fileContentLines = new List<string>(File.ReadAllLines(filePath));

                    var namespaceIndex = fileContentLines.FindIndex(line => line.Contains("namespace"));

                    fileContentLines.RemoveAt(namespaceIndex);

                    fileContentLines.Insert(namespaceIndex, "namespace " + microserviceSourceNamespace);

                    var fileContent = string.Join("\n", fileContentLines);

                    File.WriteAllText(filePath, fileContent);
                }
            }
        }

        private void AddController(
            string serviceNamespace,
            string newNamespace,
            string controllerName,
            string serviceType,
            string serviceParameterName,
            string serviceName,
            string methodName,
            string inboundParameterType,
            string calledMethodName,
            string path)
        {
            var controllerPath = path + @"\Controllers\CalculatePricingController.cs";

            var templateControllerFile = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Refactorings\Concrete\TemplateController.txt");

            var requestData = "";

            var methodCall = "";

            var list = inboundParameterType.Replace("(", "").Replace(")", "").Replace(", ", ",").Split(',');

            for (int i = 0; i < list.Count(); i++)
            {
                var item = list.ElementAt(i);

                if (i == (list.Count() - 1))
                {
                    requestData += $"\t\tpublic {item} {{ get; set; }}";
                    methodCall += "requestData." + item.Split(' ').ElementAt(1);
                }
                else
                {
                    requestData += $"\t\tpublic {item} {{ get; set; }}\n";
                    methodCall += "requestData." + item.Split(' ').ElementAt(1) + ", ";
                }
            }

            var controllerFile = templateControllerFile
                .Replace("{serviceNamespace}", serviceName)
                .Replace("{newNamespace}", newNamespace)
                .Replace("{controllerName}", controllerName)
                .Replace("{serviceType}", serviceType)
                .Replace("{serviceParameterName}", serviceParameterName)
                .Replace("{serviceName}", serviceName)
                .Replace("{methodName}", methodName)
                .Replace("{inboundParameters}", inboundParameterType)
                .Replace("{calledMethodName}", calledMethodName)
                .Replace("{parameters}", requestData)
                .Replace("{inboundParameterName}", methodCall);

            if (File.Exists(controllerPath))
            {
                File.WriteAllText(controllerPath, string.Empty);
            }

            File.AppendAllText(controllerPath, controllerFile);
        }

        private void ExecuteCommand(string command)
        {
            ProcessStartInfo processInfo;

            processInfo = new ProcessStartInfo("cmd.exe", "/K " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            var process = Process.Start(processInfo);

            process.WaitForExit();
        }

        private async Task<HashSet<DocumentAnalyzerAggregate>> InitializeDocumentRegistry(Solution solution)
        {
            var documents = new HashSet<DocumentAnalyzerAggregate>();

            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    documents.Add(
                        new DocumentAnalyzerAggregate()
                        {
                            Document = document,
                            SyntaxTree = await document.GetSyntaxTreeAsync(),
                            SemanticModel = await document.GetSemanticModelAsync(),
                            DocumentTypeFullName = await GetNameFromDocument(document)
                        }
                    );
                }
            }

            return documents;
        }

        private async Task<string> GetNameFromDocument(Document document)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();

            var root = syntaxTree.GetRoot();

            string typeName = null;

            string nameSpace = null;

            var namespaceDeclaration = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            if (namespaceDeclaration != null)
            {
                nameSpace = namespaceDeclaration.Name.ToString();
            }

            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

            if (classDeclaration == null)
            {
                var interfaceDeclaration = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();

                if (interfaceDeclaration != null)
                {
                    typeName = interfaceDeclaration.Identifier.ToString();
                }
            }
            else
            {
                typeName = classDeclaration.Identifier.ToString();
            }

            if (nameSpace == null || typeName == null)
            {
                return "";
            }

            return nameSpace + "." + typeName;
        }

        private async Task<DocumentAnalyzerAggregate> GetInvokationMethodType(Solution solution)
        {
            var project = solution.Projects.Where(p => p.Name == ProjectName).FirstOrDefault();

            var document = project.Documents.Where(d => d.Name == ClassName).FirstOrDefault();

            var semanticModel = await document.GetSemanticModelAsync();

            var syntaxTree = await document.GetSyntaxTreeAsync();

            var methodInvocations = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();

            var invokedMethod = methodInvocations.FirstOrDefault(x => x.ToString().Contains(MethodName));

            var invokedMethodMetadata = semanticModel.GetSymbolInfo(invokedMethod).Symbol;

            invokedMethodName = invokedMethodMetadata.Name;

            var typeFullName = invokedMethodMetadata.ContainingType.ToDisplayString();

            var invokedMethodDocument = documentsRegistry.FirstOrDefault(x => x.DocumentTypeFullName == typeFullName);

            invokedMethodParameters = invokedMethodDocument.SyntaxTree.GetRoot()
                .DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(x => x.ToString().Contains(MethodName)).ParameterList;

            return invokedMethodDocument;
        }

        private async Task RecursiveMethod(DocumentAnalyzerAggregate document)
        {
            var treeRoot = await document.SyntaxTree.GetRootAsync();

            // We're assuming only one class per file.
            var isClass = treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault() != null;

            var isInterface = treeRoot.DescendantNodes().OfType<InterfaceDeclarationSyntax>().FirstOrDefault() != null;

            if (isClass)
            {
                await FindTypeDependenciesForType(document);
            }
            else if (isInterface)
            {
                var interfaceImplementations = await GetInterfaceImplementations(document);

                foreach (var interfaceImplementation in interfaceImplementations)
                {
                    if (!documentsToCopy.Contains(interfaceImplementation))
                    {
                        documentsToCopy.Add(interfaceImplementation);

                        await FindTypeDependenciesForType(interfaceImplementation);
                    }
                }
            }
        }

        private async Task FindTypeDependenciesForType(DocumentAnalyzerAggregate document)
        {
            var root = await document.SyntaxTree.GetRootAsync();

            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

            foreach (var item in classDeclaration.DescendantNodes())
            {
                string typeName = null;

                if (classDeclaration != null)
                {
                    switch (item)
                    {
                        case SimpleBaseTypeSyntax simpleBaseTypeSyntax:
                            typeName = simpleBaseTypeSyntax.Type.ToString();
                            break;
                        case ParameterSyntax parameterSyntax:
                            typeName = parameterSyntax.Type.ToString();
                            break;
                        case IdentifierNameSyntax identifierNameSyntax:
                            typeName = identifierNameSyntax.ToString();
                            break;
                        case SimpleNameSyntax simpleNameSyntax:
                            typeName = simpleNameSyntax.ToString();
                            break;
                        case TypeSyntax typeSyntax:
                            typeName = typeSyntax.ToString();
                            break;
                    }

                    if (typeName != null)
                    {
                        var documentToCopy = documentsRegistry.FirstOrDefault(x => x.DocumentTypeFullName.Contains(typeName));

                        if (documentToCopy != null && !documentsToCopy.Contains(documentToCopy))
                        {
                            documentsToCopy.Add(documentToCopy);

                            await RecursiveMethod(documentToCopy);
                        }
                    }
                }
            }
        }

        private async Task<HashSet<DocumentAnalyzerAggregate>> GetInterfaceImplementations(DocumentAnalyzerAggregate interfaceDocument)
        {
            var implementations = new HashSet<DocumentAnalyzerAggregate>();

            foreach (var document in documentsRegistry)
            {
                var root = await document.SyntaxTree.GetRootAsync();

                // We only care about classes implementing interfaces and not interfaces implementing interfaces.
                var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

                if (classDeclaration != null)
                {
                    var name = interfaceDocument.Document.Name.Split('.')[0];

                    var documentLines = classDeclaration.ToString().Split('\n');

                    var classDeclarationLine = documentLines.FirstOrDefault(x => x.Contains(name));

                    if (classDeclarationLine != null)
                    {
                        var classExtensions = classDeclarationLine.Split(':');

                        if (classExtensions.Length > 1)
                        {
                            var classExtensionsList = classExtensions[1].Split(',');

                            if (classExtensionsList.FirstOrDefault(x => x.Contains(name)) != null)
                            {
                                implementations.Add(document);
                            }
                        }
                    }
                }
            }

            return implementations;
        }
    }
}
