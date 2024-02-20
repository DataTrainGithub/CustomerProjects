// Quality of Life...

foreach (Table table in Model.Tables){
    
    // Set refreshsourcexpression to the one found in the partition.
    bool rf_enabled = table.EnableRefreshPolicy;
    
    if (rf_enabled){
        table.SourceExpression = (table.Partitions[0] as MPartition).MExpression;
    }
    
    // Disabling summarizeBy for all columns
    foreach (var column in table.Columns) {
        column.SummarizeBy = AggregateFunction.None;
    }
}