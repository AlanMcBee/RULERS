namespace RuleOne.ETL

open System
open System.Net.Http
open System.Text.RegularExpressions
open System.Threading.Tasks

/// SEC EDGAR data fetching and parsing
module SecEdgar =
    
    let private httpClient = new HttpClient()
    
    // SEC requires a User-Agent header
    do httpClient.DefaultRequestHeaders.Add("User-Agent", "RuleOne ETL alan@example.com")
    
    /// Fetch company submissions metadata from SEC EDGAR API
    let fetchCompanySubmissions (cik: string) : Task<string> =
        // Pad CIK to 10 digits
        let paddedCik = cik.PadLeft(10, '0')
        let url = $"https://data.sec.gov/submissions/CIK{paddedCik}.json"
        
        task {
            let! response = httpClient.GetAsync(url)
            response.EnsureSuccessStatusCode() |> ignore
            let! content = response.Content.ReadAsStringAsync()
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
    
    /// Parse XBRL-like data from filing (simplified placeholder)
    /// In a real implementation, this would parse actual XBRL files
    let parseXBRLFacts (cik: string) (accessionNumber: string) : Task<(string * string option * string option * string option) list> =
        task {
            try
                // This is a simplified placeholder
                // In production, you would fetch and parse the actual XBRL instance document
                // For now, return mock data structure
                return [
                    ("Revenues", Some "1000000000", Some "USD", Some "2024-Q4")
                    ("NetIncomeLoss", Some "150000000", Some "USD", Some "2024-Q4")
                    ("Assets", Some "5000000000", Some "USD", Some "2024-Q4")
                    ("StockholdersEquity", Some "2000000000", Some "USD", Some "2024-Q4")
                ]
            with
            | _ -> return []
        }
    
    /// Fetch and parse 10-K/10-Q data for a given CIK
    let fetchAndParseFiling (cik: string) (formType: string) : Task<(string * (string * string option * string option * string option) list) list> =
        task {
            try
                let! json = fetchCompanySubmissions cik
                let accessions = extractFilingAccessions json formType
                
                let! facts = 
                    accessions
                    |> List.map (fun acc -> 
                        task {
                            let! factsList = parseXBRLFacts cik acc
                            return (acc, factsList)
                        })
                    |> Task.WhenAll
                
                return facts |> Array.toList
            with
            | ex ->
                printfn "Error fetching filing: %s" ex.Message
                return []
        }
