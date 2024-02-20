//var measureMetadata = ReadFile(@"C:\Users\MattiasDeConinck\Documents\Repositories\de-analytics\Mapping\MeasureMapping.csv");
var measureMetadata = ReadFile(@"D:\a\de-analytics\de-analytics\Mapping\MeasureMapping.csv");

var csvRows = measureMetadata.Split(new [] {
    '\r',
    '\n'
}, StringSplitOptions.RemoveEmptyEntries);

// Delete previously generated measures
Model.Tables["M_Measures"].Measures.Where(m => m.GetAnnotation("GENERATEDBY") == "M_HTML".ToUpper()).ToList().ForEach(m => m.Delete());

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

    if (script.ToLower() == "M_HTML".ToLower()) {
        String[] d = new String[] {"^^"};
        var extraParametersList = extraParameters.Split(d, StringSplitOptions.None);
        var compareTo = extraParametersList[0].Trim();
        var scope = extraParametersList.Count() >= 2 ? extraParametersList[1].Trim() : null;
        var baseMeasure = Model.Tables["M_Measures"].Measures.FirstOrDefault(m => m.Name.ToLower() == measureName.ToLower());
        var compareToMeasure = Model.Tables["M_Measures"].Measures.FirstOrDefault(m => m.Name.ToLower() == compareTo.ToLower());
        var baseMeasureName = baseMeasure.Name.StartsWith("M_") ? baseMeasure.Name.Substring(2) : baseMeasure.Name;
        var compareToMeasureName = compareToMeasure.Name.StartsWith("M_") ? compareToMeasure.Name.Substring(2) : compareToMeasure.Name;

        // HTML_KPI_Cards
        if (baseMeasure != null && compareToMeasure != null && scope.ToLower() == "html_card"){
            
            var kpiCardPrefix = "HTML_KPI_Card_";
            var kpiCardName = kpiCardPrefix + baseMeasureName;

            // This Year
            if (true)
            {
                var ThisPeriod = "Year";
                var ThisPeriodAnnotation = "Y";
                var PreviousPeriodAnnotation ="Y-1";
                var CompareToAnnotation = "C";
                var TodateAnnotation = "ToDate";
                var MetricAnnotation = "Metric";
                var CFAnnotation = "CF";
                var IconAnnotation = "Icon";
                var TooltipAnnotation = "Tooltip";
                var DelimitAnnotation = "_";
                var DiffAnnotation = "%";
                var DeltaAnnotation = "Delta";
                var YTDAnnotation = "YTD";
                var DisplayFolderMetrics = "HTML Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\Metrics" ;
                var DisplayFolderCF = "HTML Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\CF";
                var DisplayFolderIcon = "HTML Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\Icon";
                var DisplayFolderTooltip = "HTML Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\Tooltip";
                var DisplayFolderDelta = "HTML Measures\\Card_ThisYear\\" + baseMeasure.Name + "\\Delta";
                
                //Y
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" + 
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //YTD
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + YTDAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" + 
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = TODAY() - 1\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //CY
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" + 
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + compareToMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //CYTD
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + CompareToAnnotation + YTDAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" + 
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = TODAY() - 1\n" +
                    "VAR Result = CALCULATE(" + compareToMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Y-1
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" + 
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,YEAR),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //YTD-1
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + YTDAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" + 
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = TODAY() - 1\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,YEAR),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //YvsY-1
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" + 
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //YTDvsYTD-1
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + YTDAnnotation + DelimitAnnotation +  DeltaAnnotation,
                    "\n" + 
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + YTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + YTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //YvsCY
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + DiffAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" + 
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //YTDvsCYTD
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + DiffAnnotation + CompareToAnnotation + YTDAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" + 
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + YTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + CompareToAnnotation + YTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //HTML
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" +
                    "VAR BASE_METRIC = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + YTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR COMPARETODIFF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + CompareToAnnotation + YTDAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR METRICDIFF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + YTDAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    @"
                    VAR MIN_TRESHOLD = 0.85
                    VAR MAX_TRESHOLD = 1
                    VAR BG_CIRCLE =
                        SWITCH(
                            TRUE(), 
                            COMPARETODIFF >= MAX_TRESHOLD, ""#479E8C"",
                            COMPARETODIFF < MAX_TRESHOLD && MIN_TRESHOLD <= COMPARETODIFF, ""#ffcc00"",
                            COMPARETODIFF < MIN_TRESHOLD, ""#E76B61""
                        ) 
                    VAR BG_LIGHT_CIRCLE =
                        SWITCH(
                            TRUE(), 
                            COMPARETODIFF >= MAX_TRESHOLD, ""#80CCBC"",
                            COMPARETODIFF < MAX_TRESHOLD && MIN_TRESHOLD <= COMPARETODIFF, ""#ffeb99"",
                            COMPARETODIFF < MIN_TRESHOLD, ""#F5BEB7""
                        )
                    VAR BG_TEXT =
                        SWITCH(
                            TRUE(), 
                            METRICDIFF >= MAX_TRESHOLD, ""#479E8C"",
                            METRICDIFF < MAX_TRESHOLD && MIN_TRESHOLD <= METRICDIFF, ""#ffcc00"",
                            METRICDIFF < MIN_TRESHOLD, ""#E76B61""
                        )
                    RETURN
                    ""<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 48' overflow='visible'>
                        <rect rx='5' width='95%' height='95%' style='stroke:lightgray; stroke-width:0.3; fill:white;' transform-origin='50% 50%' transform='scale(0.98)' />
                        <path d='M 18 2.0845
                            a 15.9155 15.9155 0 0 1 0 31.831
                            a 15.9155 15.9155 0 0 1 0 -31.831' fill='""&BG_CIRCLE&""' transform='translate(5 6)' />
                        <path d='M 18 2.0845
                            a 15.9155 15.9155 0 0 1 0 31.831
                            a 15.9155 15.9155 0 0 1 0 -31.831' fill='none' stroke='""&BG_LIGHT_CIRCLE&""' stroke-width='4' stroke-dasharray='""&COMPARETODIFF * 100&"", 100'
                            transform='translate(5 6) rotate(-180 18 18)' />
                        <text x='18' y='13' fill='white' font-size='6' text-anchor='middle' font-family='tahoma'
                            transform='translate(5 6)'>BY</text>
                        <text x='18' y='22' fill='white' font-size='7' font-weight='bold' text-anchor='middle' font-family='tahoma'
                            transform='translate(5 6)'>""&FORMAT(COMPARETODIFF, ""0%"")&""</text>
                        <text x='40' y='10' fill='black' font-size='6' font-family='tahoma' transform='translate(5 6)'>Year to Date</text>
                        <text x='40' y='17' fill='black' font-size='6' font-weight='bold' font-family='tahoma'
                            transform='translate(5 6)'>""&FORMAT(BASE_METRIC, ""0,0"")&""</text>
                        <text x='40' y='26' fill='""&BG_TEXT&""' font-size='6' font-weight='bold' font-family='tahoma'
                            transform='translate(5 6)'>""&FORMAT(METRICDIFF, ""0%"")&"" vs Y-1</text>
                    </svg>
                    """,
                    DisplayFolderIcon
                ).DataCategory = "ImageUrl";
            }

            // This Quarter
            if (true)
            {
                var ThisPeriod = "Quarter";
                var ThisPeriodAnnotation = "Q";
                var CompareToAnnotation = "C";
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
                var QTDAnnotation = "QTD";
                var DisplayFolderMetrics = "HTML Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\Metrics" ;
                var DisplayFolderCF = "HTML Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\CF";
                var DisplayFolderIcon = "HTML Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\Icon";
                var DisplayFolderTooltip = "HTML Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\Tooltip";
                var DisplayFolderDelta = "HTML Measures\\Card_ThisQTR\\" + baseMeasure.Name + "\\Delta";

                //Q
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //QTD
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + QTDAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = TODAY() -1\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //CQ
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + compareToMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //CQTD
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + CompareToAnnotation + QTDAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = TODAY() -1\n" +
                    "VAR Result = CALCULATE(" + compareToMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //Q-1
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,YEAR),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //QTD-1
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + QTDAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Quarter]=QUARTER(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = TODAY() - 1\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),CALCULATETABLE(DATEADD(D_Date[Date_Date],-1,YEAR),Filter(all(D_Date),D_Date[Date_Date] >= Date_FirstDate && D_Date[Date_Date] <= Date_LastDate)))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //QvsQ-1
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" +
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //QvsCQ
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + DiffAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" +
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //QTDvsQTD-1
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + QTDAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" +
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + QTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + QTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //QTDvsCQTD
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + DiffAnnotation + CompareToAnnotation + QTDAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" +
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + QTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + CompareToAnnotation + QTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //HTML
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" +
                    "VAR BASE_METRIC = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + QTDAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR COMPARETODIFF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + CompareToAnnotation + QTDAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR METRICDIFF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + QTDAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    @"
                    VAR MIN_TRESHOLD = 0.85
                    VAR MAX_TRESHOLD = 1
                    VAR BG_CIRCLE =
                        SWITCH(
                            TRUE(), 
                            COMPARETODIFF >= MAX_TRESHOLD, ""#479E8C"",
                            COMPARETODIFF < MAX_TRESHOLD && MIN_TRESHOLD <= COMPARETODIFF, ""#ffcc00"",
                            COMPARETODIFF < MIN_TRESHOLD, ""#E76B61""
                        ) 
                    VAR BG_LIGHT_CIRCLE =
                        SWITCH(
                            TRUE(), 
                            COMPARETODIFF >= MAX_TRESHOLD, ""#80CCBC"",
                            COMPARETODIFF < MAX_TRESHOLD && MIN_TRESHOLD <= COMPARETODIFF, ""#ffeb99"",
                            COMPARETODIFF < MIN_TRESHOLD, ""#F5BEB7""
                        )
                    VAR BG_TEXT =
                        SWITCH(
                            TRUE(), 
                            METRICDIFF >= MAX_TRESHOLD, ""#479E8C"",
                            METRICDIFF < MAX_TRESHOLD && MIN_TRESHOLD <= METRICDIFF, ""#ffcc00"",
                            METRICDIFF < MIN_TRESHOLD, ""#E76B61""
                        )
                    RETURN
                    ""<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 48' overflow='visible'>
                        <rect rx='5' width='95%' height='95%' style='stroke:lightgray; stroke-width:0.3; fill:white;' transform-origin='50% 50%' transform='scale(0.98)' />
                        <path d='M 18 2.0845
                            a 15.9155 15.9155 0 0 1 0 31.831
                            a 15.9155 15.9155 0 0 1 0 -31.831' fill='""&BG_CIRCLE&""' transform='translate(5 6)' />
                        <path d='M 18 2.0845
                            a 15.9155 15.9155 0 0 1 0 31.831
                            a 15.9155 15.9155 0 0 1 0 -31.831' fill='none' stroke='""&BG_LIGHT_CIRCLE&""' stroke-width='4' stroke-dasharray='""&COMPARETODIFF * 100&"", 100'
                            transform='translate(5 6) rotate(-180 18 18)' />
                        <text x='18' y='13' fill='white' font-size='6' text-anchor='middle' font-family='tahoma'
                            transform='translate(5 6)'>BQ</text>
                        <text x='18' y='22' fill='white' font-size='7' font-weight='bold' text-anchor='middle' font-family='tahoma'
                            transform='translate(5 6)'>""&FORMAT(COMPARETODIFF, ""0%"")&""</text>
                        <text x='40' y='10' fill='black' font-size='6' font-family='tahoma' transform='translate(5 6)'>Quarter to Date</text>
                        <text x='40' y='17' fill='black' font-size='6' font-weight='bold' font-family='tahoma'
                            transform='translate(5 6)'>""&FORMAT(BASE_METRIC, ""0,0"")&""</text>
                        <text x='40' y='26' fill='""&BG_TEXT&""' font-size='6' font-weight='bold' font-family='tahoma'
                            transform='translate(5 6)'>""&FORMAT(METRICDIFF, ""0%"")&"" vs Y-1</text>
                    </svg>
                    """,
                    DisplayFolderIcon
                ).DataCategory = "ImageUrl";
            }

            // This Month
            if (true)
            {
                var ThisPeriod = "Month";
                var ThisPeriodAnnotation = "M";
                var CompareToAnnotation = "C";
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
                var DisplayFolderMetrics = "HTML Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\Metrics" ;
                var DisplayFolderCF = "HTML Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\CF";
                var DisplayFolderIcon = "HTML Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\Icon";
                var DisplayFolderTooltip = "HTML Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\Tooltip";
                var DisplayFolderDelta = "HTML Measures\\Card_ThisMonth\\" + baseMeasure.Name + "\\Delta";

                //M
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + baseMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //CM
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" +
                    "VAR Date_FirstDate = FIRSTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Date_LastDate = LASTDATE(SUMMARIZE(filter(All(D_Date),D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())),D_Date[Date_Date]))\n" +
                    "VAR Result = CALCULATE(" + compareToMeasure.DaxObjectName + ",REMOVEFILTERS(),Filter(All(D_Date[Date_Date]),D_Date[Date_Date]>=Date_FirstDate&&D_Date[Date_Date]<=Date_LastDate))\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderMetrics    
                ).FormatString = "#,0";

                //M-1
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + MetricAnnotation,
                    "\n" +
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

                //MvsCM
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + DiffAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + DeltaAnnotation,
                    "\n" +
                    "VAR Numerator = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Denominator = [" + kpiCardName + DelimitAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR Result = DIVIDE(Numerator,Denominator)\n" +
                    "RETURN Result ",     // DAX expression
                    DisplayFolderDelta    
                ).FormatString = "0.00%;-0.00%;0.00%";

                //HTML
                baseMeasure.Table.AddMeasure(
                    kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + IconAnnotation,
                    "\n" +
                    "VAR BASE_METRIC = [" + kpiCardName + DelimitAnnotation + ThisPeriodAnnotation + DelimitAnnotation + MetricAnnotation + "]\n" +
                    "VAR COMPARETODIFF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + CompareToAnnotation + ThisPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    "VAR METRICDIFF = [" + kpiCardName + DelimitAnnotation + DiffAnnotation + PreviousPeriodAnnotation + DelimitAnnotation + DeltaAnnotation + "]\n" +
                    @"
                    VAR MIN_TRESHOLD = 0.85
                    VAR MAX_TRESHOLD = 1
                    VAR BG_CIRCLE =
                        SWITCH(
                            TRUE(), 
                            COMPARETODIFF >= MAX_TRESHOLD, ""#479E8C"",
                            COMPARETODIFF < MAX_TRESHOLD && MIN_TRESHOLD <= COMPARETODIFF, ""#ffcc00"",
                            COMPARETODIFF < MIN_TRESHOLD, ""#E76B61""
                        ) 
                    VAR BG_LIGHT_CIRCLE =
                        SWITCH(
                            TRUE(), 
                            COMPARETODIFF >= MAX_TRESHOLD, ""#80CCBC"",
                            COMPARETODIFF < MAX_TRESHOLD && MIN_TRESHOLD <= COMPARETODIFF, ""#ffeb99"",
                            COMPARETODIFF < MIN_TRESHOLD, ""#F5BEB7""
                        )
                    VAR BG_TEXT =
                        SWITCH(
                            TRUE(), 
                            METRICDIFF >= MAX_TRESHOLD, ""#479E8C"",
                            METRICDIFF < MAX_TRESHOLD && MIN_TRESHOLD <= METRICDIFF, ""#ffcc00"",
                            METRICDIFF < MIN_TRESHOLD, ""#E76B61""
                        )
                    RETURN
                    ""<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 48' overflow='visible'>
                        <rect rx='5' width='95%' height='95%' style='stroke:lightgray; stroke-width:0.3; fill:white;' transform-origin='50% 50%' transform='scale(0.98)' />
                        <path d='M 18 2.0845
                            a 15.9155 15.9155 0 0 1 0 31.831
                            a 15.9155 15.9155 0 0 1 0 -31.831' fill='""&BG_CIRCLE&""' transform='translate(5 6)' />
                        <path d='M 18 2.0845
                            a 15.9155 15.9155 0 0 1 0 31.831
                            a 15.9155 15.9155 0 0 1 0 -31.831' fill='none' stroke='""&BG_LIGHT_CIRCLE&""' stroke-width='4' stroke-dasharray='""&COMPARETODIFF * 100&"", 100'
                            transform='translate(5 6) rotate(-180 18 18)' />
                        <text x='18' y='13' fill='white' font-size='6' text-anchor='middle' font-family='tahoma'
                            transform='translate(5 6)'>BM</text>
                        <text x='18' y='22' fill='white' font-size='7' font-weight='bold' text-anchor='middle' font-family='tahoma'
                            transform='translate(5 6)'>""&FORMAT(COMPARETODIFF, ""0%"")&""</text>
                        <text x='40' y='10' fill='black' font-size='6' font-family='tahoma' transform='translate(5 6)'>Month to Date</text>
                        <text x='40' y='17' fill='black' font-size='6' font-weight='bold' font-family='tahoma'
                            transform='translate(5 6)'>""&FORMAT(BASE_METRIC, ""0,0"")&""</text>
                        <text x='40' y='26' fill='""&BG_TEXT&""' font-size='6' font-weight='bold' font-family='tahoma'
                            transform='translate(5 6)'>""&FORMAT(METRICDIFF, ""0%"")&"" vs Y-1</text>
                    </svg>
                    """,
                    DisplayFolderIcon
                ).DataCategory = "ImageUrl";
            }
        }

        // HTML_Table
        if (baseMeasure != null && compareToMeasure != null && scope.ToLower() == "html_table"){

            var alternativeMeasureName = extraParametersList.Count() >= 3 && extraParametersList[2] != "" ? extraParametersList[2].Trim().ToLower() : baseMeasureName;

            var kpiCardPrefix = "HTML_Table_";
            var kpiCardName = kpiCardPrefix + alternativeMeasureName;
            var DisplayFolder = "HTML Measures\\Table\\" + alternativeMeasureName ;
            
            //HTML
            baseMeasure.Table.AddMeasure(
                kpiCardName,
                "\n" +
                "VAR METRICDIFF = DIVIDE(" + baseMeasure.DaxObjectName + ", " + compareToMeasure.DaxObjectName + ")\n" +
                @"
                VAR MIN_TRESHOLD = 0.85
                VAR MAX_TRESHOLD = 1
                VAR BG_COLOR =
                    SWITCH(
                        TRUE(), 
                        METRICDIFF >= MAX_TRESHOLD, ""#479E8C"",
                        METRICDIFF < MAX_TRESHOLD && MIN_TRESHOLD <= METRICDIFF, ""#ffcc00"",
                        METRICDIFF < MIN_TRESHOLD, ""#E76B61""
                    ) 
                VAR _SVG = 
                    ""data:image/svg+xml;utf8,"" & ""
                    <svg width='100' height='20' viewBox='-2 -2 102 22' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' overflow='visible'>
                    <rect x='0' y='0' width='100%' height='18' opacity='0.7' fill='""&BG_COLOR&""' stroke-width='0.2' ry='10' rx='10'/>
                    <text x='50%' y='50%' fill='white' font-family='tahoma' dominant-baseline='middle' text-anchor='middle'>""&FORMAT(METRICDIFF, ""Percent"")&""</text>
                    </svg>""
                RETURN IF(NOT(ISBLANK(METRICDIFF)), _SVG)",
                DisplayFolder
            ).DataCategory = "ImageUrl";
        }
        // Add Annotations
        Model.Tables["M_Measures"].Measures.Where(m => m.DisplayFolder.Contains("HTML Measures")).ToList().ForEach(m => m.SetAnnotation("GENERATEDBY", script.ToUpper()));
        
        // Hide Metrics
        var not_icons = Model.Tables["M_Measures"].Measures.Where(m => !m.DisplayFolder.Contains("Icon") && m.DisplayFolder.Contains("HTML Measures")).ToList();
        var not_table = Model.Tables["M_Measures"].Measures.Where(m => !m.DisplayFolder.Contains("Table") && m.DisplayFolder.Contains("HTML Measures")).ToList();
        var intersect = not_icons.AsQueryable().Intersect(not_table);
        intersect.ToList().ForEach(m => m.IsHidden = true);
    }
}