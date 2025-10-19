# RuleOne Examples and Use Cases

This document provides practical examples of using the RuleOne toolkit for financial analysis.

## Basic Workflow

### 1. Fetch Company Data

Fetch Microsoft's 10-K filings:
```powershell
dotnet run --project src/RuleOne.ETL 0000789019 10-K
```

Fetch quarterly (10-Q) filings:
```powershell
dotnet run --project src/RuleOne.ETL 0000789019 10-Q
```

### 2. Query the Database

View all facts for Microsoft:
```powershell
dotnet run --project src/RuleOne.ETL query 0000789019
```

Search for revenue data across all companies:
```powershell
dotnet run --project src/RuleOne.ETL concept Revenues
```

Search for earnings data:
```powershell
dotnet run --project src/RuleOne.ETL concept NetIncome
```

## Programmatic Usage

### Using the Analytics Library

Create a new F# script or project and reference the Analytics library:

```fsharp
open RuleOne.Analytics.FinancialMetrics

// Example 1: Calculate CAGR for Revenue Growth
let revenueGrowthAnalysis () =
    let year2019Revenue = 125843m // millions
    let year2024Revenue = 245122m // millions
    let years = 5m
    
    match calculateCAGR year2019Revenue year2024Revenue years with
    | Some cagr ->
        let percentage = cagr * 100m
        printfn "Revenue CAGR (2019-2024): %.2f%%" percentage
        
        // Rule #1 investing looks for 10%+ growth
        if percentage >= 10m then
            printfn "✓ Meets Rule #1 growth criteria"
        else
            printfn "✗ Does not meet Rule #1 growth criteria"
    | None ->
        printfn "Unable to calculate CAGR"

// Example 2: Calculate ROIC
let roicAnalysis () =
    let nopat = 72361m // millions
    let investedCapital = 300000m // millions
    
    match calculateROIC nopat investedCapital with
    | Some roic ->
        let percentage = roic * 100m
        printfn "ROIC: %.2f%%" percentage
        
        // Rule #1 investing looks for 10%+ ROIC
        if percentage >= 10m then
            printfn "✓ Strong capital efficiency"
        else
            printfn "✗ Weak capital efficiency"
    | None ->
        printfn "Unable to calculate ROIC"

// Example 3: Calculate EPS
let epsAnalysis () =
    let netIncome = 72361m // millions
    let outstandingShares = 7430m // millions
    
    match calculateEPS netIncome outstandingShares with
    | Some eps ->
        printfn "EPS: $%.2f" eps
    | None ->
        printfn "Unable to calculate EPS"

// Example 4: Calculate Margin of Safety
let mosAnalysis () =
    let intrinsicValue = 450m // per share
    let currentPrice = 350m // per share
    
    match calculateMOS intrinsicValue currentPrice with
    | Some mos ->
        let percentage = mos * 100m
        printfn "Margin of Safety: %.2f%%" percentage
        
        // Rule #1 investing typically requires 50% MOS
        if percentage >= 50m then
            printfn "✓ Strong margin of safety"
        elif percentage >= 25m then
            printfn "⚠ Acceptable margin of safety"
        else
            printfn "✗ Insufficient margin of safety"
    | None ->
        printfn "Unable to calculate MOS"

// Run all analyses
revenueGrowthAnalysis()
roicAnalysis()
epsAnalysis()
mosAnalysis()
```

### Using the Database Module

```fsharp
open RuleOne.ETL.Database

let dbPath = "ruleone.db"
let connectionString = $"Data Source={dbPath}"

// Initialize database
initializeDatabase connectionString

// Insert a fact
insertFact connectionString 
    "0000789019" 
    (Some "Microsoft Corp") 
    (Some "2024-06-30") 
    (Some "10-K")
    "Revenues"
    (Some "245122000000")
    (Some "USD")
    None
    (Some "FY2024")

// Query facts by CIK
let msftFacts = queryFactsByCIK connectionString "0000789019"
for fact in msftFacts do
    printfn "%s: %s %s" 
        fact.Concept 
        (Option.defaultValue "N/A" fact.Value)
        (Option.defaultValue "" fact.Unit)

// Query facts by concept
let revenueFacts = queryFactsByConcept connectionString "Revenues"
for fact in revenueFacts do
    printfn "%s - %s: %s" 
        fact.CIK
        (Option.defaultValue "Unknown" fact.CompanyName)
        (Option.defaultValue "N/A" fact.Value)
```

## Rule #1 Investing Analysis

### Complete Company Analysis

Here's a complete workflow for analyzing a company using Rule #1 principles:

```powershell
# 1. Fetch the data
Write-Host "Fetching data for Apple (CIK: 0000320193)..."
dotnet run --project src/RuleOne.ETL 0000320193 10-K

# 2. Query key metrics
Write-Host "Querying revenues..."
dotnet run --project src/RuleOne.ETL concept Revenues | Select-String "0000320193"

Write-Host "Querying earnings..."
dotnet run --project src/RuleOne.ETL concept NetIncome | Select-String "0000320193"

Write-Host "Querying equity..."
dotnet run --project src/RuleOne.ETL concept StockholdersEquity | Select-String "0000320193"

# 3. Analyze in notebook
Write-Host "Open samples/FinancialAnalysis.ipynb to visualize trends"
```

### The Big Four Numbers

Rule #1 investing focuses on four key growth metrics:

1. **Revenue (Sales) Growth** - Use CAGR over 10 years
2. **EPS Growth** - Earnings per share CAGR
3. **Operating Cash Flow Growth** - Free cash flow CAGR
4. **ROIC** - Return on Invested Capital

Example analysis for each:

```fsharp
open RuleOne.Analytics.FinancialMetrics

// Analyzing 10-year growth
let analyzeGrowth (metricName: string) (startValue: decimal) (endValue: decimal) =
    match calculateCAGR startValue endValue 10m with
    | Some cagr ->
        let percentage = cagr * 100m
        printfn "%s 10-Year CAGR: %.2f%%" metricName percentage
        
        // Rule #1 minimum: 10% growth
        if percentage >= 10m then
            printfn "✓ Passes Rule #1 test"
        else
            printfn "✗ Below Rule #1 minimum"
        
        percentage >= 10m
    | None ->
        printfn "Cannot calculate %s CAGR" metricName
        false

// Example usage
let passesRuleOne =
    let revenuePass = analyzeGrowth "Revenue" 65225m 394328m
    let epsPass = analyzeGrowth "EPS" 2.97m 6.13m
    let fcfPass = analyzeGrowth "Free Cash Flow" 37037m 110543m
    
    // ROIC check (use average over period)
    let roicPass = 
        match calculateROIC 110543m 500000m with
        | Some roic -> roic >= 0.10m
        | None -> false
    
    revenuePass && epsPass && fcfPass && roicPass

if passesRuleOne then
    printfn "✓ Company meets all Rule #1 criteria"
else
    printfn "✗ Company does not meet all Rule #1 criteria"
```

## Advanced Queries

### SQLite Direct Queries

For advanced users, you can query the database directly:

```powershell
# Get revenue trends
sqlite3 ruleone.db "SELECT FilingDate, Value FROM Facts WHERE CIK='0000789019' AND Concept LIKE '%Revenue%' ORDER BY FilingDate;"

# Compare multiple companies
sqlite3 ruleone.db "SELECT CompanyName, AVG(CAST(Value AS REAL)) as AvgRevenue FROM Facts WHERE Concept LIKE '%Revenue%' GROUP BY CompanyName;"

# Get filing count by company
sqlite3 ruleone.db "SELECT CompanyName, COUNT(*) as FilingCount FROM Facts GROUP BY CompanyName ORDER BY FilingCount DESC;"
```

### Data Export

Export data to CSV for analysis in Excel or other tools:

```powershell
sqlite3 -header -csv ruleone.db "SELECT * FROM Facts WHERE CIK='0000789019';" | Out-File -FilePath microsoft_data.csv -Encoding UTF8
```

## Tips and Best Practices

1. **Start with 10-K filings** - They contain complete annual data
2. **Fetch multiple years** - You need at least 5-10 years for meaningful CAGR
3. **Verify concepts** - XBRL concepts may vary by company; use `concept` query to explore
4. **Use the notebook** - Visual trends are easier to spot than raw numbers
5. **Compare to industry** - Rule #1 criteria should be compared to industry averages

## Common CIK Numbers

Quick reference for popular companies:

```powershell
# Tech Giants
# Microsoft: 0000789019
# Apple: 0000320193
# Amazon: 0001018724
# Alphabet: 0001652044
# Meta: 0001326801

# Traditional Companies
# Walmart: 0000104169
# JPMorgan: 0000019617
# Johnson & Johnson: 0000200406
# Visa: 0001403161
# Procter & Gamble: 0000080424

# Growth Companies
# Tesla: 0001318605
# Netflix: 0001065280
# NVIDIA: 0001045810
# Adobe: 0000796343
# Salesforce: 0001108524
```

## Troubleshooting

**Error**: Rate limiting from SEC
- **Solution**: Add delays between requests (SEC guidelines)

**Error**: Missing XBRL concepts
- **Solution**: Different companies use different concept names; query available concepts first

**Error**: Database locked
- **Solution**: Close any other processes accessing the database

## Next Steps

1. Build a watchlist of companies you want to analyze
2. Fetch historical data for each
3. Run Rule #1 analysis in the notebook
4. Create custom charts comparing multiple companies
5. Set up automated data refresh workflows
