var AnalyzeInExcelPerspective = Model.Perspectives.FirstOrDefault(p => p.Name.ToLower() == "analyzeinexcel");

if (AnalyzeInExcelPerspective != null) {

    AnalyzeInExcelPerspective.Delete();
}

Model.AddPerspective("AnalyzeInExcel");

var tables = Model.Tables.Where(t => t.IsHidden == false);

foreach (var t in tables){
    t.InPerspective["AnalyzeInExcel"] = true; 
    
    var measures = t.Measures.Where(m => !m.DisplayFolder.Contains("Analytics"));
    foreach (var m in measures){
        m.InPerspective["AnalyzeInExcel"] = false;
    }
}