# Sample Notebook Guide

## Prerequisites

To run the hybrid F#/PowerShell notebook (`FinancialAnalysis.verso`), you need:

1. Install [VS Code](https://code.visualstudio.com/)
2. Install the [Verso Notebook](https://marketplace.visualstudio.com/items?itemName=Datafication.verso-notebook) extension
3. Open `samples/FinancialAnalysis.verso` in VS Code

See [ADR-0004](../docs/adr/ADR-0004-notebook-polyglot-language-strategy.md) for why the notebook uses both F# and PowerShell.

## Before Running the Notebook

1. First, fetch some data using the ETL application:
   ```powershell
   # Example: Fetch Microsoft 10-K filings
   dotnet run --project src/RuleOne.ETL 0000789019 10-K
   ```

2. Verify data was stored:
   ```powershell
   dotnet run --project src/RuleOne.ETL query 0000789019
   ```

## Running the Notebook

1. Open `samples/FinancialAnalysis.verso` in your notebook environment
2. Execute cells sequentially
3. The notebook will:
   - Connect to the SQLite database and query financial facts (F#)
   - Create visualizations with Plotly, rendered as standalone HTML (PowerShell)
   - Calculate growth metrics (F#)
   - List securities via the `RuleOne` PowerShell module (PowerShell)

## Notebook Contents

The notebook demonstrates:
- Database connectivity and data shaping via `RuleOne.ETL`/`RuleOne.Analytics` (F#)
- Querying revenue and earnings data (F#)
- Creating line charts for trends (PowerShell)
- Calculating CAGR (Compound Annual Growth Rate) (F#)
- Orchestrating the `RuleOne` PowerShell module (PowerShell)

## Troubleshooting

**Issue**: Database file not found
- **Solution**: Run the ETL app first to create and populate the database

**Issue**: No data to display
- **Solution**: Ensure you've fetched data for at least one company using the ETL app

**Issue**: Plotly charts not displaying
- **Solution**: The chart cells write a standalone HTML file to `samples/` and open it with `Invoke-Item`; confirm your default browser opened and that you have network access to `cdn.plot.ly`.

**Issue**: `Get-R1Securities` (or other `R1`-prefixed commands) not found
- **Solution**: Make sure the module import cell ran successfully; see [POWERSHELL_MODULE.md](../docs/usage/POWERSHELL_MODULE.md).

## Example Companies to Analyze

Here are some CIK numbers for major companies:

| Company | CIK |
|---------|-----|
| Microsoft | 0000789019 |
| Apple | 0000320193 |
| Amazon | 0001018724 |
| Alphabet (Google) | 0001652044 |
| Tesla | 0001318605 |
| Meta (Facebook) | 0001326801 |
| Netflix | 0001065280 |
| NVIDIA | 0001045810 |

## Next Steps

After running the basic notebook:
1. Modify queries to explore different financial concepts
2. Add additional chart types (bar charts, scatter plots)
3. Calculate custom metrics using the Analytics library
4. Compare multiple companies side-by-side
