// var measureMetadata = ReadFile(@"C:\Users\MattiasDeConinck\Documents\Repositories\de-analytics\Mapping\MeasureMapping.csv");
var measureMetadata = ReadFile(@"D:\a\de-analytics\de-analytics\Mapping\MeasureMapping.csv");

var displayFolderAnalytics = "Analytics Measures";
var csvRows = measureMetadata.Split(new [] {
    '\r',
    '\n'
}, StringSplitOptions.RemoveEmptyEntries);

// Delete previously generated measures
Model.Tables["M_Measures"].Measures.Where(m => m.GetAnnotation("GENERATEDBY") == "M_Analytics".ToUpper()).ToList().ForEach(m => m.Delete());

foreach(var row in csvRows.Skip(1)) {
    var csvColumns = row.Split(';');
    var modelName = csvColumns[0].Trim();
    var columnName = csvColumns[2].Trim();
    var script = csvColumns[3].Trim();
    var extraParameters = csvColumns[4].Trim();

    // Check if Model names are equal
    if (Model.GetAnnotation("MappingModel").ToLower() != modelName.ToLower()){
        continue;
    }
    
    if (script.ToLower() == "M_Analytics".ToLower()) {
        String[] d = new String[] {"^^"};
        var extraParametersList = extraParameters.Split(d, StringSplitOptions.None);
        var periods = extraParametersList[0].Trim().Split(',');
        var selectedMeasure = extraParametersList.Count() >= 2 ? extraParametersList[1].Trim() : null;
        var useTimeIntelligence = extraParametersList.Count() >= 3 ? Convert.ToBoolean(extraParametersList[2].Trim()) : true;
        var table = Model.Tables["M_Measures"];
        var measures = table.Measures.Where(m => (m.GetAnnotation("GENERATEDBY") == "M_Summarization".ToUpper() || m.GetAnnotation("GENERATEDBY") == "M_Custom".ToUpper()) && m.GetAnnotation("GENERATEDON").ToUpper() == columnName.ToUpper()).ToList();
        
        foreach (var measure in measures){
            var measureList = measure.Name.Split('_');
            var measureName = String.Join("_", measureList.Take(measureList.Length - 1));

            var baseMeasureExpression = useTimeIntelligence ? "CALCULATE(" + measure.DaxObjectName + ", USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ))" : measure.DaxObjectName;
            var measureExpression = selectedMeasure != null && selectedMeasure != "" ? selectedMeasure.Replace("SELECTEDMEASURE()", baseMeasureExpression) : baseMeasureExpression;
            var formatString =  measure.GetAnnotation("FORMATSTRING") != null ? measure.GetAnnotation("FORMATSTRING") : "#,0";

            // Default Metric
            var baseMeasure = table.AddMeasure(
                measureName,
                measureExpression,
                displayFolderAnalytics + "\\" + columnName
            );

            baseMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());
            baseMeasure.SetAnnotation("GENERATEDON", columnName);
            baseMeasure.FormatString = formatString;

            if (periods != null){
                foreach (var p in periods){
                    if (p == ""){
                        continue;
                    }
                    var basePeriodMeasureExpression = "CALCULATE(" + measure.DaxObjectName + ", 'D_TimeIntelligence'[Type]=\"" +  p + "\")";
                    var periodMeasureExpression = selectedMeasure != null ? selectedMeasure.Replace("SELECTEDMEASURE()", basePeriodMeasureExpression) : basePeriodMeasureExpression;
                    var newMeasure = table.AddMeasure(
                        measureName + "_" + p,
                        periodMeasureExpression,
                        displayFolderAnalytics + "\\" + columnName + "\\" + p
                    );

                    newMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());
                    newMeasure.SetAnnotation("GENERATEDON", columnName);
                    newMeasure.FormatString = formatString;
                } 
            }

            // Delta Measures
            var analyticsMeaures = table.Measures.Where(m => m.GetAnnotation("GENERATEDBY") == "M_Analytics".ToUpper() && m.GetAnnotation("GENERATEDON").ToUpper() == columnName.ToUpper());

            // %Y-1
            var PYMeasure = analyticsMeaures.FirstOrDefault(m => m.Name.ToLower() == (measureName + "_PY").ToLower());
            if (PYMeasure != null){
                table.AddMeasure(
                    measureName + "_%Y-1",
                    "VAR _division = DIVIDE(" + baseMeasure.DaxObjectName + ", " + PYMeasure.DaxObjectName + ")\n" +
                    "VAR _result = IF (_division > 5, \"> 500 %\", FORMAT(_division, \"Percent\"))\n" + 
                    "return _result",
                    displayFolderAnalytics + "\\" + columnName + "\\%Y-1"
                );
            }

            // %Y-1_ToDate
            var YTDPYMeasure = analyticsMeaures.FirstOrDefault(m => m.Name.ToLower() == (measureName + "_YTDPY").ToLower());
            if (YTDPYMeasure != null){
                table.AddMeasure(
                    measureName + "_%Y-1_ToDate",
                    "VAR _division = DIVIDE(" + baseMeasure.DaxObjectName + ", " + YTDPYMeasure.DaxObjectName + ")\n" +
                    "VAR _result = IF (_division > 5, \"> 500 %\", FORMAT(_division, \"Percent\"))\n" + 
                    "return _result",
                    displayFolderAnalytics + "\\" + columnName + "\\%Y-1_ToDate"
                );
            }

            // %Q-1
            var PQMeasure = analyticsMeaures.FirstOrDefault(m => m.Name.ToLower() == (measureName + "_PQ").ToLower());
            if (PQMeasure != null){
                table.AddMeasure(
                    measureName + "_%Q-1",
                    "VAR _division = DIVIDE(" + baseMeasure.DaxObjectName + ", " + PQMeasure.DaxObjectName + ")\n" +
                    "VAR _result = IF (_division > 5, \"> 500 %\", FORMAT(_division, \"Percent\"))\n" + 
                    "return _result",
                    displayFolderAnalytics + "\\" + columnName + "\\%Q-1"
                );
            }

            // %Q-1_ToDate
            var QTDPYMeasure = analyticsMeaures.FirstOrDefault(m => m.Name.ToLower() == (measureName + "_QTDPY").ToLower());
            if (QTDPYMeasure != null){
                table.AddMeasure(
                    measureName + "_%Q-1",
                    "VAR _division = DIVIDE(" + baseMeasure.DaxObjectName + ", " + QTDPYMeasure.DaxObjectName + ")\n" +
                    "VAR _result = IF (_division > 5, \"> 500 %\", FORMAT(_division, \"Percent\"))\n" + 
                    "return _result",
                    displayFolderAnalytics + "\\" + columnName + "\\%Q-1_ToDate"
                );
            }
        }
    }
}  