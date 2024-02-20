//var measureMetadata = ReadFile(@"C:\Users\MattiasDeConinck\Documents\Repositories\de-analytics\Mapping\MeasureMapping.csv");
var measureMetadata = ReadFile(@"D:\a\de-analytics\de-analytics\Mapping\MeasureMapping.csv");

var csvRows = measureMetadata.Split(new [] {
    '\r',
    '\n'
}, StringSplitOptions.RemoveEmptyEntries);

foreach(var row in csvRows.Skip(1)) {
    var csvColumns = row.Split(';');
    var modelName = csvColumns[0].Trim();
    var tableName = csvColumns[1].Trim();
    var measureName = csvColumns[2].Trim();
    var script = csvColumns[3].Trim();
    var extraParameters = csvColumns[4].Trim();

    // Check if Model names are equal
    if (Model.GetAnnotation("MappingModel").ToLower() != modelName.ToLower()){
        continue;
    }
    if (script.ToLower() == "M_FieldParameters".ToLower()) {
        String[] d = new String[] {"^^"};
        var extraParametersList = extraParameters.Split(d, StringSplitOptions.None);
        var inputObjects = extraParametersList[0].Split(',');
        var inputObjectNames = extraParametersList.Count() >= 2 ? extraParametersList[1].Split(',') : null;
        var groupings = extraParametersList.Count() >= 3 ? extraParametersList[2].Split(',') : null;
        var selectedObjects = new List<ITabularTableObject>();
        
        foreach (var inputObject in inputObjects){
            var baseMeasure = Model.Tables["M_Measures"].Measures.FirstOrDefault(m => m.Name.ToLower() == inputObject.ToLower());

            if (baseMeasure == null){

                var baseTableName = inputObject.Split('[')[0];
                var baseTable = Model.Tables[baseTableName];
                var baseColumnName = (inputObject.Split('[')[1]).Split(']')[0];
                var baseColumn = baseTable.Columns.FirstOrDefault(c => c.Name.ToLower() == baseColumnName.ToLower());

                selectedObjects.Add(baseColumn);
            }
            else {
                selectedObjects.Add(baseMeasure);
            }
        }
        
        var name = "Metric";
        // Construct the DAX for the calculated table based on the current selection:
        var objects = selectedObjects;

        var dax = "";

        if (inputObjectNames == null){
            if (groupings == null){
                dax = "{\n    " + string.Join(",\n    ", objects.Select((c,i) => string.Format("(\"{0}\", NAMEOF('{1}'[{2}]), {3})", c.Name.Remove(0, c.Name.IndexOf("_")).Replace("_", " ").Trim(), c.Table.Name, c.Name, i))) + "\n}";
            } else {
                dax = "{\n    " + string.Join(",\n    ", objects.Select((c,i) => string.Format("(\"{0}\", NAMEOF('{1}'[{2}]), {3}, \"{4}\")", c.Name.Remove(0, c.Name.IndexOf("_")).Replace("_", " ").Trim(), c.Table.Name, c.Name, i, groupings[i]))) + "\n}";
            }
        }
        else {
            if (groupings == null){
                dax = "{\n    " + string.Join(",\n    ", objects.Select((c,i) => string.Format("(\"{0}\", NAMEOF('{1}'[{2}]), {3})", inputObjectNames[i].Replace("_", " ") , c.Table.Name, c.Name, i))) + "\n}";
            } else {
                dax = "{\n    " + string.Join(",\n    ", objects.Select((c,i) => string.Format("(\"{0}\", NAMEOF('{1}'[{2}]), {3}, \"{4}\")", inputObjectNames[i].Replace("_", " ") , c.Table.Name, c.Name, i, groupings[i]))) + "\n}";
            }
        }

        // Add the calculated table to the model:
        var table = Model.AddCalculatedTable(tableName, dax);

        // In TE2 columns are not created automatically from a DAX expression, so 
        // we will have to add them manually:
        var te2 = table.Columns.Count == 0;
        var nameColumn = te2 ? table.AddCalculatedTableColumn(name, "[Value1]") : table.Columns["Value1"] as CalculatedTableColumn;
        var fieldColumn = te2 ? table.AddCalculatedTableColumn(name + " Fields", "[Value2]") : table.Columns["Value2"] as CalculatedTableColumn;
        var orderColumn = te2 ? table.AddCalculatedTableColumn(name + " Order", "[Value3]") : table.Columns["Value3"] as CalculatedTableColumn;
        CalculatedTableColumn groupingColumn = null;
        if (groupings != null){
            groupingColumn = te2 ? table.AddCalculatedTableColumn(name + " Grouping", "[Value4]") : table.Columns["Value4"] as CalculatedTableColumn;
        }

        if(!te2) {
            // Rename the columns that were added automatically in TE3:
            nameColumn.IsNameInferred = false;
            nameColumn.Name = name;
            fieldColumn.IsNameInferred = false;
            fieldColumn.Name = name + " Fields";
            orderColumn.IsNameInferred = false;
            orderColumn.Name = name + " Order";

            if (groupings != null){
                groupingColumn.IsNameInferred = false;
                groupingColumn.Name = name + " Grouping";
            }
        }
        // Set remaining properties for field parameters to work
        // See: https://twitter.com/markbdi/status/1526558841172893696
        nameColumn.SortByColumn = orderColumn;
        nameColumn.GroupByColumns.Add(fieldColumn);
        fieldColumn.SortByColumn = orderColumn;
        fieldColumn.SetExtendedProperty("ParameterMetadata", "{\"version\":3,\"kind\":2}", ExtendedPropertyType.Json);
        fieldColumn.IsHidden = true;
        orderColumn.IsHidden = true;
        if (groupings != null){
            groupingColumn.IsHidden = true;
        }

        table.SetAnnotation("GENERATEDBY", script.ToUpper());
    }
}
