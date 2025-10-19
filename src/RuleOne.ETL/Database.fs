namespace RuleOne.ETL

open System
open Microsoft.Data.Sqlite

/// SQLite database operations for storing SEC EDGAR XBRL facts
module Database =
    
    /// Initialize the database schema
    let initializeDatabase (connectionString: string) =
        use connection = new SqliteConnection(connectionString)
        connection.Open()
        
        let createTableCommand = connection.CreateCommand()
        createTableCommand.CommandText <- """
            CREATE TABLE IF NOT EXISTS Facts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CIK TEXT NOT NULL,
                CompanyName TEXT,
                FilingDate TEXT,
                FormType TEXT,
                Concept TEXT NOT NULL,
                Value TEXT,
                Unit TEXT,
                ContextRef TEXT,
                Period TEXT,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            );
            
            CREATE INDEX IF NOT EXISTS idx_cik ON Facts(CIK);
            CREATE INDEX IF NOT EXISTS idx_concept ON Facts(Concept);
            CREATE INDEX IF NOT EXISTS idx_filing_date ON Facts(FilingDate);
        """
        createTableCommand.ExecuteNonQuery() |> ignore
        connection.Close()
    
    /// Insert a fact into the database
    let insertFact (connectionString: string) (cik: string) (companyName: string option) 
                   (filingDate: string option) (formType: string option) (concept: string) 
                   (value: string option) (unit: string option) (contextRef: string option) 
                   (period: string option) =
        use connection = new SqliteConnection(connectionString)
        connection.Open()
        
        let insertCommand = connection.CreateCommand()
        insertCommand.CommandText <- """
            INSERT INTO Facts (CIK, CompanyName, FilingDate, FormType, Concept, Value, Unit, ContextRef, Period)
            VALUES (@cik, @companyName, @filingDate, @formType, @concept, @value, @unit, @contextRef, @period)
        """
        
        let addParam name value =
            match value with
            | Some v -> insertCommand.Parameters.AddWithValue(name, v) |> ignore
            | None -> insertCommand.Parameters.AddWithValue(name, DBNull.Value) |> ignore
        
        insertCommand.Parameters.AddWithValue("@cik", cik) |> ignore
        addParam "@companyName" companyName
        addParam "@filingDate" filingDate
        addParam "@formType" formType
        insertCommand.Parameters.AddWithValue("@concept", concept) |> ignore
        addParam "@value" value
        addParam "@unit" unit
        addParam "@contextRef" contextRef
        addParam "@period" period
        
        insertCommand.ExecuteNonQuery() |> ignore
        connection.Close()
    
    /// Query facts by CIK
    let queryFactsByCIK (connectionString: string) (cik: string) =
        use connection = new SqliteConnection(connectionString)
        connection.Open()
        
        let queryCommand = connection.CreateCommand()
        queryCommand.CommandText <- "SELECT * FROM Facts WHERE CIK = @cik ORDER BY FilingDate DESC"
        queryCommand.Parameters.AddWithValue("@cik", cik) |> ignore
        
        use reader = queryCommand.ExecuteReader()
        
        let results = ResizeArray<_>()
        while reader.Read() do
            results.Add({|
                Id = reader.GetInt32(0)
                CIK = reader.GetString(1)
                CompanyName = if reader.IsDBNull(2) then None else Some(reader.GetString(2))
                FilingDate = if reader.IsDBNull(3) then None else Some(reader.GetString(3))
                FormType = if reader.IsDBNull(4) then None else Some(reader.GetString(4))
                Concept = reader.GetString(5)
                Value = if reader.IsDBNull(6) then None else Some(reader.GetString(6))
                Unit = if reader.IsDBNull(7) then None else Some(reader.GetString(7))
                ContextRef = if reader.IsDBNull(8) then None else Some(reader.GetString(8))
                Period = if reader.IsDBNull(9) then None else Some(reader.GetString(9))
            |})
        
        connection.Close()
        results |> Seq.toList
    
    /// Query facts by concept (e.g., "Revenues", "NetIncomeLoss")
    let queryFactsByConcept (connectionString: string) (concept: string) =
        use connection = new SqliteConnection(connectionString)
        connection.Open()
        
        let queryCommand = connection.CreateCommand()
        queryCommand.CommandText <- "SELECT * FROM Facts WHERE Concept LIKE @concept ORDER BY FilingDate DESC"
        queryCommand.Parameters.AddWithValue("@concept", $"%%{concept}%%") |> ignore
        
        use reader = queryCommand.ExecuteReader()
        
        let results = ResizeArray<_>()
        while reader.Read() do
            results.Add({|
                Id = reader.GetInt32(0)
                CIK = reader.GetString(1)
                CompanyName = if reader.IsDBNull(2) then None else Some(reader.GetString(2))
                FilingDate = if reader.IsDBNull(3) then None else Some(reader.GetString(3))
                FormType = if reader.IsDBNull(4) then None else Some(reader.GetString(4))
                Concept = reader.GetString(5)
                Value = if reader.IsDBNull(6) then None else Some(reader.GetString(6))
                Unit = if reader.IsDBNull(7) then None else Some(reader.GetString(7))
                ContextRef = if reader.IsDBNull(8) then None else Some(reader.GetString(8))
                Period = if reader.IsDBNull(9) then None else Some(reader.GetString(9))
            |})
        
        connection.Close()
        results |> Seq.toList
