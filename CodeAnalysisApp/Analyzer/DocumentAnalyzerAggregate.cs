using Microsoft.CodeAnalysis;

namespace CodeAnalysisApp.Analyzer
{
	public class DocumentAnalyzerAggregate
	{
		public Document Document { get; set; }

		public SyntaxTree SyntaxTree { get; set; }

		public SemanticModel SemanticModel { get; set; }		

		public string DocumentTypeFullName { get; set; }
	}
}
