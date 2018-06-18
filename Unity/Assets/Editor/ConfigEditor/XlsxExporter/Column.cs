using System;

namespace BKEditor.Config.Export
{
    public class Column
    {
        public int ColumnNum { get; }
        public string Name { get; }
        public string DefaultValue { get; }
		public string ColumnTypeString { get; }
        public IColumnType ColumnType { get; }
        public ConfigTypes ConfigTypes { get; }
        private IData defaultData = null;
        public IData DefaultData
        {
            get
            {
                if (defaultData == null)
                    defaultData = ColumnType.Parse(DefaultValue);
                return defaultData;
            }
        }
        private SheetView.Column SheetColumn { get; }
        
        public Column(DataView dataView, SheetView sheetView, int colNum)
        {
            ColumnNum = colNum;
	        SheetColumn = sheetView.GetColumn(colNum);
            Name = SheetColumn[2];
	        ColumnTypeString = SheetColumn[4];
			ColumnType = dataView.ColumnTypeParser.ParseColumnType(ColumnTypeString, Name);
            ConfigTypes = Utils.ParseConfigTypes(SheetColumn[5]);
            DefaultValue = SheetColumn[3] == "" ? ColumnType.DefaultValue : SheetColumn[3];
			//ID检查
	        if (colNum == 1)
	        {
		        if (Name != "Id")
		        {
			        throw new Exception("表中第一列名称必须为Id！");
		        }
		        if (!(ColumnType is IIdColumnType))
		        {
			        throw new Exception("表中第一列类型必须为Id类型！");
				}
			}
		}

        public IData ParseRow(int row)
        {
            return ColumnType.Parse(SheetColumn[row]);
        }
    }
}
