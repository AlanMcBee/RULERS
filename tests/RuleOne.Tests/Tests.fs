module Tests

open System
open Xunit
open RuleOne.Analytics.FinancialMetrics

[<Fact>]
let ``CAGR calculation with valid inputs returns correct result`` () =
    let beginningValue = 100m
    let endingValue = 200m
    let years = 5m
    
    let result = calculateCAGR beginningValue endingValue years
    
    match result with
    | Some cagr ->
        // Expected CAGR for doubling in 5 years is approximately 14.87%
        Assert.True(cagr > 0.14m && cagr < 0.15m, $"CAGR was {cagr}")
    | None ->
        Assert.True(false, "CAGR calculation should return Some value")

[<Fact>]
let ``CAGR calculation with zero beginning value returns None`` () =
    let result = calculateCAGR 0m 200m 5m
    Assert.True(result.IsNone, "CAGR with zero beginning value should return None")

[<Fact>]
let ``CAGR calculation with negative years returns None`` () =
    let result = calculateCAGR 100m 200m -5m
    Assert.True(result.IsNone, "CAGR with negative years should return None")

[<Fact>]
let ``ROIC calculation with valid inputs returns correct result`` () =
    let nopat = 1000000m
    let investedCapital = 5000000m
    
    let result = calculateROIC nopat investedCapital
    
    match result with
    | Some roic ->
        Assert.Equal(0.2m, roic) // 20% ROIC
    | None ->
        Assert.True(false, "ROIC calculation should return Some value")

[<Fact>]
let ``ROIC calculation with zero invested capital returns None`` () =
    let result = calculateROIC 1000000m 0m
    Assert.True(result.IsNone, "ROIC with zero invested capital should return None")

[<Fact>]
let ``EPS calculation with valid inputs returns correct result`` () =
    let netIncome = 10000000m
    let outstandingShares = 1000000m
    
    let result = calculateEPS netIncome outstandingShares
    
    match result with
    | Some eps ->
        Assert.Equal(10m, eps) // $10 per share
    | None ->
        Assert.True(false, "EPS calculation should return Some value")

[<Fact>]
let ``EPS calculation with zero shares returns None`` () =
    let result = calculateEPS 10000000m 0m
    Assert.True(result.IsNone, "EPS with zero shares should return None")

[<Fact>]
let ``MOS calculation with valid inputs returns correct result`` () =
    let intrinsicValue = 100m
    let currentPrice = 75m
    
    let result = calculateMOS intrinsicValue currentPrice
    
    match result with
    | Some mos ->
        Assert.Equal(0.25m, mos) // 25% margin of safety
    | None ->
        Assert.True(false, "MOS calculation should return Some value")

[<Fact>]
let ``MOS calculation with zero intrinsic value returns None`` () =
    let result = calculateMOS 0m 75m
    Assert.True(result.IsNone, "MOS with zero intrinsic value should return None")

[<Fact>]
let ``MOS calculation with negative margin returns negative result`` () =
    let intrinsicValue = 100m
    let currentPrice = 125m
    
    let result = calculateMOS intrinsicValue currentPrice
    
    match result with
    | Some mos ->
        Assert.True(mos < 0m, "MOS should be negative when price exceeds value")
    | None ->
        Assert.True(false, "MOS calculation should return Some value")
