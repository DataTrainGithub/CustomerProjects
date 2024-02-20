foreach (var table in Model.Tables){

    foreach (var column in table.Columns){
    
        if (column.DataType.ToString() == "DateTime"){
            column.FormatString = "dd/MM/yyyy";
        };
    }
}