namespace RuleOne.ETL

open System
open System.Globalization
open System.IO
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text.Json
open System.Text.RegularExpressions
open System.Threading.Tasks

/// SEC EDGAR data fetching and parsing
module SecEdgar =

    type ConceptFilter = {
        Allow: Set<string> option
        Deny: Set<string>
    }

    type SecFact = {
        Concept: string
        Value: string option
        Unit: string option
        Period: string option
        FilingDate: string option
        FormType: string option
        AccessionNumber: string option
        FiscalYear: string option
        FiscalPeriod: string option
    }
    
    let private getConfiguredContact () =
        let configured = Environment.GetEnvironmentVariable("RULEONE_SEC_CONTACT")
        if String.IsNullOrWhiteSpace(configured) then
            "https://github.com/AlanMcBee/RULERS"
        else
            configured

    let private httpClient =
        lazy
            let client = new HttpClient()
            let contact = getConfiguredContact ()
            client.DefaultRequestHeaders.UserAgent.Clear()
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"RuleOne/1.0 (+{contact})")
            client.DefaultRequestHeaders.Accept.Clear()
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue("application/json"))
            client.DefaultRequestHeaders.AcceptEncoding.Clear()
            client.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue("gzip"))
            client.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue("deflate"))
            client.DefaultRequestHeaders.Add("Accept-Encoding", "br")
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty")
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors")
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin")
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest")
            client.DefaultRequestHeaders.Add("Contact", contact)
            client.DefaultRequestHeaders.Add("Origin", "https://www.sec.gov")
            client

    let shouldRetryStatusCode (statusCode: HttpStatusCode) =
        statusCode = HttpStatusCode.TooManyRequests
        || statusCode = HttpStatusCode.InternalServerError
        || statusCode = HttpStatusCode.BadGateway
        || statusCode = HttpStatusCode.ServiceUnavailable
        || statusCode = HttpStatusCode.GatewayTimeout

    let calculateRetryDelayMs (attempt: int) (response: HttpResponseMessage option) =
        match response with
        | Some response when response.Headers.RetryAfter <> null && response.Headers.RetryAfter.Delta.HasValue ->
            int (response.Headers.RetryAfter.Delta.Value.TotalMilliseconds)
        | _ ->
            let baseDelayMs = 1000
            let cappedDelay = min 8000 (baseDelayMs * (pown 2 attempt))
            cappedDelay

    let private shouldThrottle (statusCode: HttpStatusCode) =
        statusCode = HttpStatusCode.TooManyRequests
        || statusCode = HttpStatusCode.Forbidden

    let private sendWithRetry (requestUri: Uri) =
        let rec loop attempt =
            task {
                let! response = httpClient.Value.GetAsync(requestUri)
                if response.IsSuccessStatusCode then
                    let! content = response.Content.ReadAsStringAsync()
                    return content, response
                elif attempt < 4 && shouldRetryStatusCode response.StatusCode then
                    let delayMs = calculateRetryDelayMs attempt (Some response)
                    do! Task.Delay(delayMs)
                    return! loop (attempt + 1)
                elif attempt = 0 && shouldThrottle response.StatusCode then
                    do! Task.Delay(2000)
                    return! loop (attempt + 1)
                else
                    response.EnsureSuccessStatusCode() |> ignore
                    let! content = response.Content.ReadAsStringAsync()
                    return content, response
            }

        loop 0

    let private defaultAllowedConcepts =
        [
            "Revenues"
            "RevenueFromContractWithCustomerExcludingAssessedTax"
            "SalesRevenueNet"
            "NetIncomeLoss"
            "OperatingIncomeLoss"
            "Assets"
            "Liabilities"
            "StockholdersEquity"
            "CashAndCashEquivalentsAtCarryingValue"
            "NetCashProvidedByUsedInOperatingActivities"
            "PaymentsToAcquirePropertyPlantAndEquipment"
            "CommonStockSharesOutstanding"
            "WeightedAverageNumberOfDilutedSharesOutstanding"
            "EarningsPerShareBasic"
            "EarningsPerShareDiluted"
            "LongTermDebt"
            "LongTermDebtNoncurrent"
            "InterestExpense"
            "CostOfGoodsSold"
            "GrossProfit"
            "ResearchAndDevelopmentExpense"
            "SellingGeneralAndAdministrativeExpense"
        ]
        |> Set.ofList

    let private defaultConceptFilter = {
        Allow = Some defaultAllowedConcepts
        Deny = Set.empty
    }
    
    /// Fetch ticker lookup data from SEC company tickers feed.
    let fetchTickerLookup () : Task<string> =
        let url = "https://www.sec.gov/files/company_tickers.json"

        task {
            let! content, _ = sendWithRetry (Uri(url))
            return content
        }

    /// Fetch company submissions metadata from SEC EDGAR API
    let fetchCompanySubmissions (cik: string) : Task<string> =
        // Pad CIK to 10 digits
        let paddedCik = cik.PadLeft(10, '0')
        let url = $"https://data.sec.gov/submissions/CIK{paddedCik}.json"
        
        task {
            let! content, _ = sendWithRetry (Uri(url))
            return content
        }

    /// Fetch company facts from SEC EDGAR XBRL API
    let fetchCompanyFacts (cik: string) : Task<string> =
        let paddedCik = cik.PadLeft(10, '0')
        let url = $"https://data.sec.gov/api/xbrl/companyfacts/CIK{paddedCik}.json"

        task {
            let! content, _ = sendWithRetry (Uri(url))
            return content
        }
    
    /// Parse basic company information from submissions JSON
    let parseCompanyName (json: string) : string option =
        try
            // Simple regex-based parsing for name field
            let nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"")
            if nameMatch.Success then
                Some nameMatch.Groups.[1].Value
            else
                None
        with
        | _ -> None

    /// Parse company name from companyfacts JSON
    let parseCompanyFactsCompanyName (json: string) : string option =
        try
            use document = JsonDocument.Parse(json)
            let mutable entityName = Unchecked.defaultof<JsonElement>
            if document.RootElement.TryGetProperty("entityName", &entityName) then
                match entityName.ValueKind with
                | JsonValueKind.String -> Some (entityName.GetString())
                | _ -> None
            else
                None
        with
        | _ -> None
    
    /// Extract filing accession numbers from submissions JSON
    let extractFilingAccessions (json: string) (formType: string) : string list =
        try
            // Extract recent filings matching the form type (10-K or 10-Q)
            let filingPattern = $"\"{formType}\""
            let accessionPattern = "\"accessionNumber\"\\s*:\\s*\"([^\"]+)\""
            
            let formMatches = Regex.Matches(json, filingPattern)
            let accessionMatches = Regex.Matches(json, accessionPattern)
            
            // Simple approach: return first 5 accession numbers found
            accessionMatches
            |> Seq.cast<Match>
            |> Seq.take (min 5 accessionMatches.Count)
            |> Seq.map (fun m -> m.Groups.[1].Value)
            |> Seq.toList
        with
        | _ -> []

    let private tryGetProperty (name: string) (element: JsonElement) =
        let mutable value = Unchecked.defaultof<JsonElement>
        if element.TryGetProperty(name, &value) then Some value else None

    let private tryGetStringProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some value when value.ValueKind = JsonValueKind.String ->
            let raw = value.GetString()
            if String.IsNullOrWhiteSpace(raw) then None else Some raw
        | _ -> None

    let private jsonValueToString (value: JsonElement) =
        match value.ValueKind with
        | JsonValueKind.String ->
            let raw = value.GetString()
            if String.IsNullOrWhiteSpace(raw) then None else Some raw
        | JsonValueKind.Number
        | JsonValueKind.True
        | JsonValueKind.False -> Some (value.GetRawText())
        | _ -> None

    let private toPeriod (factNode: JsonElement) =
        let startDate = tryGetStringProperty "start" factNode
        let endDate = tryGetStringProperty "end" factNode
        let instantDate = tryGetStringProperty "end" factNode |> Option.orElse (tryGetStringProperty "instant" factNode)

        match startDate, endDate, instantDate with
        | Some s, Some e, _ -> Some $"{s}..{e}"
        | _, Some e, _ -> Some e
        | _, _, Some i -> Some i
        | _ -> None

    let private shouldIncludeConcept (conceptFilter: ConceptFilter) (concept: string) =
        if conceptFilter.Deny.Contains(concept) then
            false
        else
            match conceptFilter.Allow with
            | Some allowSet -> allowSet.Contains(concept)
            | None -> true

    let private parseConceptFilterJson (json: string) =
        use document = JsonDocument.Parse(json)

        let parseListProperty name =
            match tryGetProperty name document.RootElement with
            | Some property when property.ValueKind = JsonValueKind.Array ->
                property.EnumerateArray()
                |> Seq.choose (fun item ->
                    if item.ValueKind = JsonValueKind.String then
                        let value = item.GetString()
                        if String.IsNullOrWhiteSpace(value) then None else Some value
                    else
                        None)
                |> Set.ofSeq
                |> Some
            | _ -> None

        let allow = parseListProperty "allow"
        let deny = parseListProperty "deny" |> Option.defaultValue Set.empty

        {
            Allow = allow
            Deny = deny
        }

    /// Load concept filter from JSON file.
    /// Format: { "allow": ["Revenues"], "deny": ["Assets"] }
    let loadConceptFilter (filePath: string option) : ConceptFilter =
        match filePath with
        | None -> defaultConceptFilter
        | Some path when String.IsNullOrWhiteSpace(path) -> defaultConceptFilter
        | Some path when not (File.Exists(path)) ->
            printfn "Concept filter file not found at '%s'. Falling back to default concept filter." path
            defaultConceptFilter
        | Some path ->
            try
                let json = File.ReadAllText(path)
                let filter = parseConceptFilterJson json

                if filter.Allow.IsNone && filter.Deny.IsEmpty then
                    defaultConceptFilter
                else
                    filter
            with
            | ex ->
                printfn "Could not parse concept filter file '%s': %s. Falling back to default concept filter." path ex.Message
                defaultConceptFilter
    
    let parseTickerLookupJson (json: string) (ticker: string) : string option =
        try
            use document = JsonDocument.Parse(json)
            let normalizedTicker = ticker.ToUpperInvariant()

            if document.RootElement.ValueKind = JsonValueKind.Object then
                let mutable foundCik = None

                for property in document.RootElement.EnumerateObject() do
                    if foundCik.IsNone && property.Value.ValueKind = JsonValueKind.Object then
                        let mutable cik = Unchecked.defaultof<JsonElement>
                        let mutable tickerValue = Unchecked.defaultof<JsonElement>

                        if property.Value.TryGetProperty("cik_str", &cik)
                           && property.Value.TryGetProperty("ticker", &tickerValue) then
                            let candidateTicker =
                                match tickerValue.ValueKind with
                                | JsonValueKind.String -> tickerValue.GetString()
                                | _ -> null

                            if String.Equals(candidateTicker, normalizedTicker, StringComparison.OrdinalIgnoreCase) then
                                match cik.ValueKind with
                                | JsonValueKind.Number ->
                                    let preciseCik = cik.GetInt64()
                                    foundCik <- Some (preciseCik.ToString("D10"))
                                | JsonValueKind.String ->
                                    let parsed = Int64.Parse(cik.GetString())
                                    foundCik <- Some (parsed.ToString("D10"))
                                | _ -> ()

                foundCik
            else
                None
        with
        | ex ->
            printfn "Error parsing ticker lookup JSON: %s" ex.Message
            None

    /// Parse SEC companyfacts JSON into normalized facts and filter by form type.
    let parseCompanyFactsByFormType (json: string) (formType: string) (conceptFilter: ConceptFilter) : SecFact list =
        try
            use document = JsonDocument.Parse(json)

            let factsNode =
                match tryGetProperty "facts" document.RootElement with
                | Some node -> node
                | None -> raise (InvalidOperationException("Missing 'facts' node in SEC companyfacts response."))

            let formTypeUpper = formType.ToUpperInvariant()

            let factRows = ResizeArray<SecFact>()

            for taxonomy in factsNode.EnumerateObject() do
                for conceptProperty in taxonomy.Value.EnumerateObject() do
                    let concept = conceptProperty.Name

                    if shouldIncludeConcept conceptFilter concept then
                        match tryGetProperty "units" conceptProperty.Value with
                        | Some unitsNode when unitsNode.ValueKind = JsonValueKind.Object ->
                            for unitProperty in unitsNode.EnumerateObject() do
                                let unitName = unitProperty.Name

                                if unitProperty.Value.ValueKind = JsonValueKind.Array then
                                    for item in unitProperty.Value.EnumerateArray() do
                                        let itemFormType = tryGetStringProperty "form" item

                                        match itemFormType with
                                        | Some f when f.ToUpperInvariant() = formTypeUpper ->
                                            let parsed = {
                                                Concept = concept
                                                Value = tryGetProperty "val" item |> Option.bind jsonValueToString
                                                Unit = Some unitName
                                                Period = toPeriod item
                                                FilingDate = tryGetStringProperty "filed" item
                                                FormType = itemFormType
                                                AccessionNumber = tryGetStringProperty "accn" item
                                                FiscalYear = tryGetProperty "fy" item |> Option.bind jsonValueToString
                                                FiscalPeriod = tryGetStringProperty "fp" item
                                            }

                                            factRows.Add(parsed)
                                        | _ -> ()
                        | _ -> ()

            factRows
            |> Seq.sortByDescending (fun row ->
                match row.FilingDate with
                | Some filingDate ->
                    match DateTime.TryParse(filingDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) with
                    | true, parsedDate -> parsedDate
                    | _ -> DateTime.MinValue
                | None -> DateTime.MinValue)
            |> Seq.toList
        with
        | ex ->
            printfn "Error parsing company facts JSON: %s" ex.Message
            []
    
    /// Fetch and parse 10-K/10-Q facts for a given CIK from SEC companyfacts endpoint.
    let fetchAndParseFiling (cik: string) (formType: string) (conceptFilter: ConceptFilter) : Task<SecFact list> =
        task {
            try
                let! companyFactsJson = fetchCompanyFacts cik
                let facts = parseCompanyFactsByFormType companyFactsJson formType conceptFilter
                return facts
            with
            | ex ->
                printfn "Error fetching filing: %s" ex.Message
                return []
        }
