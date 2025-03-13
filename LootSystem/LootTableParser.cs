using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

//Manages the process of parsing a loot table from a JSON, including user I/O
public class LootTableParser {

  private const string PROMPT_ENTER_TABLE = "Enter loot table file name (or press RETURN to use {0}):";
  private const string PATH_DEFAULT_TABLE = "test_cases/loot_table.json";

  private const string ERR_READ_FILE = "Error reading file {0}.";
  private const string ERR_PARSE_DATA = "Error parsing data file {0}. JSON malformed.";
  private const string ERR_PARSE_TABLE = "Error parsing table in {0}: {1}";

  //Request JSON Path from user
  private string PromptJSONPath() 
  {
    var tablePrompt = string.Format(PROMPT_ENTER_TABLE, Path.GetFileName(PATH_DEFAULT_TABLE));
    
    Console.WriteLine(tablePrompt);
    string jsonPath = Console.ReadLine();
    if (string.IsNullOrEmpty(jsonPath))
    {
        jsonPath = PATH_DEFAULT_TABLE;
    }
    return jsonPath;
  }

  //Try to parse the loot tables
  private bool TryParseLootTables(string jsonPath, out List<LootTable> lootTables) 
  {
    lootTables = null;
    var jsonString = "";
    try
    {
        jsonString = File.ReadAllText(jsonPath);
    }
    catch
    {
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
  private bool ValidateTables(List<LootTable> lootTables) {
    foreach (LootTable table in lootTables)
    {
        ValidationResult tableResult = table.ValidateTable();
        if (!tableResult.IsValid)
        {
          Console.WriteLine(string.Format(ERR_PARSE_TABLE, jsonPath, tableResult.ErrorMessage)));
          return false;
        }
    }
    return true;
  }

  //Prompts user to enter JSON path, parses and returns loot tables accordingly
  public List<LootTable> parseLootTables()
  {
      List<LootTable> lootTables;

      //Doing it this way gives users infinite chances to try and sort out table issues
      bool tablesParsed = false;
      while(!tablesParsed){
        string jsonPath = PromptJSONPath();
        if(TryParseLootTables(jsonPath, out lootTables) &&
           ValidateTables(lootTables)) 
        {
          tablesParsed = true;  
        }
      }

      return lootTables;
  }
  
}