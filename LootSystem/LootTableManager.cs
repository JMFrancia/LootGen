using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

//Singleton class that parses and stores loot tables from a JSON  
public class LootTableManager
{
    private const string ERR_READ_FILE = "Error reading file {0}.";
    private const string ERR_PARSE_DATA = "Error parsing data file {0}. JSON malformed.";
    private const string ERR_PARSE_TABLE = "Error parsing table in {0}: {1}";

    //Singelton setup
    public static LootTableManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LootTableManager();
            }

            return _instance;
        }
    }

    private static LootTableManager _instance;
    private Dictionary<string, LootTable> _lootTables;
    private bool testMode = true;
    
    //BUG: Trying to parse loot tables and validate them, but checking circular refs is part of validation
    //Wait until parsing is complete to check circular refs
    
    //Try to parse the loot tables
    private bool TryParseLootTables(string jsonPath, out List<LootTable> lootTables)
    {
        lootTables = null;
        var jsonString = "";
        try
        {
            jsonString = File.ReadAllText(jsonPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(string.Format(ERR_READ_FILE, jsonPath));
            return false;
        }

        try
        {
            lootTables = JsonSerializer.Deserialize<List<LootTable>>(jsonString);
        }
        catch
        {
            Console.WriteLine(string.Format(ERR_PARSE_DATA, jsonPath));
            return false;
        }

        return true;
    }

    //Run validation on parsed tables
    //TODO: Support multiple validation errors
    private bool ValidateTables(List<LootTable> lootTables)
    {
        var cachedTableNames = new HashSet<string>();
        
        foreach (LootTable table in lootTables)
        {
            //Check if table is a duplicate
            if (cachedTableNames.Contains(table.TableName))
            {
                Console.WriteLine("Duplicate table name {0} found!", table.TableName);
                return false;
            }
            else
            {
                cachedTableNames.Add(table.TableName);
            }
            
            //Validate table normally
            ValidationResult tableResult = table.ValidateTable();
            if (!tableResult.IsValid)
            {
                Console.WriteLine(string.Format(ERR_PARSE_TABLE, table.TableName, tableResult.ErrorMessage));
                return false;
            }

            //Check for circular references
            //NOTE: This is not part of normal table validation because it can't be done properly until all tables are added
            var circularRefCheck = LootTableContainsCircularReference(table);
            if (!circularRefCheck.IsValid)
            {
                Console.WriteLine(circularRefCheck.ErrorMessage);
                return false;
            }
        }

        return true;
    }

    //TODO: Move this to inside Loot Table validation
    //Returns valid if no circular references detected for a specific table
    public ValidationResult LootTableContainsCircularReference(LootTable table)
    {
        var cachedTables = new HashSet<string>(new[] { table.TableName });
        var tablesToVisit = new Queue<LootTable>(new[] { table });
        while (tablesToVisit.TryDequeue(out var thisTable))
        {
            foreach (var connectedTable in thisTable.GetAllTableTypeEntries())
            {
                if (cachedTables.Contains(connectedTable.TableName))
                {
                    return ValidationResult.Error(string.Format("Circular table reference detected from loot table {0}", table.TableName));
                }
                else
                {
                    //TODO: Handle duplicate table-type entries
                    cachedTables.Add(connectedTable.TableName);
                    tablesToVisit.Enqueue(connectedTable);
                }
            }
        }
        return ValidationResult.Valid();
    }

    public bool TryLoadLootTables(string JSONPath)
    {
        if (TryParseLootTables(JSONPath, out var tables)) 
        {
            _lootTables = new Dictionary<string, LootTable>();
            foreach (var table in tables)
            {
                _lootTables.Add(table.TableName, table);
            }
            Console.WriteLine("Successfully loaded tables from {0}", JSONPath);
            return ValidateTables(tables); //Done after to account for race condition w/ Circular Ref check
        }

        return false;
    }

    public bool TryGetLootTable(string tableName, out LootTable lootTable)
    {
        return _lootTables.TryGetValue(tableName, out lootTable);
    }
}