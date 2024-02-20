// var measureMetadata = ReadFile(@"C:\Users\MattiasDeConinck\Documents\Repositories\de-analytics\Mapping\DatasourceMapping.csv");
var measureMetadata = ReadFile(@"D:\a\de-analytics\de-analytics\Mapping\DatasourceMapping.csv");

var displayFolderCustom = "Custom Measures";
var csvRows = measureMetadata.Split(new [] {
    '\r',
    '\n'
}, StringSplitOptions.RemoveEmptyEntries);

foreach(var row in csvRows.Skip(1)) {
    var csvColumns = row.Split(';');
    var modelName = csvColumns[0].Trim();
    var devDatasource = csvColumns[1].Trim();
    var uatDatasource = csvColumns[2].Trim();
    var prdDatasource = csvColumns[3].Trim();

    // Check if Model names are equal
    if (Model.GetAnnotation("MappingModel").ToLower() != modelName.ToLower()){
        continue;
    }

    // Get Environment Variable
    var env = Environment.GetEnvironmentVariable("POWERBI_ENV");
    var environment = env != null ? env : "UAT";

    var stringToReplace = devDatasource;
    var replacementString = environment == "PRD" ? prdDatasource : uatDatasource;

    var shared_expressions = Model.Expressions;
    var tables = Model.Tables;
    var partitions = Model.AllPartitions.OfType<MPartition>();

    foreach (var expression in shared_expressions)
    {
        expression.Expression = expression.Expression.Replace(stringToReplace, replacementString);
    }

    foreach (var table in tables)
    {
        if (table.SourceExpression != null){
            table.SourceExpression = table.SourceExpression.Replace(stringToReplace, replacementString);
        }
    }

    foreach (var partition in partitions)
    {
        partition.Expression = partition.Expression.Replace(stringToReplace, replacementString);
    }
}