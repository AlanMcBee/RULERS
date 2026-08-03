open System
open System.IO
open RuleOne.ETL.Database
open RuleOne.ETL.SecEdgar

/// Main ETL application for fetching SEC EDGAR data
[<EntryPoint>]
let main argv =
    printfn "RuleOne ETL - SEC EDGAR Data Fetcher"
    printfn "====================================="
    printfn ""
    
    // Database setup
    let dbPath = Path.Combine(Environment.CurrentDirectory, "ruleone.db")
    let connectionString = $"Data Source={dbPath}"
    
    printfn "Initializing database at: %s" dbPath
    initializeDatabase connectionString
    printfn "Database initialized successfully"
    printfn ""
    
    // Parse command line arguments
    match argv with
    | [| cik; formType |] when formType = "10-K" || formType = "10-Q" ->
        printfn "Fetching %s filings for CIK: %s" formType cik
        printfn ""

        try
            let conceptFilter = loadConceptFilter None

            // Fetch metadata and company facts once, then parse locally.
            let submissionsTask = fetchCompanySubmissions cik
            let submissionsJson = submissionsTask.Result

            let companyFactsTask = fetchCompanyFacts cik
            let companyFactsJson = companyFactsTask.Result

            let companyName =
                parseCompanyFactsCompanyName companyFactsJson
                |> Option.orElse (parseCompanyName submissionsJson)

            match companyName with
            | Some name -> printfn "Company: %s" name
            | None -> printfn "Company name not found"

            printfn "Parsing %s facts from SEC companyfacts endpoint..." formType
            let facts = parseCompanyFactsByFormType companyFactsJson formType conceptFilter

            let filingCount =
                facts
                |> List.choose (fun fact -> fact.AccessionNumber)
                |> Set.ofList
                |> Set.count

            printfn "Found %d facts across %d filings" (List.length facts) filingCount

            // Store facts in database.
            let mutable totalFacts = 0
            for fact in facts do
                insertFact
                    connectionString
                    cik
                    companyName
                    fact.FilingDate
                    fact.FormType
                    fact.Concept
                    fact.Value
                    fact.Unit
                    None
                    fact.Period
                    fact.AccessionNumber
                    fact.FiscalYear
                    fact.FiscalPeriod

                totalFacts <- totalFacts + 1

            printfn ""
            printfn "Successfully stored %d facts in database" totalFacts

            // Query and display sample data.
            printfn ""
            printfn "Sample data from database:"
            let sampleFacts = queryFactsByCIK connectionString cik |> List.truncate 5
            for fact in sampleFacts do
                printfn "  %s: %s %s" fact.Concept (Option.defaultValue "N/A" fact.Value) (Option.defaultValue "" fact.Unit)

            0 // Success
        with
        | ex ->
            printfn "Error: %s" ex.Message
            1 // Error

    | [| cik; formType; "--concept-filter"; conceptFilterPath |] when formType = "10-K" || formType = "10-Q" ->
        printfn "Fetching %s filings for CIK: %s" formType cik
        printfn ""

        try
            let conceptFilter = loadConceptFilter (Some conceptFilterPath)

            // Fetch metadata and company facts once, then parse locally.
            let submissionsTask = fetchCompanySubmissions cik
            let submissionsJson = submissionsTask.Result

            let companyFactsTask = fetchCompanyFacts cik
            let companyFactsJson = companyFactsTask.Result

            let companyName =
                parseCompanyFactsCompanyName companyFactsJson
                |> Option.orElse (parseCompanyName submissionsJson)

            match companyName with
            | Some name -> printfn "Company: %s" name
            | None -> printfn "Company name not found"

            printfn "Parsing %s facts from SEC companyfacts endpoint with filter: %s" formType conceptFilterPath
            let facts = parseCompanyFactsByFormType companyFactsJson formType conceptFilter

            let filingCount =
                facts
                |> List.choose (fun fact -> fact.AccessionNumber)
                |> Set.ofList
                |> Set.count

            printfn "Found %d facts across %d filings" (List.length facts) filingCount

            // Store facts in database.
            let mutable totalFacts = 0
            for fact in facts do
                insertFact
                    connectionString
                    cik
                    companyName
                    fact.FilingDate
                    fact.FormType
                    fact.Concept
                    fact.Value
                    fact.Unit
                    None
                    fact.Period
                    fact.AccessionNumber
                    fact.FiscalYear
                    fact.FiscalPeriod

                totalFacts <- totalFacts + 1

            printfn ""
            printfn "Successfully stored %d facts in database" totalFacts

            // Query and display sample data.
            printfn ""
            printfn "Sample data from database:"
            let sampleFacts = queryFactsByCIK connectionString cik |> List.truncate 5
            for fact in sampleFacts do
                printfn "  %s: %s %s" fact.Concept (Option.defaultValue "N/A" fact.Value) (Option.defaultValue "" fact.Unit)

            0 // Success
        with
        | ex ->
            printfn "Error: %s" ex.Message
            1 // Error
    
    | [| "lookup"; ticker |] ->
        printfn "Looking up CIK for ticker: %s" ticker
        printfn ""

        try
            let tickerJson = fetchTickerLookup () |> Async.AwaitTask |> Async.RunSynchronously
            let resolvedCik = parseTickerLookupJson tickerJson ticker

            match resolvedCik with
            | Some cik -> printfn "Resolved %s -> %s" ticker cik
            | None -> printfn "Could not resolve ticker %s" ticker

            0
        with
        | ex ->
            printfn "Could not resolve ticker %s from SEC data. The SEC access request was rejected or rate-limited. Details: %s" ticker ex.Message
            1

    | [| "query"; cik |] ->
        printfn "Querying facts for CIK: %s" cik
        printfn ""
        
        let facts = queryFactsByCIK connectionString cik
        printfn "Found %d facts" (List.length facts)
        
        for fact in facts |> List.truncate 20 do
            printfn "%s | %s | %s: %s %s" 
                (Option.defaultValue "N/A" fact.FilingDate)
                (Option.defaultValue "N/A" fact.FormType)
                fact.Concept 
                (Option.defaultValue "N/A" fact.Value)
                (Option.defaultValue "" fact.Unit)
        
        0 // Success
    
    | [| "concept"; concept |] ->
        printfn "Querying facts for concept: %s" concept
        printfn ""
        
        let facts = queryFactsByConcept connectionString concept
        printfn "Found %d facts" (List.length facts)
        
        for fact in facts |> List.truncate 20 do
            printfn "%s | %s | %s: %s %s" 
                fact.CIK
                (Option.defaultValue "N/A" fact.CompanyName)
                fact.Concept 
                (Option.defaultValue "N/A" fact.Value)
                (Option.defaultValue "" fact.Unit)
        
        0 // Success

    | [| "list" |] ->
        printfn "Listing securities in database"
        printfn ""

        let securities = listSecurities connectionString
        printfn "Found %d securities" (List.length securities)

        for security in securities do
            printfn "%s | %s | %s | %d facts"
                security.CIK
                (Option.defaultValue "N/A" security.CompanyName)
                (Option.defaultValue "N/A" security.LastFilingDate)
                security.FactCount

        0 // Success
    
    | _ ->
        printfn "Usage:"
        printfn "  dotnet run <CIK> <10-K|10-Q>                              - Fetch and store SEC filings"
        printfn "  dotnet run <CIK> <10-K|10-Q> --concept-filter <path>      - Fetch with concept allow/deny JSON"
        printfn "  dotnet run lookup <TICKER>                                - Resolve ticker to CIK"
        printfn "  dotnet run query <CIK>                                     - Query facts by CIK"
        printfn "  dotnet run concept <CONCEPT>                               - Query facts by concept name"
        printfn "  dotnet run list                                            - List securities currently in the database"
        printfn ""
        printfn "Examples:"
        printfn "  dotnet run 0000789019 10-K                             - Fetch Microsoft 10-K filings"
        printfn "  dotnet run 0000789019 10-K --concept-filter .\\concepts.json"
        printfn "  dotnet run lookup AAPL                                  - Resolve Apple ticker to CIK"
        printfn "  dotnet run query 0000789019                            - Query Microsoft facts"
        printfn "  dotnet run concept Revenues                            - Query all revenue facts"
        printfn "  dotnet run list                                         - List securities in the database"
        1 // Error
