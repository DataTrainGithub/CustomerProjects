// var measureMetadata = ReadFile(@"C:\Users\MattiasDeConinck\Documents\Repositories\de-analytics\Mapping\MeasureMapping.csv");
 var measureMetadata = ReadFile(@"D:\a\de-analytics\de-analytics\Mapping\MeasureMapping.csv");

var csvRows = measureMetadata.Split(new [] {
    '\r',
    '\n'
}, StringSplitOptions.RemoveEmptyEntries);

// Delete previously generated measures
Model.Tables["M_Measures"].Measures.Where(m => m.GetAnnotation("GENERATEDBY") == "M_Functional".ToUpper()).ToList().ForEach(m => m.Delete());

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

    if (script.ToLower() == "M_Functional".ToLower()) {
        String[] d = new String[] {"^^"};
        var extraParametersList = extraParameters.Split(d, StringSplitOptions.None);
        var scope = extraParametersList[0].Trim();
        var baseMeasure = Model.Tables["M_Measures"].Measures.FirstOrDefault(m => m.Name.ToLower() == measureName.ToLower());
        var factTable = Model.Tables.FirstOrDefault(t => t.Name.ToUpper() == tableName.ToUpper());

        if (baseMeasure == null){
            // Comparative Buttons
            if (scope.ToLower() == "comparative_button"){
                var type = extraParametersList.Count() >= 2 ? extraParametersList[1].Trim() : null;
                var customText = extraParametersList.Count() >= 3 ? extraParametersList[2].Trim() : null;
                var MeasuresToCompare = extraParametersList.Count() >= 4 ? extraParametersList[3].Trim() : null;
                var ButtonAnnotation = "Button";
                var DelimitAnnotation = "_";
                var DisplayFolderButtons = "Functional Measures\\Buttons";
                var ButtonPrefix = customText;
                var MeasureTable = (Model.Tables["M_Measures"] as Table);

                String[] c = new String[] {","};
                var MeasuesToCompareList = MeasuresToCompare.Split(c, StringSplitOptions.None);
                var MeasureSum = String.Join(" + ", MeasuesToCompareList.Select(m => "[" + m + "]").ToList());
                var AppendedMeasureText = String.Join(" & \" | \" & ", MeasuesToCompareList.Select(m => "Format([" + m + "],\"#,###,#0\")").ToList());

                if (type.ToLower() == "valuta"){
                    
                    MeasureTable.AddMeasure(
                    ButtonAnnotation + DelimitAnnotation + "Comparative" + DelimitAnnotation + customText,
                        "Var Button_Text = \"" + ButtonPrefix + ": € \"\n" +
                        "Var MeasureSum = " + MeasureSum + "\n" +
                        "Var MeasureText = " + AppendedMeasureText + "\n" +
                        "Var Result = If(MeasureSum > 0, Button_Text & MeasureText, \"...\")\n" +
                        "Return Result",
                        DisplayFolderButtons
                    );
                }
        }
        } else {
            // Check if we need to use the old prefix name
            var baseMeasureName = baseMeasure.Name.StartsWith("M_") ? baseMeasure.Name.Substring(2) : baseMeasure.Name;
            var kpiCardPrefix = baseMeasure.Name.StartsWith("M_") ? "KPI_Card_" : "KPI_";
            var kpiCardName = kpiCardPrefix + baseMeasureName;

            // This Year
            if (baseMeasure != null && (scope.ToLower() == "all" || scope.ToLower() == "cards")) {

                var ThisPeriod = "Year";
                var ThisPeriodAnnotation = "Y";
                var PreviousPeriodAnnotation ="Y-1";
                var TodateAnnotation = "ToDate";
                var MetricAnnotation = "Metric";
                var CFAnnotation = "CF";
                var IconAnnotation = "Icon";
                var TooltipAnnotation = "Tooltip";
                var DelimitAnnotation = "_";
                var DiffAnnotation = "%";
                var DeltaAnnotation = "Delta";
                var DisplayFolderMetrics = "Functional Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\Metrics" ;
                var DisplayFolderCF = "Functional Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\CF";
                var DisplayFolderIcon = "Functional Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\Icon";
                var DisplayFolderTooltip = "Functional Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\Tooltip";
                var DisplayFolderDelta = "Functional Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\Delta";

                //Y
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                "\n" + "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";


                //YTD
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation,
                "\n" + "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Date]=today()-1),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";
                
                //Y-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" + "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,YEAR),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Y_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_LastFactDate = MAXX(ALL(" + factTable.Name + "[Date]), [Date])\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Date] = Date_LastFactDate),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate)) / (Date_LastDate - Date_FirstDate + 1)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Y-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation,
                    "\n" + "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR PrevPeriod_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) -1),D_Date[Date_Date]))\n" +
                    "VAR PrevPeriod_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) -1),D_Date[Date_Date]))\n" +
                    "VAR Result = DIVIDE(CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date], -1, YEAR), D_Date, Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))), (PrevPeriod_LastDate - PrevPeriod_FirstDate + 1))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Y-1_Todate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" + "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Date]=today()-1),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,YEAR),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Y-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_LastFactDate = MAXX(ALL(" + factTable.Name + "[Date]), [Date])\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Date]=Date_LastFactDate),D_Date[Date_Date]))\n" +
                    "VAR PrevPeriod_FirstDate = FIRSTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]), -1, YEAR))\n" +
                    "VAR PrevPeriod_LastDate = LASTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Date]=Date_LastFactDate),D_Date[Date_Date]), -1, YEAR))\n" +
                    "VAR Result = DIVIDE(CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date], -1, YEAR), D_Date, Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))), (PrevPeriod_LastDate - PrevPeriod_FirstDate + 1))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Y_YTD
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "YTD" + DelimitAnnotation + MetricAnnotation,
                    "\n" + "VAR Date_LastFactDate = MAXX(ALL("+ factTable.Name + "[Date]),[Date])\n" + 
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(ALL(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Date] <= Date_LastFactDate),D_Date[Date_Date]))\n" +
                    "VAR _result = TOTALYTD(CALCULATE(" + baseMeasure.DaxObjectName +", CALCULATETABLE(DATEADD(D_Date[Date_Date], 0, YEAR), D_Date, D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)), D_Date[Date_Date])\n" +
                    "RETURN if(MAX(D_Date[Date_Date]) <= Date_LastFactDate, _result) \n",  // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";
                
                //Y-1_YTD
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "YTD" + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(ALL(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR _result = TOTALYTD(CALCULATE(" + baseMeasure.DaxObjectName +", CALCULATETABLE(DATEADD(D_Date[Date_Date], -1, YEAR), D_Date, D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)), D_Date[Date_Date])\n" +
                    "RETURN _result \n",  // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //YvsY-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" + "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //YvsY-1_ToDate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" + "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //YvsY-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //YvsY-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //CF YvsY-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + CFAnnotation,
                    "\n" + "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF YvsY-1_Todate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + CFAnnotation,
                    "\n" + "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" +  kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF %Y-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation+ DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF %Y-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //Icon YvsY-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );


                //Icon YvsY-1_ToDate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon  
                );

                //Icon Y
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "\nVar Metric1 = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]" +
                    "\nVar Metric1_Text = \"K\"" +
                    //"\nVar Result = Format(Metric1, \"0,0\") & Metric1_Text" +
                    "\nVar Result = Format(Metric1, \"0,0\")" +
                    "\nReturn Result",
                DisplayFolderIcon  
                );  

                //Icon %Y-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation+ DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation+ DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );

                //Icon %Y-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );          

                //TooltipTitle 
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation  + DelimitAnnotation + TooltipAnnotation,
                    "\n" + "VAR ToolTip_Text = "+ '\u0022' + "General Stats for year: " + '\u0022' + "\n" +
                    "VAR Tooltip_Period = CALCULATE(Max(D_Date[Date_Year]),Filter(D_Date,D_Date[Date_Date] = Today()))\n" +
                    "VAR Result = ToolTip_Text & Tooltip_Period\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderTooltip 
                );
            }
            
            // This Quarter
            if (baseMeasure != null && (scope.ToLower() == "all" || scope.ToLower() == "cards")) {

                var ThisPeriod = "Quarter";
                var ThisPeriodAnnotation = "Q";
                var PreviousPeriodAnnotation ="Q-1";
                var PreviousYearAnnotation = "PY";
                var TodateAnnotation = "ToDate";
                var CFAnnotation = "CF";
                var MetricAnnotation = "Metric";
                var IconAnnotation = "Icon";
                var TooltipAnnotation = "Tooltip";
                var DelimitAnnotation = "_";
                var DiffAnnotation = "%";
                var DeltaAnnotation = "Delta";
                var DisplayFolderMetrics = "Functional Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\Metrics" ;
                var DisplayFolderCF = "Functional Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\CF";
                var DisplayFolderIcon = "Functional Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\Icon";
                var DisplayFolderTooltip = "Functional Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\Tooltip";
                var DisplayFolderDelta = "Functional Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\Delta";

                //Q
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Q_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_LastFactDate = MAXX(ALL(" + factTable.Name + "[Date]), [Date])\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) &&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = MIN(LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) &&D_Date[Date_Quarter]=QUARTER(now()) && D_Date[Date_Date] = Date_LastFactDate),D_Date[Date_Date])), Date_LastFactDate)\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ", REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate)) / (Date_LastDate - Date_FirstDate+1)\n" + 
                    "RETURN Result",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Q-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation,
                    "\n" + "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]))\n" +
                    "VAR PrevPeriod_FirstDate = FIRSTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]), -1, QUARTER))\n" +
                    "VAR PrevPeriod_LastDate = LASTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&& D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]), -1, QUARTER))\n" +
                    "VAR Result = DIVIDE(CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date], -1, QUARTER), D_Date, Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))), (PrevPeriod_LastDate - PrevPeriod_FirstDate + 1))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //QTD
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Date]=today()-1),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";
                
                //Q-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,QUARTER),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";
                
                //Q-1_ToDate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Date]=today()-1),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,QUARTER),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Q-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_LastFactDate = MAXX(ALL(" + factTable.Name + "[Date]), [Date])\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now()) && D_Date[Date_Date]=Date_LastFactDate),D_Date[Date_Date]))\n" +
                    "VAR PrevPeriod_FirstDate = FIRSTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]), -1, QUARTER))\n" +
                    "VAR PrevPeriod_LastDate = LASTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now()) && D_Date[Date_Date]=Date_LastFactDate),D_Date[Date_Date]), -1, QUARTER))\n" +
                    "VAR Result = DIVIDE(CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date], -1, QUARTER), D_Date, Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))), (PrevPeriod_LastDate - PrevPeriod_FirstDate + 1))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Q_QTD
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "QTD" + DelimitAnnotation + MetricAnnotation,
                    "\n" + "VAR Date_LastFactDate = MAXX(ALL("+ factTable.Name + "[Date]),[Date])\n" + 
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(ALL(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]))\n" +
                    "VAR _result = TOTALQTD(CALCULATE(" + baseMeasure.DaxObjectName + ", CALCULATETABLE(DATEADD(D_Date[Date_Date], 0 , QUARTER), D_Date, D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)), D_Date[Date_Date])\n" +
                    "RETURN if (MAX(D_Date[Date_Date]) <= Date_LastFactDate, _result)",  // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Q-1_QTD
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "QTD" + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(ALL(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Quarter]=quarter(now())),D_Date[Date_Date]))\n" +
                    "VAR _result = TOTALQTD(CALCULATE(" + baseMeasure.DaxObjectName + ", CALCULATETABLE(DATEADD(D_Date[Date_Date], -1 , QUARTER), D_Date, D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)), D_Date[Date_Date])\n" +
                    "RETURN _result",  // DAX expression
                    DisplayFolderMetrics
                ).FormatString = "#,0";

                //QPY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation  +  PreviousYearAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=Quarter(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=Quarter(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,YEAR),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";
                
                //QvsQ-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //QvsQ-1_Todate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //QvsQPY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation  +  PreviousYearAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //QvsQ-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //QvsQ-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //CF QvsQ-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF QvsQ-1_Todate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" +  kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF QvsQPY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF %Q-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation+ DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF %Q-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //Icon QvsQ-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );


                //Icon QvsQ-1_ToDate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon  
                );


                //Icon QvsQPY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );
                

                //Icon Q
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "Var Metric1 = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" + 
                    "VAR Metric2 = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    //"Var Metric1_Text = \"K  |  \"" + 
                    //"Var Metric2_Text = \"K / Day \"" +
                    "Var Metric1_Text = \" | \"" + 
                    "Var Metric2_Text = \" / Day\"" +
                    "Var Result1 = Format(Metric1, \"0,0\") & Metric1_Text & Format(Metric2, \"0,0\") & Metric2_Text\n" +
                    "Var Result2 = Format(Metric1, \"0,0\")\n" +
                    "Var Result = If(ISBLANK(Metric2), Result2, Result1)\n" +
                    "Return Result", // DAX expression
                DisplayFolderIcon   
                );

                //Icon %Q-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation+ DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation+ DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );

                //Icon %Q-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );

                //TooltipTitle 
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation  + DelimitAnnotation + TooltipAnnotation,
                    "\n" + "VAR ToolTip_Text = "+ '\u0022' + "General Stats for QTR: " + '\u0022' + "\n" +
                    "VAR Tooltip_Period = CALCULATE(Max(D_Date[Date_Quarter]),Filter(D_Date,D_Date[Date_Date] = Today()))\n" +
                    "VAR Result = ToolTip_Text & Tooltip_Period\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderTooltip 
                );
            }
            
            // This Month
            if (baseMeasure != null && (scope.ToLower() == "all" || scope.ToLower() == "cards")) {

                var ThisPeriod = "Month";
                var ThisPeriodAnnotation = "M";
                var PreviousPeriodAnnotation ="M-1";
                var PreviousYearAnnotation = "PY";
                var TodateAnnotation = "ToDate";
                var CFAnnotation = "CF";
                var MetricAnnotation = "Metric";
                var IconAnnotation = "Icon";
                var TooltipAnnotation = "Tooltip";
                var DelimitAnnotation = "_";
                var DiffAnnotation = "%";
                var DeltaAnnotation = "Delta";
                var DisplayFolderMetrics = "Functional Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\Metrics" ;
                var DisplayFolderCF = "Functional Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\CF";
                var DisplayFolderIcon = "Functional Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\Icon";
                var DisplayFolderTooltip = "Functional Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\Tooltip";
                var DisplayFolderDelta = "Functional Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\Delta";

                //M
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //M_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_LastFactDate = MAXX(ALL(" + factTable.Name + "[Date]), [Date])\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) &&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) &&D_Date[Date_Month]=Month(now()) && D_Date[Date_Date]=Date_LastFactDate),D_Date[Date_Date]))\n" +
                    "VAR Result = DIVIDE(CALCULATE(" + baseMeasure.DaxObjectName + ", REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate)), (Date_LastDate - Date_FirstDate + 1))\n" + 
                    "RETURN Result",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //M-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation,
                    "\n" + "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now())),D_Date[Date_Date]))\n" +
                    "VAR PrevPeriod_FirstDate = FIRSTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now())),D_Date[Date_Date]), -1, MONTH))\n" +
                    "VAR PrevPeriod_LastDate = LASTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&& D_Date[Date_Month]=month(now())),D_Date[Date_Date]), -1, MONTH))\n" +
                    "VAR Result = DIVIDE(CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date], -1, MONTH), D_Date, Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate && D_Date[Date_Date]<=Date_LastDate))), (PrevPeriod_LastDate - PrevPeriod_FirstDate + 1))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //MTD
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Date]=today()-1),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";
                
                //M-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,MONTH),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";
                
                //M-1_ToDate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Date]=today()-1),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,MONTH),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //M-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_LastFactDate = MAXX(ALL(" + factTable.Name + "[Date]), [Date])\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now()) && D_Date[Date_Date]=Date_LastFactDate),D_Date[Date_Date]))\n" +
                    "VAR PrevPeriod_FirstDate = FIRSTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now())),D_Date[Date_Date]), -1, month))\n" +
                    "VAR PrevPeriod_LastDate = LASTDATE(DATEADD(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now()) && D_Date[Date_Date]=Date_LastFactDate),D_Date[Date_Date]), -1, month))\n" +
                    "VAR Result = DIVIDE(CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date], -1, month), D_Date, Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))), (PrevPeriod_LastDate - PrevPeriod_FirstDate + 1))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //M_MTD
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "MTD" + DelimitAnnotation + MetricAnnotation,
                    "\n" + "VAR Date_LastFactDate = MAXX(ALL("+ factTable.Name + "[Date]),[Date])\n" + 
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(ALL(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now())),D_Date[Date_Date]))\n" +
                    "VAR _result = TOTALMTD(CALCULATE(" + baseMeasure.DaxObjectName + ", CALCULATETABLE(DATEADD(D_Date[Date_Date], 0 , Month), D_Date, D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)), D_Date[Date_Date])\n" +
                    "RETURN if (MAX(D_Date[Date_Date]) <= Date_LastFactDate, _result)",  // DAX expression
                    DisplayFolderMetrics  
                ).FormatString = "#,0";

                //M-1_MTD
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "MTD" + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(ALL(D_Date),D_Date[Date_Year]=year(now()) && D_Date[Date_Month]=month(now())),D_Date[Date_Date]))\n" +
                    "VAR _result = TOTALMTD(CALCULATE(" + baseMeasure.DaxObjectName + ", CALCULATETABLE(DATEADD(D_Date[Date_Date], -1 , Month), D_Date, D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)), D_Date[Date_Date])\n" +
                    "RETURN _result",  // DAX expression
                    DisplayFolderMetrics
                ).FormatString = "#,0";

                //MPY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation  +  PreviousYearAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,YEAR),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                
                //MvsM-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //MvsM-1_Todate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";


                //MvsMPY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation  +  PreviousYearAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //MvsM-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //MvsM-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation,
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation +  "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //CF MvsM-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF MvsM-1_Todate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" +  kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF MvsMPY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF %M-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation+ DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //CF %M-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = " + '\u0022' + "#A81222" + '\u0022' + "\n" +
                    "VAR Color_Orange = " + '\u0022' + "#ffa500" + '\u0022' + "\n" +
                    "VAR Color_Green = " + '\u0022' + "#57A812" + '\u0022' + "\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" + 
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderCF    
                );

                //Icon MvsM-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );


                //Icon MvsM-1_ToDate
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + TodateAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon  
                );

                //Icon MvsMPY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + ThisPeriodAnnotation+ PreviousYearAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );

                //Icon %M-1_ToDate_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation+ DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation+ DelimitAnnotation + TodateAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );

                //Icon %M-1_AVG
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + IconAnnotation,
                    "\n" + "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & " + '\u0022' + " " + '\u0022' +  "& FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "),Icon_Down & " + '\u0022' + " " + '\u0022' +  "&FORMAT(KPI_Icon," + '\u0022' + "Percent" + '\u0022'+ "))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );

                // Icon M
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" + "Var Metric1 = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Metric2 = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + "AVG" + DelimitAnnotation + MetricAnnotation + "]\n" +
                    // "Var Metric1_Text = \"K  |  \"\n" +
                    // "Var Metric2_Text = \"K / Day \"\n" +
                    "Var Metric1_Text = \" | \"\n" +
                    "Var Metric2_Text = \" / Day\"\n" +
                    "Var Result1 = Format(Metric1, \"0,0\") & Metric1_Text & Format(Metric2, \"0,0\") & Metric2_Text\n" +
                    "Var Result2 = Format(Metric1, \"0,0\")\n" +
                    "Var Result = If(ISBLANK(Metric2), Result2, Result1)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderIcon   
                );
                
                //TooltipTitle 
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation  + DelimitAnnotation + TooltipAnnotation,
                    "\n" + "VAR ToolTip_Text = "+ '\u0022' + "General Stats for Month: " + '\u0022' + "\n" +
                    "VAR Tooltip_Period = CALCULATE(Max(D_Date[Date_Month]),Filter(D_Date,D_Date[Date_Date] = Today()))\n" +
                    "VAR Result = ToolTip_Text & Tooltip_Period\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderTooltip 
                );
            }

            // Day Before Yesterday
            if (baseMeasure != null && (scope.ToLower() == "all" || scope.ToLower() == "cards")) {
            
                var ThisPeriod = "DayBeforeYesterDay";
                var ThisPeriodAnnotation = "DBY";
                var PreviousPeriodAnnotation ="DBY-1";
                var PreviousYearAnnotation = "PDBY";
                var TodateAnnotation = "ToDate";
                var CFAnnotation = "CF";
                var IconAnnotation = "Icon";
                var TooltipAnnotation = "Tooltip";
                var DelimitAnnotation = "_";
                var DiffAnnotation = "%";
                var DeltaAnnotation = "Delta";
                var MetricAnnotation = "Metric";
                var TitleAnnotation = "Title";
                var DescriptionAnnotation = "Description";
                var DisplayFolderMetrics = "Functional Measures\\Card_DayBeforeYesterDay\\" + baseMeasure.Name + "\\Metrics" ;
                var DisplayFolderCF = "Functional Measures\\Card_DayBeforeYesterDay\\" + baseMeasure.Name + "\\CF";
                var DisplayFolderIcon = "Functional Measures\\Card_DayBeforeYesterDay\\" + baseMeasure.Name + "\\Icon";
                var DisplayFolderTooltip = "Functional Measures\\Card_DayBeforeYesterDay\\" + baseMeasure.Name + "\\Tooltip";
                var DisplayFolderTitle = "Functional Measures\\Card_DayBeforeYesterDay\\" + baseMeasure.Name + "\\Title";
                var DisplayFolderDescription = "Functional Measures\\Card_DayBeforeYesterDay\\" + baseMeasure.Name + "\\Description";            
                var DisplayFolderDelta = "Functional Measures\\Card_DayBeforeYesterDay\\" + baseMeasure.Name + "\\Delta";

                // DBY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Max_Fact_Date = MAXX(ALL(" + factTable.Name + "[Date]), [Date])" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Date]=Max_Fact_Date),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                DisplayFolderMetrics    
                ).FormatString = "#,0";

                // DBY-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "VAR Max_Fact_Date = MAXX(ALL(" + factTable.Name + "[Date]), [Date])" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Date]=Max_Fact_Date - 1),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                DisplayFolderMetrics    
                ).FormatString = "#,0";

                // %DBY-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation +  PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "VAR Result = DIVIDE([" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "],[" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "])\n" +
                    "RETURN Result ",     // DAX expression
                DisplayFolderDelta    
                ).FormatString = "#,0";

                // Icon DBY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation +  ThisPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "Var Metric1 = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "Var Metric1_Text = \"K\"\n" +
                    //"Var Result = Format(Metric1, \"0,0\") & Metric1_Text\n" +
                    "Var Result = Format(Metric1, \"0,0\")\n" +
                    "RETURN Result ",     // DAX expression
                DisplayFolderIcon    
                ).FormatString = "#,0";

                // Icon %DBY-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "VAR Icon_Down = UNICHAR(9207)\n" +
                    "VAR Icon_Up = UNICHAR(9206)\n" +
                    "VAR KPI_Icon = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = If(KPI_Icon>=1,Icon_Up & \" \" & FORMAT(KPI_Icon,\"Percent\"),Icon_Down & \" \" & FORMAT(KPI_Icon,\"Percent\"))\n" +
                    "RETURN Result ",     // DAX expression
                DisplayFolderIcon    
                ).FormatString = "#,0";

                // CF %DBY-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + CFAnnotation,
                    "VAR Color_Red = \"#A81222\"\n" +
                    "VAR Color_Orange = \"#ffa500\"\n" +
                    "VAR Color_Green = \"#57A812\"\n" +
                    "VAR KPI_CF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR Result = SWITCH(TRUE(), KPI_CF>=1, Color_Green, KPI_CF > 0.8 , Color_Orange, KPI_CF> 0 , Color_Red)\n" +
                    "RETURN Result ",     // DAX expression
                DisplayFolderCF    
                ).FormatString = "#,0";

                // Title DBY
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + TitleAnnotation,
                    "VAR Max_Fact_Date = MAXX(ALL(" + factTable.Name + "[Date]), [Date])" +
                    "Var Result = \"Last \" & FORMAT(Max_Fact_Date, \"dddd\")\n" +
                    "RETURN Result ",     // DAX expression
                DisplayFolderTitle    
                ).FormatString = "#,0";

                // Description DBY-1
                baseMeasure.Table.AddMeasure(
                kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DescriptionAnnotation,
                    "VAR Max_Fact_Date = MAXX(ALL(" + factTable.Name + "[Date]), [Date])" +
                    "Var Result = \"Last \" & FORMAT(Max_Fact_Date - 1, \"dddd\")\n" +
                    "RETURN Result ",     // DAX expression
                DisplayFolderDescription    
                ).FormatString = "#,0";
            
            }
            

            // Buttons
            if (baseMeasure != null && (scope.ToLower() == "all" || scope.ToLower() == "button")) {
                var type = extraParametersList.Count() >= 2 ? extraParametersList[1].Trim() : null;
                var customText = extraParametersList.Count() >= 3 ? extraParametersList[2].Trim() : null;
                var ButtonAnnotation = "Button";
                var DelimitAnnotation = "_";
                var DisplayFolderButtons = "Functional Measures\\Buttons";
                var ButtonPrefix = customText == null ? baseMeasure.GetAnnotation("GENERATEDON").Replace('_', ' ') : customText;

                if (type.ToLower() == "valuta"){
                    
                    baseMeasure.Table.AddMeasure(
                    ButtonAnnotation + DelimitAnnotation + baseMeasure.Name,
                        "Var Button_Text = \"" + ButtonPrefix + ": € \"\n" +
                        "Var Button_Metric = " + baseMeasure.DaxObjectName + "\n" +
                        "Var Result = Button_Text & Format(Button_Metric,\"#,###,#0.00\")\n" +
                        "Return Result",
                        DisplayFolderButtons    
                    );
                } 

                else if (type.ToLower() == "valutanodecimals"){

                    baseMeasure.Table.AddMeasure(
                    ButtonAnnotation + DelimitAnnotation + baseMeasure.Name,
                        "Var Button_Text = \"" + ButtonPrefix + ": € \"\n" +
                        "Var Button_Metric = " + baseMeasure.DaxObjectName + "\n" +
                        "Var Result = IF(Button_Metric > 0, Button_Text & Format(Button_Metric,\"#,##0\"), \"...\")\n" +
                        "Return Result",
                        DisplayFolderButtons    
                    );
                }
                else if (type.ToLower() == "numeric"){

                    baseMeasure.Table.AddMeasure(
                    ButtonAnnotation + DelimitAnnotation + baseMeasure.Name,
                        "Var Button_Text = \"" + ButtonPrefix + ": \"\n" +
                        "Var Button_Metric = " + baseMeasure.DaxObjectName + "\n" +
                        "Var Result = Button_Text & Format(Button_Metric,\"#,##0,00\")\n" +
                        "Return Result",
                        DisplayFolderButtons    
                    );
                }
                else if (type.ToLower() == "percent"){
                    baseMeasure.Table.AddMeasure(
                    ButtonAnnotation + DelimitAnnotation + baseMeasure.Name,
                        "Var Button_Text = \"" + ButtonPrefix + ": \"\n" +
                        "Var Button_Metric = " + baseMeasure.DaxObjectName + "\n" +
                        "Var Result = Button_Text & Format(Button_Metric,\"Percent\")\n" +
                        "Return Result",
                        DisplayFolderButtons    
                    );
                }
            }
            
            // Upper Bound
            if (baseMeasure != null && (scope.ToLower() == "all" || scope.ToLower() == "upperbound")) {
                var groupByColumn = extraParametersList.Count() >= 2 ? extraParametersList[1].Trim() : null;
                var UpperBoundAnnotation = "UpperBound";
                var DelimitAnnotation = "_";
                var DisplayFolderUpperBound = "Functional Measures\\Upper_Bound";

                baseMeasure.Table.AddMeasure(
                    baseMeasure.Name + DelimitAnnotation + groupByColumn.Replace(" ", "") + DelimitAnnotation + UpperBoundAnnotation,
                        "Var Result = \n" + 
                        "SELECTCOLUMNS (TOPN (1, ADDCOLUMNS ( SUMMARIZE ( + \n" + 
                        factTable.Name + ", [" +  groupByColumn + "]),\n" + 
                        "\"A\", " + baseMeasure.DaxObjectName +  " * 1.2),[A], DESC),[A])\n" +
                        "Return Result",
                        DisplayFolderUpperBound    
                    );
            }
        }
        
        // Selected Date Measures
        var table = Model.Tables["M_Measures"];
        var selectedPeriodMeasures = table.Measures.Where(m => m.DisplayFolder.Contains("Selected_Date")).ToList();

        if (selectedPeriodMeasures == null || selectedPeriodMeasures.Count() == 0) {
            var displayFolderSelectedPeriod = "Functional Measures\\Selected_Date";
            table.AddMeasure(
                "Selected_Date_Low",
                "Var Result = MIN(D_Date_DynamicPeriods[Date])\n" +
                "RETURN Result",
                displayFolderSelectedPeriod
            ).FormatString = "dd/MM/yyyy";

            table.AddMeasure(
                "Selected_Date_High",
                "Var Result = MAX(D_Date_DynamicPeriods[Date])\n" + 
                "RETURN Result",
                displayFolderSelectedPeriod
            ).FormatString = "dd/MM/yyyy";

            table.AddMeasure(
                "Selected_Date",
                "FORMAT([Selected_Date_Low], \"dd/MM/yyyy\")&\" - \"&FORMAT([Selected_Date_High], \"dd/MM/yyyy\")",
                displayFolderSelectedPeriod
            );
        }
        
        // Add Annotations
        Model.Tables["M_Measures"].Measures.Where(m => m.DisplayFolder.Contains("Functional Measures")).ToList().ForEach(m => m.SetAnnotation("GENERATEDBY", script.ToUpper()));
        
        // Hide Metrics
        Model.Tables["M_Measures"].Measures.Where(m => m.DisplayFolder.Contains("Metrics") || m.DisplayFolder.Contains("Delta")).ToList().ForEach(m => m.IsHidden = true);
    }
}