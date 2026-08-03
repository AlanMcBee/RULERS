module SecEdgarTests

open System
open System.Net
open System.Net.Http
open Xunit
open RuleOne.ETL.SecEdgar

let private sampleCompanyFactsJson =
    """
{
  "cik": 789019,
  "entityName": "Microsoft Corp",
  "facts": {
    "us-gaap": {
      "Revenues": {
        "label": "Revenues",
        "units": {
          "USD": [
            {
              "end": "2024-06-30",
              "val": 245122000000,
              "accn": "0000950170-24-087843",
              "fy": 2024,
              "fp": "FY",
              "form": "10-K",
              "filed": "2024-07-30"
            },
            {
              "end": "2024-03-31",
              "val": 61750000000,
              "accn": "0000950170-24-035692",
              "fy": 2024,
              "fp": "Q3",
              "form": "10-Q",
              "filed": "2024-04-25"
            }
          ]
        }
      },
      "Assets": {
        "label": "Assets",
        "units": {
          "USD": [
            {
              "end": "2024-06-30",
              "val": 512000000000,
              "accn": "0000950170-24-087843",
              "fy": 2024,
              "fp": "FY",
              "form": "10-K",
              "filed": "2024-07-30"
            }
          ]
        }
      }
    }
  }
}
"""

[<Fact>]
let ``parseCompanyFactsCompanyName returns entityName`` () =
    let result = parseCompanyFactsCompanyName sampleCompanyFactsJson
    Assert.Equal(Some "Microsoft Corp", result)

[<Fact>]
let ``parseCompanyFactsByFormType filters rows by requested SEC form`` () =
    let filter = {
        Allow = None
        Deny = Set.empty
    }

    let facts = parseCompanyFactsByFormType sampleCompanyFactsJson "10-K" filter

    Assert.Equal(2, List.length facts)
    Assert.All(facts, fun fact -> Assert.Equal(Some "10-K", fact.FormType))

[<Fact>]
let ``parseCompanyFactsByFormType applies deny list`` () =
    let filter = {
        Allow = None
        Deny = Set.ofList [ "Assets" ]
    }

    let facts = parseCompanyFactsByFormType sampleCompanyFactsJson "10-K" filter

    Assert.Single(facts) |> ignore
    Assert.Equal("Revenues", facts.Head.Concept)

[<Fact>]
let ``parseCompanyFactsByFormType applies explicit allow list`` () =
    let filter = {
        Allow = Some (Set.ofList [ "Assets" ])
        Deny = Set.empty
    }

    let facts = parseCompanyFactsByFormType sampleCompanyFactsJson "10-K" filter

    Assert.Single(facts) |> ignore
    Assert.Equal("Assets", facts.Head.Concept)

[<Fact>]
let ``shouldRetryStatusCode retries on rate limit and server errors`` () =
    let retryable = [ HttpStatusCode.TooManyRequests; HttpStatusCode.InternalServerError; HttpStatusCode.BadGateway; HttpStatusCode.ServiceUnavailable; HttpStatusCode.GatewayTimeout ]
    let results = retryable |> List.map shouldRetryStatusCode
    Assert.All(results, fun value -> Assert.True(value))

[<Fact>]
let ``calculateRetryDelayMs returns fallback exponential backoff`` () =
    let delay = calculateRetryDelayMs 2 None
    Assert.True(delay >= 4000)

[<Fact>]
let ``calculateRetryDelayMs honors Retry-After header`` () =
    let response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
    response.Headers.RetryAfter <- new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(3.0))
    let delay = calculateRetryDelayMs 1 (Some response)
    Assert.Equal(3000, delay)

[<Fact>]
let ``parseTickerLookupJson resolves ticker to CIK`` () =
    let payload = """
    {
      "0": { "cik_str": 320193, "ticker": "AAPL", "title": "Apple Inc." },
      "1": { "cik_str": 789019, "ticker": "MSFT", "title": "Microsoft Corp" }
    }
    """

    let result = parseTickerLookupJson payload "AAPL"
    Assert.Equal(Some "0000320193", result)

[<Fact>]
let ``parseTickerLookupJson returns None when ticker is not found`` () =
    let payload = """
    {
      "0": { "cik_str": 320193, "ticker": "AAPL", "title": "Apple Inc." }
    }
    """

    let result = parseTickerLookupJson payload "ZZZZ"
    Assert.Equal(None, result)

[<Fact>]
let ``parseCompanyName returns name from submissions JSON`` () =
    let payload = """{ "cik": 789019, "name": "Microsoft Corporation" }"""
    let result = parseCompanyName payload
    Assert.Equal(Some "Microsoft Corporation", result)

[<Fact>]
let ``parseCompanyName returns None for malformed JSON`` () =
    let result = parseCompanyName "not json"
    Assert.Equal(None, result)
