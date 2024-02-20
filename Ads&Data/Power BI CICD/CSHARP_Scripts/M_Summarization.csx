 // var measureMetadata = ReadFile(@"C:\Users\MattiasDeConinck\Documents\Repositories\de-analytics\Mapping\MeasureMapping.csv");
 var measureMetadata = ReadFile(@"D:\a\de-analytics\de-analytics\Mapping\MeasureMapping.csv");

var displayFolderSummarization = "Summarization Measures";
var displaySubFolderNumericSummarization = "\\Numeric";
var displaySubFolderNonNumericSummarization = "\\NonNumeric";

var csvRows = measureMetadata.Split(new [] {
    '\r',
    '\n'
}, StringSplitOptions.RemoveEmptyEntries);

// Delete previously generated measures
Model.Tables["M_Measures"].Measures.Where(m => m.GetAnnotation("GENERATEDBY") == "M_Summarization".ToUpper()).ToList().ForEach(m => m.Delete());

foreach(var row in csvRows.Skip(1)) {
    var csvColumns = row.Split(';');
    var modelName = csvColumns[0].Trim();
    var tableName = csvColumns[1].Trim();
    var columnName = csvColumns[2].Trim();
    var script = csvColumns[3].Trim();
    var extraParameters = csvColumns[4].Trim();
     
    // Check if Model names are equal
    if (Model.GetAnnotation("MappingModel").ToLower() != modelName.ToLower()){
        continue;
    }

    if (script.ToLower() == "M_Summarization".ToLower()) {
    
        var table = Model.Tables[tableName];
        var columns = table.Columns;
        var column = columns.FirstOrDefault(m => m.Name == columnName);
        var targetTable = Model.Tables["M_Measures"];

        String[] d = new String[] { "^^" };
        var extraParametersList = extraParameters.Split(d, StringSplitOptions.None);
        var type = extraParametersList[0].Trim();
        var formatString = extraParametersList.Count() >= 2 && extraParametersList[1] != "" ? extraParametersList[1].Trim() : null;
        var measureName = extraParametersList.Count() >= 3 && extraParametersList[2] != "" ? extraParametersList[2].Trim().ToLower() : columnName;
        var prefixFlag = extraParametersList.Count() >= 4 && extraParametersList[3] != "" ? bool.Parse(extraParametersList[3].Trim().ToLower()) : true;
        var hiddenFlag = extraParametersList.Count() >= 5 && extraParametersList[4] != "" ? bool.Parse(extraParametersList[4].Trim().ToLower()) : true;

        var prefix = prefixFlag ? "M_" : "";

        if (column != null && type.ToLower() == "numeric"){
                        
            targetTable.AddMeasure(
            prefix + measureName + "_Base",
            "SUM(" + tableName + "[" + columnName + "])",
            displayFolderSummarization + displaySubFolderNumericSummarization + "\\" + measureName
            );
            
            targetTable.AddMeasure(
            prefix + "AVG_" + measureName + "_Base",
            "AVERAGE(" + tableName + "[" + columnName + "])",
            displayFolderSummarization  + displaySubFolderNumericSummarization + "\\" + measureName
            );

            targetTable.AddMeasure(
            prefix + "MAX_" + measureName + "_Base",
            "MAX(" + tableName + "[" + columnName + "])",
            displayFolderSummarization  + displaySubFolderNumericSummarization + "\\" + measureName
            );

            targetTable.AddMeasure(
            prefix + "MIN_" + measureName + "_Base",
            "MIN(" + tableName + "[" + columnName + "])",
            displayFolderSummarization  + displaySubFolderNumericSummarization + "\\" + measureName
            );

        } else if (column != null && type.ToLower() == "non-numeric"){
            
            targetTable.AddMeasure(
            prefix + "#_" + measureName + "_Base",
            "COUNT(" + tableName + "[" + columnName + "])",
            displayFolderSummarization + displaySubFolderNonNumericSummarization + "\\" + measureName
            );

            targetTable.AddMeasure(
            prefix + "MAX_" + measureName + "_Base",
            "MAX(" + tableName + "[" + columnName + "])",
            displayFolderSummarization + displaySubFolderNonNumericSummarization + "\\" + measureName
            );
            targetTable.AddMeasure(
            prefix + "MIN_" + measureName + "_Base",
            "MIN(" + tableName + "[" + columnName + "])",
            displayFolderSummarization + displaySubFolderNonNumericSummarization + "\\" + measureName
            );
        }

        var measures = targetTable.Measures.Where(m => m.DisplayFolder.Contains(displayFolderSummarization) && m.DisplayFolder.Contains(measureName)  && m.GetAnnotation("GENERATEDBY") == null);
        foreach (var m in measures){
            m.SetAnnotation("GENERATEDBY", script.ToUpper());
            m.SetAnnotation("GENERATEDON", measureName);

            if (formatString != null){
                m.FormatString = formatString;
                m.SetAnnotation("FORMATSTRING", formatString);
            }
        }

        // Hide base column
        column.IsHidden = true;

        // Hide Metrics
        Model.Tables["M_Measures"].Measures.Where(m => m.GetAnnotation("GENERATEDBY") == "M_Summarization".ToUpper()).ToList().ForEach(m => m.IsHidden = hiddenFlag);
    }

}