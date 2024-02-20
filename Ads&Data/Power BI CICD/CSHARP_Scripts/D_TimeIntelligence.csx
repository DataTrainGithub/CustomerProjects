var addItem = new Func<CalculationGroupTable,int, string, string, CalculationItem>((table, ordinal, item, expression) => 
{
    CalculationItem ci = null;
    if (table.CalculationItems.FirstOrDefault(i => i.Name == item) == null){
    
        ci = table.AddCalculationItem(item, expression);
        ci.Ordinal = ordinal;
    }
    else {
    
        ci = table.CalculationItems[item];
        ci.Expression = expression;
        ci.Ordinal = ordinal;
    }
    
    return ci;
});


CalculationGroupTable cg = null;

if (Model.Tables.FirstOrDefault(t => t.Name == "D_TimeIntelligence") == null){
    
    cg = Model.AddCalculationGroup("D_TimeIntelligence");
    cg.Columns["Name"].Name = "Type";
    cg.IsHidden = true;
}

cg = (Model.Tables["D_TimeIntelligence"] as CalculationGroupTable);

addItem(cg, 0, "Y", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (D_Date, USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ))
)");

addItem(cg, 1, "PY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (DATEADD ('D_Date'[Date_Date],-1,YEAR),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ))
)");

addItem(cg, 2, "PPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (DATEADD ('D_Date'[Date_Date],-2,YEAR),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ))
)");

addItem(cg, 3, "YTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESYTD ( 'D_Date'[Date_Date] ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
    )
)");

addItem(cg, 4, "YTDPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE(
    DATESYTD (
                DATEADD (
                'D_Date'[Date_Date],
                -1,
                YEAR
            )),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
        )
    )
");

addItem(cg, 5, "YC", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        PARALLELPERIOD ( 'D_Date'[Date_Date], 0, YEAR ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
    )
)
");

addItem(cg, 6, "YCPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        PARALLELPERIOD ( 'D_Date'[Date_Date], -1, YEAR ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
    )
)
");

addItem(cg, 7, "QTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESQTD ( 'D_Date'[Date_Date] ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
    )
)
");

addItem(cg, 8, "QTDPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE(
    DATESQTD (
                DATEADD (
                'D_Date'[Date_Date],
                -1,
                YEAR
            )),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
        )
    )
");

addItem(cg, 9, "PQ", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (DATEADD ('D_Date'[Date_Date],-1,Quarter),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ))
)
");  

addItem(cg, 10, "PQTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE(
    DATESQTD (
                DATEADD (
                'D_Date'[Date_Date],
                -1,
                Quarter
            )),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
        )
    )
"); 

addItem(cg, 11, "MTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( 'D_Date'[Date_Date] ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
    )
)
"); 

addItem(cg, 12, "MTDPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE(
    DATESMTD (
                DATEADD (
                'D_Date'[Date_Date],
                -1,
                YEAR
            )),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
        )
    )
");  

addItem(cg, 13, "PM", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (DATEADD ('D_Date'[Date_Date],-1,Month),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ))
)
"); 

addItem(cg, 14, "PMTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE(
    DATESMTD (
                DATEADD (
                'D_Date'[Date_Date],
                -1,
                Month
            )),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] )
        )
    )
");
 
addItem(cg, 15, "TY_Y", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        D_Date,
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () )
    )
)
"); 

addItem(cg, 16, "TY_PY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATEADD ( 'D_Date'[Date_Date], -1, YEAR ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () )
    )
)
"); 

addItem(cg, 17, "TY_PPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATEADD ( 'D_Date'[Date_Date], -2, YEAR ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () )
    )
)
");

addItem(cg, 18, "TY_YTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESYTD ( 'D_Date'[Date_Date] ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () )
    )
)
");

addItem(cg, 19, "TY_YTDPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE(
    DATESYTD (
                DATEADD (
                'D_Date'[Date_Date],
                -1,
                YEAR
            )),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
            D_Date[Date_Year] = YEAR ( NOW () )
        )
    )
");

addItem(cg, 20, "TY_QTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESQTD ( 'D_Date'[Date_Date] ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () )
        
    )
)
");

addItem(cg, 21, "TY_QTDPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESQTD ( DATEADD ( 'D_Date'[Date_Date], -1, YEAR ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) 
    )
)
");

addItem(cg, 22, "TY_PQ", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATEADD ( 'D_Date'[Date_Date], -1, QUARTER ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () )
    )
)
");

addItem(cg, 23, "TY_PQTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESQTD ( DATEADD ( 'D_Date'[Date_Date], -1, QUARTER ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () )
    )
)
");

addItem(cg, 24, "TY_MTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( 'D_Date'[Date_Date] ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) 
    )
)
");

addItem(cg, 25, "TY_MTDPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( DATEADD ( 'D_Date'[Date_Date], -1, YEAR ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) 
    )
)
");

addItem(cg, 26, "TY_PM", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATEADD ( 'D_Date'[Date_Date], -1, MONTH ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () )
    )
)
");

addItem(cg, 27, "TY_PMTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( DATEADD ( 'D_Date'[Date_Date], -1, MONTH ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () )
    )
)
");

addItem(cg, 28, "TQ_Y", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        D_Date,
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 29, "TQ_PY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATEADD ( 'D_Date'[Date_Date], -1, YEAR ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 30, "TQ_PPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATEADD ( 'D_Date'[Date_Date], -2, YEAR ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 31, "TQ_QTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESQTD ( 'D_Date'[Date_Date] ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 32, "TQ_QTDPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESQTD ( DATEADD ( 'D_Date'[Date_Date], -1, YEAR ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 33, "TQ_PQ", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATEADD ( 'D_Date'[Date_Date], -1, QUARTER ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 34, "TQ_PQTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESQTD ( DATEADD ( 'D_Date'[Date_Date], -1, QUARTER ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 35, "TQ_MTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( 'D_Date'[Date_Date] ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 36, "TQ_MTDPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( DATEADD ( 'D_Date'[Date_Date], -1, YEAR ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 37, "TQ_PM", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATEADD ( 'D_Date'[Date_Date], -1, MONTH ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 38, "TQ_PMTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( DATEADD ( 'D_Date'[Date_Date], -1, MONTH ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Quarter]=Quarter(now())
    )
)
");

addItem(cg, 39, "TM_Y", @"
CALCULATE (
    SELECTEDMEASURE (),
    USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
    D_Date[Date_Year]=year(now())&&D_Date[Date_Month]=Month(now())
    )
");

addItem(cg, 40, "TM_MTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( 'D_Date'[Date_Date] ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Month]=Month(now())
    )
)
");

addItem(cg, 41, "TM_MTDPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( DATEADD ( 'D_Date'[Date_Date], -1, YEAR ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Month]=Month(now())
    )
)
");

addItem(cg, 42, "TM_PM", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATEADD ( 'D_Date'[Date_Date], -1, MONTH ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Month]=Month(now())
    )
)
");

addItem(cg, 43, "TM_PMTD", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (
        DATESMTD ( DATEADD ( 'D_Date'[Date_Date], -1, MONTH ) ),
        USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ),
        D_Date[Date_Year] = YEAR ( NOW () ) && D_Date[Date_Month]=Month(now())
    )
)
");

addItem(cg, 44, "PPPY", @"
CALCULATE (
    SELECTEDMEASURE (),
    CALCULATETABLE (DATEADD ('D_Date'[Date_Date],-3,YEAR),USERELATIONSHIP ( D_Date[Date_Date], D_Date_DynamicPeriods[Date] ))
)");