using OfficeOpenXml;
using System.IO;

namespace BK.Config.Export
{
    public class Exporter
    {
        public ColumnTypeParser ColumnTypeParser { get; } = new ColumnTypeParser();
        private string FilePath { get; }
        private string OutputPath { get; }
        private ExcelPackage excelPackage { get; }

        public Exporter(string filePath, string outPath)
        {
            FilePath = filePath;
            OutputPath = outPath;

            excelPackage = new ExcelPackage(new FileInfo(filePath));
            SheetView = new XLSSheetView()
        }
    }
}
