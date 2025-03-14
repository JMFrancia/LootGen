using System;
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
 
 

  //Try to parse the loot tables
  private bool TryParseLootTables(string jsonPath, out List<LootTable> lootTables) 
  {
    lootTables = null;
    var jsonString = "";
    try
    {
        jsonString = File.ReadAllText(jsonPath);
    }
    catch(Exception e)
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
  private bool ValidateTables(List<LootTable> lootTables) {
    foreach (LootTable table in lootTables)
    {
        ValidationResult tableResult = table.ValidateTable();
        if (!tableResult.IsValid)
        {
            //TODO: FIX THIS
          Console.WriteLine(string.Format(ERR_PARSE_TABLE, "BLAH BLAH", tableResult.ErrorMessage));
          return false;
        }
    }
    return true;
  }

  public bool TryLoadLootTables(string JSONPath)
  {
      if (TryParseLootTables(JSONPath, out var tables)  &&
          ValidateTables(tables))
      {
          _lootTables = new Dictionary<string, LootTable>();
          foreach (var table in tables)
          {
              _lootTables.Add(table.TableName, table);
          }
          return true;
      }
      return false;
  }

  public bool TryGetLootTable(string tableName, out LootTable lootTable)
  {
      return _lootTables.TryGetValue(tableName, out lootTable);
  }
  
}