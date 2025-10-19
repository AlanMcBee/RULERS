namespace RuleOne.Analytics

/// Financial analytics calculations for Rule #1 investing
module FinancialMetrics =
    
    /// Calculate Compound Annual Growth Rate (CAGR)
    /// CAGR = (Ending Value / Beginning Value) ^ (1 / Number of Years) - 1
    let calculateCAGR (beginningValue: decimal) (endingValue: decimal) (years: decimal) : decimal option =
        if beginningValue <= 0m || endingValue <= 0m || years <= 0m then
            None
        else
            let ratio = endingValue / beginningValue
            let exponent = 1.0 / (float years)
            let cagr = (float ratio) ** exponent - 1.0
            Some (decimal cagr)
    
    /// Calculate Return on Invested Capital (ROIC) - Placeholder
    /// ROIC = NOPAT / Invested Capital
    /// This is a placeholder implementation
    let calculateROIC (nopat: decimal) (investedCapital: decimal) : decimal option =
        if investedCapital <= 0m then
            None
        else
            Some (nopat / investedCapital)
    
    /// Calculate Earnings Per Share (EPS) - Placeholder
    /// EPS = Net Income / Outstanding Shares
    let calculateEPS (netIncome: decimal) (outstandingShares: decimal) : decimal option =
        if outstandingShares <= 0m then
            None
        else
            Some (netIncome / outstandingShares)
    
    /// Calculate Margin of Safety (MOS) - Placeholder
    /// MOS = (Intrinsic Value - Current Price) / Intrinsic Value
    let calculateMOS (intrinsicValue: decimal) (currentPrice: decimal) : decimal option =
        if intrinsicValue <= 0m then
            None
        else
            let mos = (intrinsicValue - currentPrice) / intrinsicValue
            Some mos
