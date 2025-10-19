# RULERS - RuleOne Investment Analysis

An F# .NET 8 solution for fetching, storing, and analyzing SEC EDGAR financial data using Rule #1 investing principles.

## Overview

RuleOne is a lightweight, modular financial analysis toolkit that:
- Fetches SEC EDGAR 10-K/10-Q Inline XBRL filings by CIK
- Stores financial facts in a SQLite database
- Provides analytics functions for key metrics (CAGR, ROIC, EPS, MOS)
- Includes visualization capabilities via F# notebooks with Plotly.NET

## Features

### ETL Console Application (`RuleOne.ETL`)
- Command-line interface for fetching SEC EDGAR data
- Parses company submissions and XBRL facts
- SQLite database storage with indexed queries
- Support for both 10-K and 10-Q filings

### Analytics Library (`RuleOne.Analytics`)
- **CAGR** (Compound Annual Growth Rate) - Calculate growth rate over time
- **ROIC** (Return on Invested Capital) - Measure capital efficiency
- **EPS** (Earnings Per Share) - Calculate per-share earnings
- **MOS** (Margin of Safety) - Determine investment safety margin

### Interactive Notebook
- F# Jupyter notebook with Plotly.NET integration
- Query and visualize revenue, earnings, and growth trends
- Calculate financial metrics interactively

## Project Structure

```
RULERS/
├── src/
│   ├── RuleOne.ETL/          # ETL console application
│   │   ├── Database.fs        # SQLite operations
│   │   ├── SecEdgar.fs        # SEC EDGAR data fetching
│   │   └── Program.fs         # CLI interface
│   └── RuleOne.Analytics/     # Analytics class library
│       └── Library.fs         # Financial metrics calculations
├── tests/
│   └── RuleOne.Tests/         # xUnit test project
│       └── Tests.fs           # Unit tests
├── samples/
│   └── FinancialAnalysis.ipynb # Sample F# notebook
├── .github/
│   └── workflows/
│       └── dotnet.yml         # CI/CD workflow
├── RuleOne.sln                # Solution file
├── LICENSE                    # MIT License
└── README.md                  # This file
```

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- (Optional) [Jupyter](https://jupyter.org/) or VS Code with .NET Interactive for notebooks

### Building the Solution

```bash
# Clone the repository
git clone https://github.com/AlanMcBee/RULERS.git
cd RULERS

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Usage

### Fetching SEC Data

Fetch and store SEC filings for a company by CIK:

```bash
# Fetch Microsoft (CIK: 0000789019) 10-K filings
dotnet run --project src/RuleOne.ETL 0000789019 10-K

# Fetch 10-Q filings
dotnet run --project src/RuleOne.ETL 0000789019 10-Q
```

### Querying Data

Query stored facts from the database:

```bash
# Query all facts for a specific CIK
dotnet run --project src/RuleOne.ETL query 0000789019

# Query facts by concept (e.g., Revenues, NetIncome)
dotnet run --project src/RuleOne.ETL concept Revenues
```

### Using the Analytics Library

```fsharp
open RuleOne.Analytics.FinancialMetrics

// Calculate CAGR
let cagr = calculateCAGR 100m 200m 5m
match cagr with
| Some rate -> printfn "CAGR: %.2f%%" (rate * 100m)
| None -> printfn "Invalid inputs"

// Calculate ROIC
let roic = calculateROIC 1000000m 5000000m
match roic with
| Some rate -> printfn "ROIC: %.2f%%" (rate * 100m)
| None -> printfn "Invalid inputs"

// Calculate EPS
let eps = calculateEPS 10000000m 1000000m
match eps with
| Some value -> printfn "EPS: $%.2f" value
| None -> printfn "Invalid inputs"

// Calculate Margin of Safety
let mos = calculateMOS 100m 75m
match mos with
| Some margin -> printfn "MOS: %.2f%%" (margin * 100m)
| None -> printfn "Invalid inputs"
```

### Using the Notebook

1. Fetch some data first using the ETL app
2. Open `samples/FinancialAnalysis.ipynb` in Jupyter or VS Code
3. Execute cells sequentially to analyze and visualize data

## Database Schema

The SQLite database (`ruleone.db`) contains a single `Facts` table:

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| CIK | TEXT | Company CIK number |
| CompanyName | TEXT | Company name |
| FilingDate | TEXT | Filing date |
| FormType | TEXT | Form type (10-K, 10-Q) |
| Concept | TEXT | XBRL concept name |
| Value | TEXT | Fact value |
| Unit | TEXT | Unit of measurement |
| ContextRef | TEXT | XBRL context reference |
| Period | TEXT | Reporting period |
| CreatedAt | TEXT | Timestamp |

Indexes are created on CIK, Concept, and FilingDate for efficient queries.

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed
```

### CI/CD

GitHub Actions automatically builds and tests the solution on push and pull requests to the main branch.

## Examples

### Common Company CIKs
- Microsoft: 0000789019
- Apple: 0000320193
- Amazon: 0001018724
- Google (Alphabet): 0001652044
- Tesla: 0001318605

### Example Workflow

```bash
# 1. Fetch Apple's 10-K filings
dotnet run --project src/RuleOne.ETL 0000320193 10-K

# 2. Query revenue data
dotnet run --project src/RuleOne.ETL concept Revenues

# 3. Open the notebook and visualize trends
# (Open samples/FinancialAnalysis.ipynb)
```

## Rule #1 Investing

This toolkit implements concepts from Phil Town's "Rule #1" investing philosophy:
- **Meaning** - Understanding the business fundamentals
- **Moat** - Competitive advantages (ROIC analysis)
- **Management** - Quality leadership indicators
- **Margin of Safety** - Buying at a discount to intrinsic value
- **Big Four Numbers** - Revenue, EPS, ROIC, Free Cash Flow growth

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This software is for educational and research purposes only. It is not financial advice. Always conduct your own research and consult with qualified financial advisors before making investment decisions.

## Acknowledgments

- SEC EDGAR API for public company data
- Plotly.NET for visualization capabilities
- F# community for excellent tooling and libraries
