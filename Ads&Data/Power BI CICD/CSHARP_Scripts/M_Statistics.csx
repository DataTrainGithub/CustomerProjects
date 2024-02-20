//var measureMetadata = ReadFile(@"C:\Users\MattiasDeConinck\Documents\Repositories\de-analytics\Mapping\MeasureMapping.csv");
var measureMetadata = ReadFile(@"D:\a\de-analytics\de-analytics\Mapping\MeasureMapping.csv");

var displayFolderAnalytics = "Analytics Measures";
var csvRows = measureMetadata.Split(new[] {
    '\r',
    '\n'
}, StringSplitOptions.RemoveEmptyEntries);

// Delete previously generated measures
Model.Tables["M_Measures"].Measures.Where(m => m.GetAnnotation("GENERATEDBY") == "M_Statistics".ToUpper()).ToList().ForEach(m => m.Delete());

foreach (var row in csvRows.Skip(1))
{
    var csvColumns = row.Split(';');
    var modelName = csvColumns[0].Trim();
    var tableName = csvColumns[1].Trim();
    var measureName = csvColumns[2].Trim();
    var script = csvColumns[3].Trim();
    var extraParameters = csvColumns[4].Trim();

    // Check if Model names are equal
    if (Model.GetAnnotation("MappingModel").ToLower() != modelName.ToLower())
    {
        continue;
    }

    if (script.ToLower() == "M_Statistics".ToLower())
    {

        var table = Model.Tables["M_Measures"];

        String[] d = new String[] { "^^" };
        var extraParametersList = extraParameters.Split(d, StringSplitOptions.None);
        var months = extraParametersList[0].Trim().Split(',');
        var weeks = extraParametersList.Count() >= 2 ? extraParametersList[1].Trim().Split(',') : null;
        var type = extraParametersList.Count() >= 3 ? extraParametersList[2].Trim() : "Daily";

        var measure = table.Measures.FirstOrDefault(m => m.Name.ToUpper() == measureName.ToUpper());
        var measureList = measure.Name.Split('_');
        var newMeasureName = measure.Name.Contains("Base") || measure.Name.Contains("Custom") ? String.Join("_", measureList.Take(measureList.Length - 1)) : measure.Name;
        var generatedOn = measure.GetAnnotation("GENERATEDON");

        var displayFolderAnalyticsMovingAverages = displayFolderAnalytics + "\\" + generatedOn  + "\\Moving Averages";
        var displayFolderAnalyticsStandardDeviations = displayFolderAnalytics + "\\" + generatedOn  + "\\Moving Standard Deviations";

        if (type.ToLower() == "daily"){
            foreach (var month in months)
            {
                var movingAverageName = newMeasureName + "_MovingAverage_" + month.PadLeft(2, '0') + "_Months";
                var movingAverageMeasure = table.AddMeasure(
                    movingAverageName,
                    "\nVAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + month + ", MONTH), -1, DAY)\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates\n" + 
                    "VAR _Result = CALCULATE(AVERAGEX(_Table, [M]), _Period)\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsMovingAverages
                );
                movingAverageMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

                var standardDeviationName = newMeasureName + "_StandardDeviation_" + month.PadLeft(2, '0') + "_Months";
                var standardDeviationMeasure = table.AddMeasure(
                    standardDeviationName,
                  "\nVAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + month + ", MONTH), -1, DAY)\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates\n" + 
                    "VAR _Result = STDEVX.P(_Table, [M])\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsStandardDeviations
                );
                standardDeviationMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

            }

            foreach (var week in weeks)
            {
                var days = int.Parse(week) * 7;
                var movingAverageName = newMeasureName + "_MovingAverage_" + week.PadLeft(2, '0') + "_Weeks";
                var movingAverageMeasure = table.AddMeasure(
                    movingAverageName,
                    "\nVAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + days + ", DAY), -1, DAY)\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates\n" + 
                    "VAR _Result = CALCULATE(AVERAGEX(_Table, [M]), _Period)\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsMovingAverages
                );
                movingAverageMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

                var standardDeviationName = newMeasureName + "_StandardDeviation_" + week.PadLeft(2, '0') + "_Weeks";
                var standardDeviationMeasure = table.AddMeasure(
                    standardDeviationName,
                    "\nVAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + days + ", DAY), -1, DAY)\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates\n" + 
                    "VAR _Result = STDEVX.P(_Table, [M])\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsStandardDeviations
                );
                standardDeviationMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

            }

            foreach (var week in weeks)
            {

                if (week == "1"){
                    continue;
                }

                var days = int.Parse(week) * 7;
                var movingAverageName = newMeasureName + "_MovingAverage_" + week.PadLeft(2, '0') + "_Weeks_SameWeekday";
                var movingAverageMeasure = table.AddMeasure(
                    movingAverageName,
                    "\nVAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = FILTER(DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + days + ", DAY), -1, DAY), WEEKDAY(D_Date[Date_Date]) = WEEKDAY(_CurrentDate))\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates\n" + 
                    "VAR _Result = CALCULATE(AVERAGEX(_Table, [M]), _Period)\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsMovingAverages
                );
                movingAverageMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

                var standardDeviationName = newMeasureName + "_StandardDeviation_" + week.PadLeft(2, '0') + "_Weeks_SameWeekday";
                var standardDeviationMeasure = table.AddMeasure(
                    standardDeviationName,
                    "\nVAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = FILTER(DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + days + ", DAY), -1, DAY), WEEKDAY(D_Date[Date_Date]) = WEEKDAY(_CurrentDate))\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates\n" + 
                    "VAR _Result = STDEVX.P(_Table, [M])\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsStandardDeviations
                );
                standardDeviationMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

            }
        }
        else if (type.ToLower() == "hourly"){
            foreach (var month in months)
            {
                var movingAverageName = newMeasureName + "_MovingAverage_" + month.PadLeft(2, '0') + "_Months";
                var movingAverageMeasure = table.AddMeasure(
                    movingAverageName,
                    "\nVAR _MaxHour = CALCULATE(MAX(" + tableName + "[Hour]), REMOVEFILTERS(D_Time[Hour]))\n" +
                    "VAR _CurrentHour = MAX(D_Time[Hour])\n" +
                    "VAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + month + ", MONTH), -1, DAY)\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates && IF(_LastFactDate = _CurrentDate, _CurrentHour <= _MaxHour, True)\n" + 
                    "VAR _Result = CALCULATE(AVERAGEX(_Table, [M]), _Period)\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsMovingAverages
                );
                movingAverageMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

                var standardDeviationName = newMeasureName + "_StandardDeviation_" + month.PadLeft(2, '0') + "_Months";
                var standardDeviationMeasure = table.AddMeasure(
                    standardDeviationName,
                    "\nVAR _MaxHour = CALCULATE(MAX(" + tableName + "[Hour]), REMOVEFILTERS(D_Time[Hour]))\n" +
                    "VAR _CurrentHour = MAX(D_Time[Hour])\n" +
                    "VAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + month + ", MONTH), -1, DAY)\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates && IF(_LastFactDate = _CurrentDate, _CurrentHour <= _MaxHour, True)\n" + 
                    "VAR _Result = STDEVX.P(_Table, [M])\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsStandardDeviations
                );
                standardDeviationMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

            }

            foreach (var week in weeks)
            {
                var days = int.Parse(week) * 7;
                var movingAverageName = newMeasureName + "_MovingAverage_" + week.PadLeft(2, '0') + "_Weeks";
                var movingAverageMeasure = table.AddMeasure(
                    movingAverageName,
                    "\nVAR _MaxHour = CALCULATE(MAX(" + tableName + "[Hour]), REMOVEFILTERS(D_Time[Hour]))\n" +
                    "VAR _CurrentHour = MAX(D_Time[Hour])\n" +
                    "VAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + days + ", DAY), -1, DAY)\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates && IF(_LastFactDate = _CurrentDate, _CurrentHour <= _MaxHour, True)\n" + 
                    "VAR _Result = CALCULATE(AVERAGEX(_Table, [M]), _Period)\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsMovingAverages
                );
                movingAverageMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

                var standardDeviationName = newMeasureName + "_StandardDeviation_" + week.PadLeft(2, '0') + "_Weeks";
                var standardDeviationMeasure = table.AddMeasure(
                    standardDeviationName,
                    "\nVAR _MaxHour = CALCULATE(MAX(" + tableName + "[Hour]), REMOVEFILTERS(D_Time[Hour]))\n" +
                    "VAR _CurrentHour = MAX(D_Time[Hour])\n" +
                    "VAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + days + ", DAY), -1, DAY)\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates && IF(_LastFactDate = _CurrentDate, _CurrentHour <= _MaxHour, True)\n" + 
                    "VAR _Result = STDEVX.P(_Table, [M])\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsStandardDeviations
                );
                standardDeviationMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

            }

            foreach (var week in weeks)
            {

                if (week == "1"){
                    continue;
                }

                var days = int.Parse(week) * 7;
                var movingAverageName = newMeasureName + "_MovingAverage_" + week.PadLeft(2, '0') + "_Weeks_SameWeekday";
                var movingAverageMeasure = table.AddMeasure(
                    movingAverageName,
                    "\nVAR _MaxHour = CALCULATE(MAX(" + tableName + "[Hour]), REMOVEFILTERS(D_Time[Hour]))\n" +
                    "VAR _CurrentHour = MAX(D_Time[Hour])\n" +
                    "VAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = FILTER(DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + days + ", DAY), -1, DAY), WEEKDAY(D_Date[Date_Date]) = WEEKDAY(_CurrentDate))\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates && IF(_LastFactDate = _CurrentDate, _CurrentHour <= _MaxHour, True)\n" + 
                    "VAR _Result = CALCULATE(AVERAGEX(_Table, [M]), _Period)\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsMovingAverages
                );
                movingAverageMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

                var standardDeviationName = newMeasureName + "_StandardDeviation_" + week.PadLeft(2, '0') + "_Weeks_SameWeekday";
                var standardDeviationMeasure = table.AddMeasure(
                    standardDeviationName,
                    "\nVAR _MaxHour = CALCULATE(MAX(" + tableName + "[Hour]), REMOVEFILTERS(D_Time[Hour]))\n" +
                    "VAR _CurrentHour = MAX(D_Time[Hour])\n" +
                    "VAR _LastFactDate = CALCULATE(MAX(" + tableName + "[Date]), REMOVEFILTERS())\n" +
                    "VAR _CurrentDate = MAX(D_Date[Date_Date])\n" +
                    "VAR _DynamicDates = SUMMARIZE(CALCULATETABLE(D_Date, USERELATIONSHIP(D_Date[Date_Date], D_Date_DynamicPeriods[Date])), D_Date[Date_Date])\n" +
                    "VAR _Period = FILTER(DATEADD(DATESINPERIOD(D_Date[Date_Date], _CurrentDate, -" + days + ", DAY), -1, DAY), WEEKDAY(D_Date[Date_Date]) = WEEKDAY(_CurrentDate))\n" +
                    "VAR _Table = SUMMARIZE(_Period, D_Date[Date_Date], \"M\", " + measure.DaxObjectName + " + 0)\n" +
                    "VAR _ShowResult = _CurrentDate <= _LastFactDate && _CurrentDate in _DynamicDates && IF(_LastFactDate = _CurrentDate, _CurrentHour <= _MaxHour, True)\n" + 
                    "VAR _Result = STDEVX.P(_Table, [M])\n" +
                    "RETURN IF(_ShowResult, _Result)\n",
                    displayFolderAnalyticsStandardDeviations
                );
                standardDeviationMeasure.SetAnnotation("GENERATEDBY", script.ToUpper());

            }
        }
    }
}