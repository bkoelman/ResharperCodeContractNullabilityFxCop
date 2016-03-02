using System.IO;
using CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage;
using CodeContractNullabilityFxCopRules.SymbolAnalysis.Symbols;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.ExternalAnnotations
{
    /// <summary>
    /// Attempts to find and parse a side-by-side [AssemblyName].ExternalAnnotations.xml file that resides in the same folder
    /// as the assembly that contains the requested symbol.
    /// </summary>
    public static class AssemblyExternalAnnotationsLoader
    {
        [CanBeNull]
        public static string GetPathForExternalSymbolOrNull([NotNull] ISymbol symbol)
        {
            Guard.NotNull(symbol, "symbol");

            string folder = Path.GetDirectoryName(symbol.ContainingAssemblyPath);
            if (folder != null)
            {
                string assemblyFileName = Path.GetFileNameWithoutExtension(symbol.ContainingAssemblyPath);
                string annotationFilePath = Path.Combine(folder, assemblyFileName + ".ExternalAnnotations.xml");

                return File.Exists(annotationFilePath) ? annotationFilePath : null;
            }

            return null;
        }

        [NotNull]
        public static ExternalAnnotationsMap ParseFile([NotNull] string externalAnnotationsPath)
        {
            Guard.NotNull(externalAnnotationsPath, "externalAnnotationsPath");

            using (TextReader reader = File.OpenText(externalAnnotationsPath))
            {
                var map = new ExternalAnnotationsMap();

                var parser = new ExternalAnnotationDocumentParser();
                parser.ProcessDocument(reader, map);

                return map;
            }
        }
    }
}