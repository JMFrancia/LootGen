using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace LootSystem;

/*
 * Singleton class that parses loot tables from a JSON and stores them, managing access
 */  
public class LootTableManager
{
    private const string ERR_READ_FILE = "Error reading file {0}.";
    private const string ERR_PARSE_DATA = "Error parsing data file {0}. JSON malformed.";
    private const string ERR_PARSE_TABLE = "Error parsing table {0}: {1}";
    private const string ERR_PARSE_TABLE_NO_NAME = "Error parsing table: {1}";
    private const string ERR_DUPLICATE_TABLE = "Duplicate table name {0} found!";
    private const string ERR_CIRCULAR_TABLE_REF = "Circular table reference detected from loot table {0}.";


    //Singleton setup
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
    
    //TODO: Remove this
    private bool testMode = true;
    
    
    /*
     * Tries to parse loot tables from given jsonPath
     */
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

    /*
     * Validates a list of tables
     */
    private bool ValidateTables(List<LootTable> lootTables)
    {
        foreach (LootTable table in lootTables)
        {
            //Validate table normally
            ValidationResult tableResult = table.ValidateTable();
            if (!tableResult.IsValid)
            {
                var msg = string.IsNullOrEmpty(table.TableName) ? ERR_PARSE_TABLE_NO_NAME : ERR_PARSE_TABLE;
                Console.WriteLine(string.Format(msg, table.TableName, tableResult.ErrorMessage));
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
    
    /*
    Returns valid if no circular references detected for a specific table
    This is done here in TableManager instead of being a part of the LootTable's validation method
    in order to ensure that it occurs after all tables are loaded
    */
    public ValidationResult LootTableContainsCircularReference(LootTable table)
    {
        if (HasCircularReference(table, new HashSet<string>()))
        {
            return ValidationResult.Error(string.Format(ERR_CIRCULAR_TABLE_REF, table.TableName));
        }
        return ValidationResult.Valid();
    }

    private bool HasCircularReference(LootTable table, HashSet<string> currentBranch)
    {
        if (currentBranch.Contains(table.TableName))
        {
            return true;
        }

        currentBranch.Add(table.TableName);
        foreach (var connectedTable in table.GetAllTableTypeEntries())
        {
            if (HasCircularReference(connectedTable, currentBranch))
            {
                return true;
            }
        }

        currentBranch.Remove(table.TableName);
        return false;
    }

    /*
     * Tries to load loot tables from given JSON path
     */
    public bool TryLoadLootTables(string JSONPath)
    {
        if (TryParseLootTables(JSONPath, out var tables)) 
        {
            _lootTables = new Dictionary<string, LootTable>();
            foreach (var table in tables)
            {
                //Check if table is a duplicate
                if (_lootTables.ContainsKey(table.TableName))
                {
                    Console.WriteLine(ERR_DUPLICATE_TABLE, table.TableName);
                    return false;
                }
                _lootTables.Add(table.TableName, table);
            }
            return ValidateTables(tables); //Done after to account for race condition w/ Circular Ref check
        }

        return false;
    }

    /*
     * Tries to return a loaded loot table
     */
    public bool TryGetLootTable(string tableName, out LootTable lootTable)
    {
        return _lootTables.TryGetValue(tableName, out lootTable);
    }
}