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
            // Fetch company submissions
            let submissionsTask = fetchCompanySubmissions cik
            let json = submissionsTask.Result
            
            let companyName = parseCompanyName json
            match companyName with
            | Some name -> printfn "Company: %s" name
            | None -> printfn "Company name not found"
            
            printfn "Fetching %s filings..." formType
            
            // Fetch and parse filings
            let filingsTask = fetchAndParseFiling cik formType
            let filings = filingsTask.Result
            
            printfn "Found %d filings" (List.length filings)
            
            // Store facts in database
            let mutable totalFacts = 0
            for (accession, facts) in filings do
                printfn "Processing filing: %s" accession
                for (concept, value, unit, period) in facts do
                    insertFact connectionString cik companyName None (Some formType) concept value unit None period
                    totalFacts <- totalFacts + 1
            
            printfn ""
            printfn "Successfully stored %d facts in database" totalFacts
            
            // Query and display sample data
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
    
    | _ ->
        printfn "Usage:"
        printfn "  dotnet run <CIK> <10-K|10-Q>    - Fetch and store SEC filings"
        printfn "  dotnet run query <CIK>           - Query facts by CIK"
        printfn "  dotnet run concept <CONCEPT>     - Query facts by concept name"
        printfn ""
        printfn "Examples:"
        printfn "  dotnet run 0000789019 10-K       - Fetch Microsoft 10-K filings"
        printfn "  dotnet run query 0000789019      - Query Microsoft facts"
        printfn "  dotnet run concept Revenues      - Query all revenue facts"
        1 // Error
