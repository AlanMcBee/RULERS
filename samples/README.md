# Sample Notebook Guide

## Prerequisites

To run the hybrid F#/PowerShell notebook (`FinancialAnalysis.verso`), you need:

1. Install [VS Code](https://code.visualstudio.com/)
2. Install the [Verso Notebook](https://marketplace.visualstudio.com/items?itemName=Datafication.verso-notebook) extension
3. Open `samples/FinancialAnalysis.verso` in VS Code

See [ADR-0004](../docs/adr/ADR-0004-notebook-polyglot-language-strategy.md) for why the notebook uses both F# and PowerShell.

## Choosing a Company

The notebook analyzes one company at a time, selected by the `ticker` parameter
(defaults to `MSFT`). Set it in the Parameters cell near the top of the notebook,
or pass it on the command line for a headless run:

```powershell
verso run samples/FinancialAnalysis.verso --param ticker=AAPL
```

The first cell resolves the ticker to a CIK and automatically fetches its 10-K
filings if the database doesn't already have data for that company - there's no
separate manual fetch step required before running the notebook.

## Running the Notebook

1. Open `samples/FinancialAnalysis.verso` in your notebook environment
2. Set the `ticker` parameter to the company you want to analyze (or keep the `MSFT` default)
3. Execute cells sequentially
4. The notebook will:
   - Resolve the ticker to a CIK and fetch 10-K data if it isn't already stored (PowerShell)
   - Connect to the SQLite database and query financial facts for that company (F#)
   - Create visualizations with Plotly, rendered as standalone HTML (PowerShell)
   - Calculate growth metrics (F#)
   - List securities via the `RuleOne` PowerShell module (PowerShell)

## Notebook Contents

The notebook demonstrates:
- Resolving a `ticker` parameter to a CIK and auto-fetching missing data (PowerShell)
- Database connectivity and data shaping via `RuleOne.ETL`/`RuleOne.Analytics`, scoped to one company (F#)
- Querying revenue and earnings data (F#)
- Creating line charts for trends, titled with the current ticker (PowerShell)
- Calculating CAGR (Compound Annual Growth Rate) (F#)
- Orchestrating the `RuleOne` PowerShell module (PowerShell)

## Troubleshooting

**Issue**: `Could not resolve ticker '...' to a CIK via SEC data`
- **Solution**: Confirm the ticker is spelled correctly and is a valid, currently-listed SEC ticker; check network access to SEC's ticker lookup endpoint.

**Issue**: Database file not found
- **Solution**: Run the notebook's ticker-resolution cell first; it creates and populates the database automatically for the current `ticker`

**Issue**: No data to display
- **Solution**: Confirm the ticker-resolution cell reported a successful fetch (or existing facts) for the current `ticker` before running later cells

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
