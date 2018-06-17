namespace BK.Config.Export
{
    public class Column
    {
        public int ColumnNum { get; }
        public string Name { get; }
        public string DefaultValue { get; }
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
        
        public Column(Exporter exporter, int colNum)
        {
            ColumnNum = colNum;
            SheetColumn = exporter.SheetView.GetColumn(colNum);
            Name = SheetColumn[2];
            ColumnType = exporter.ColumnTypeParser.ParseColumnType(SheetColumn[4]);
            ConfigTypes = Utils.ParseConfigTypes(SheetColumn[5]);
            DefaultValue = SheetColumn[3] == "" ? ColumnType.DefaultValue : SheetColumn[3];
        }

        public IData ParseRow(int row)
        {
            return ColumnType.Parse(SheetColumn[row]);
        }
    }
}
