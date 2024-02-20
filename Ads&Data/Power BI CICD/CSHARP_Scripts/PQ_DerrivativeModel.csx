
// Get Environment Variable
var json = Environment.GetEnvironmentVariable("CONFIG");
var config = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

foreach (var mapping in config["mapping"])
{
    string stringToReplace = mapping["source"];
    string replacementString = mapping["target"];

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