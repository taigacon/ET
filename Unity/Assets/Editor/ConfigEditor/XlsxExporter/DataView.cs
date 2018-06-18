using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace BKEditor.Config.Export
{
    public class DataView
    {
		public string Name { get; }
        public SheetView MainSheetView { get; }
		public List<SheetView> SubSheetViews { get; } = new List<SheetView>();
		public ExcelPackage ExcelPackage { get; }
		public ColumnTypeParser ColumnTypeParser { get; } = new ColumnTypeParser();
		public List<Column> ColumnHeaders { get; private set; }
		private List<List<Column>> SubColumnHeaders { get; set; } = new List<List<Column>>();

		public DataView(string name, string filePath, ExcelPackage excel)
	    {
		    Name = name;
			ExcelPackage = excel;
		    ExcelWorksheet worksheet = excel.Workbook.Worksheets[name];
		    if (worksheet == null)
		    {
				throw new Exception($"沒有找到Worksheet {name}");
		    }
			MainSheetView = new ExcelSheetView(worksheet, filePath);
		    foreach (var ws in excel.Workbook.Worksheets)
		    {
			    if (ws.Name != name && ws.Name != "设置" && !ws.Name.StartsWith("~"))
			    {
				    SubSheetViews.Add(new ExcelSheetView(ws, filePath));
			    }
		    }
	    }

	    private Tuple<List<Column>, Dictionary<string, Column>> ReadHeader(SheetView sheetView)
	    {
		    try
		    {
			    List<Column> columns = new List<Column>();
			    for (int i = 1; i <= sheetView.MaxColumn; i++)
			    {
				    if (sheetView.IsStopColumn(i, 0))
					    break;
				    Column column;
				    try
				    {
						column = new Column(this, sheetView, i);
					}
				    catch (Exception e)
				    {
					    throw new Exception($"{e.Message}\n处理列{Utils.ColumnNum2Label(i)}失败", e);
				    }
					columns.Add(column);
			    }
			    Dictionary<string, Column> columnsDic = new Dictionary<string, Column>();
			    foreach (var column in columns)
			    {
				    if (columnsDic.ContainsKey(column.Name))
				    {
					    throw new Exception($"列{column.ColumnNum}和{columnsDic[column.Name].ColumnNum}有重复的键名{column.Name}");
				    }
				    columnsDic.Add(column.Name, column);
			    }

			    return new Tuple<List<Column>, Dictionary<string, Column>>(columns, columnsDic);
			}
		    catch (Exception e)
		    {
			    throw new Exception($"{e.Message}\n处理表{sheetView.Name}失败", e);
		    }
	    }

	    public void ReadHeader()
	    {
		    var mainTuple = ReadHeader(MainSheetView);
			ColumnHeaders = mainTuple.Item1;
		    var mainHeadersDic = mainTuple.Item2;
		    SubColumnHeaders.Clear();
			foreach (var sheetView in SubSheetViews)
		    {
			    var subTuple = ReadHeader(sheetView);
			    //检查多余字段
				foreach (var header in subTuple.Item1)
			    {
				    Column column;
				    if (mainHeadersDic.TryGetValue(header.Name, out column))
				    {
					    if (header.ColumnType.TypeName != column.ColumnType.TypeName ||
					        header.ColumnTypeString != column.ColumnTypeString)
					    {
							throw new Exception($"字表{sheetView.Name}第{Utils.ColumnNum2Label(header.ColumnNum)}字段{header.Name}类型{header.ColumnTypeString}和主表类型{column.ColumnTypeString}不符");
					    }
				    }
				    else
				    {
					    throw new Exception($"字表{sheetView.Name}第{Utils.ColumnNum2Label(header.ColumnNum)}列存在多余的字段{header.Name}");
				    }
				}
			    List<Column> subColumnHeader = new List<Column>();
				//重新排列顺序
			    foreach (var columnHeader in ColumnHeaders)
			    {
				    Column column;
				    subColumnHeader.Add(subTuple.Item2.TryGetValue(columnHeader.Name, out column) ? column : null);
			    }
				SubColumnHeaders.Add(subColumnHeader);
			}
	    }
    }
}
