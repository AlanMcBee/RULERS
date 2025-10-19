# Sample Notebook Guide

## Prerequisites

To run the F# notebook (`FinancialAnalysis.ipynb`), you need one of the following:

### Option 1: Visual Studio Code (Recommended)
1. Install [VS Code](https://code.visualstudio.com/)
2. Install the [.NET Interactive Notebooks extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)
3. Open the notebook file in VS Code

### Option 2: Jupyter with .NET Interactive
1. Install [Jupyter](https://jupyter.org/install)
2. Install [.NET Interactive](https://github.com/dotnet/interactive):
   ```powershell
   dotnet tool install -g Microsoft.dotnet-interactive
   dotnet interactive jupyter install
   ```
3. Launch Jupyter:
   ```powershell
   jupyter notebook
   ```
4. Navigate to `samples/FinancialAnalysis.ipynb`

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

1. Open `samples/FinancialAnalysis.ipynb` in your notebook environment
2. Execute cells sequentially (Shift+Enter)
3. The notebook will:
   - Connect to the SQLite database
   - Query financial facts
   - Create visualizations with Plotly.NET
   - Calculate growth metrics

## Notebook Contents

The notebook demonstrates:
- Database connectivity
- Querying revenue and earnings data
- Creating line charts for trends
- Calculating CAGR (Compound Annual Growth Rate)
- Interactive data exploration

## Troubleshooting

**Issue**: Database file not found
- **Solution**: Run the ETL app first to create and populate the database

**Issue**: No data to display
- **Solution**: Ensure you've fetched data for at least one company using the ETL app

**Issue**: Plotly charts not displaying
- **Solution**: Make sure you're running in a notebook environment that supports JavaScript output

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
