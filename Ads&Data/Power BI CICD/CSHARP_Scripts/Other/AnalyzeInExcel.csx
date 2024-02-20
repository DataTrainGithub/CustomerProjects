var sharedExpressionName = "DirectQuery_to_AS";
var modelName = Model.GetAnnotation("MappingModel");
var expressions = Model.Expressions.ToList();
var relationships = Model.Relationships.ToList();
var tables = Model.Tables.ToList();

// Save Relationship info in order to recreate them after the deletion of the original tables and columns
List<Dictionary<string, object>> listHash = new List<Dictionary<string, object>>();

foreach (var relationship in relationships)
{
    Dictionary<string, object> hash = new Dictionary<string, object>();

    hash.Add("FromCardinality", relationship.FromCardinality);
    hash.Add("ToCardinality", relationship.ToCardinality);
    hash.Add("FromColumn", relationship.FromColumn.Name);
    hash.Add("FromTable", relationship.FromColumn.Table.Name);
    hash.Add("ToColumn", relationship.ToColumn.Name);
    hash.Add("ToTable", relationship.ToColumn.Table.Name);
    hash.Add("CrossFilteringBehavior", relationship.CrossFilteringBehavior);
    hash.Add("SecurityFilteringBehavior", relationship.SecurityFilteringBehavior);
    hash.Add("IsActive", relationship.IsActive);
    
    listHash.Add(hash);
}

// Hide Field Parameters
string[] scripts = {"m_fieldparameters"};
var tablesToHide = Model.Tables.Where(t => t.GetAnnotations().Contains("GENERATEDBY") && scripts.Contains(t.GetAnnotation("GENERATEDBY").ToLower())).ToList();

foreach (var table in tablesToHide){
    if (table != null) {
            table.IsHidden = true;
    }

}

foreach (var expression in expressions)
{
    expression.Delete();
}

var expressionText = "" +
    "let\n" +
    "   Source = AnalysisServices.Database(\"powerbi://api.powerbi.com/v1.0/myorg/DaIP_Datasets_PRD\", \"" + modelName + "\"),\n" +
    "   Cubes = Table.Combine(Source[Data]),\n" + 
    "   Cube = Cubes{[Id=\"Model\", Kind=\"Cube\"]}[Data]\n" +
    "in\n" +
    "   Cube";

var namedExpression = NamedExpression.CreateNew(Model, sharedExpressionName);
namedExpression.Kind = ExpressionKind.M;
namedExpression.Expression = expressionText;
namedExpression.SetAnnotation("PBI_IncludeFutureArtifacts", "True");

foreach (var table in tables)
{

    var originalTableName = table.Name;
    var newTableName = "New_" + originalTableName;
    var newTable = Model.AddTable(newTableName);
    
    newTable.IsHidden = table.IsHidden;

    foreach(var column in table.Columns){
        var newColumn = newTable.AddDataColumn(column.Name, column.Name, column.DisplayFolder, column.DataType);
        newColumn.FormatString = column.FormatString;
        newColumn.IsHidden = column.IsHidden;

    }

    foreach(var measure in table.Measures){
        var originalMeasureName = measure.Name;
        var newMeasureName = "New_" + originalMeasureName;
        measure.Clone(newMeasureName, true, newTable);
    }

    // Creating the directquerypartition
    var directQueryPartition = (EntityPartition.CreateNew(newTable, "DirectQueryPartition") as EntityPartition);
    directQueryPartition.Mode = ModeType.DirectQuery;
    directQueryPartition.ExpressionSource = namedExpression;
    directQueryPartition.EntityName = originalTableName;
    var partitions = newTable.Partitions;

    foreach (var partition in partitions.ToList())
    {
        if (partition.Name != "DirectQueryPartition"){
            partition.Delete();
        }
    }

    table.Delete();
}

var newTables = Model.Tables.ToList();

foreach (var table in newTables)
{
    table.Name = table.Name.Substring(4, table.Name.Length - 4);

    foreach(var measure in table.Measures){
        measure.Name = measure.Name.Substring(4, measure.Name.Length - 4);
    }
}

foreach (var item in listHash)
{
    var fromTableString = (string)item["FromTable"];
    var fromColumnString = (string)item["FromColumn"];
    var fromColumn = Model.Tables[fromTableString].Columns[fromColumnString];

    var toTableString = (string)item["ToTable"];
    var toColumnString = (string)item["ToColumn"];
    var toColumn = Model.Tables[toTableString].Columns[toColumnString];

    var newRelationship = Model.AddRelationship();
    newRelationship.FromCardinality = (RelationshipEndCardinality)item["FromCardinality"];
    newRelationship.ToCardinality = (RelationshipEndCardinality)item["ToCardinality"];
    newRelationship.FromColumn = fromColumn;
    newRelationship.ToColumn = toColumn;
    newRelationship.CrossFilteringBehavior = (CrossFilteringBehavior)item["CrossFilteringBehavior"];
    newRelationship.SecurityFilteringBehavior = (SecurityFilteringBehavior)item["SecurityFilteringBehavior"];
    newRelationship.IsActive = (bool)item["IsActive"];

}

// Hide Functional Measures 
Model.Tables["M_Measures"].Measures.Where(m => m.DisplayFolder.Contains("Functional Measures")).ToList().ForEach(m => m.IsHidden = true);
