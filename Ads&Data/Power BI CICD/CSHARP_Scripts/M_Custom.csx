// var measureMetadata = ReadFile(@"C:\Users\MattiasDeConinck\Documents\Repositories\de-analytics\Mapping\MeasureMapping.csv");
using System.Text.RegularExpressions;

var measureMetadata = ReadFile(@"D:\a\de-analytics\de-analytics\Mapping\MeasureMapping.csv");

var displayFolderCustom = "Custom Measures";
var csvRows = measureMetadata.Split(new [] {
    '\r',
    '\n'
}, StringSplitOptions.RemoveEmptyEntries);

// Delete previously generated measures
Model.Tables["M_Measures"].Measures.Where(m => m.GetAnnotation("GENERATEDBY") == "M_Custom".ToUpper()).ToList().ForEach(m => m.Delete());

// Regular expression for splitting on semicolons only when they are outside double quotes
var semicolonSplit = @";(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

foreach(var row in csvRows.Skip(1)) {
    var csvColumns = Regex.Split(row, semicolonSplit);
    var modelName = csvColumns[0].Trim();
    var measureName = csvColumns[2].Trim();
    var script = csvColumns[3].Trim();
    var extraParameters = csvColumns[4].Trim();

    // Check if Model names are equal
    if (Model.GetAnnotation("MappingModel").ToLower() != modelName.ToLower()){
        continue;
    }
    
    if (script.ToLower() == "M_Custom".ToLower()) {
        
        String[] d = new String[] {"^^"};
        var extraParametersList = extraParameters.Split(d, StringSplitOptions.None);
        var daxExpression = extraParametersList[0];
        var formatString = extraParametersList.Count() >= 2 ? extraParametersList[1].Trim('"') : null;
        var alternativeDisplayFolder = extraParametersList.Count() >= 3 ? extraParametersList[2].Trim('"') : null;
        var table = Model.Tables["M_Measures"];
        var inalternativefolder = alternativeDisplayFolder == null ? "false" : "true";
        var measureNameSuffix = alternativeDisplayFolder == null ? "_Custom" : "";
        displayFolderCustom = alternativeDisplayFolder == null ? displayFolderCustom : alternativeDisplayFolder;

        
    
            // Custom Metric
            var baseMeasure = table.AddMeasure(
                "M_" + measureName + measureNameSuffix,
                daxExpression,
                displayFolderCustom
            );

            if (formatString != null){
                baseMeasure.FormatString = formatString;
                baseMeasure.SetAnnotation("FORMATSTRING", formatString);
            }

            baseMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());
            baseMeasure.SetAnnotation("GENERATEDON", measureName);
            baseMeasure.SetAnnotation("ALTERNATIVEFOLDER", inalternativefolder);
    }

    // Hide Metrics
    Model.Tables["M_Measures"].Measures.Where(m => m.GetAnnotation("GENERATEDBY") == "M_Custom".ToUpper() && m.GetAnnotation("ALTERNATIVEFOLDER") == "false").ToList().ForEach(m => m.IsHidden = true);
}