using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

//Manages the process of parsing a loot table from a JSON, including user I/O
//TODO: Move all user IO to separate class, focus exclusively on parsing and storing Loot Tables
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

    private bool testMode = true;

    private Dictionary<string, LootTable> _lootTables;


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
        foreach (LootTable table in lootTables)
        {
            ValidationResult tableResult = table.ValidateTable();
            if (!tableResult.IsValid)
            {
                Console.WriteLine(string.Format(ERR_PARSE_TABLE, table.TableName, tableResult.ErrorMessage));
                return false;
            }

            //TODO: Move this to be part of loot table validation
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