using System;
using System.Collections.Generic;
using System.IO;

namespace main.LootSystem;

public class LootSystem
{
    #region Constant strings

    #region Commands

    private const string CMD_EXIT = "exit";
    private const string CMD_HELP = "help";
    private const string CMD_RELOAD = "reload";
    
    #endregion

    #region Prompts

    private const string PROMPT_ENTER_TABLE = "Enter loot table file name (or press RETURN to use {0}):";

    #endregion

    #region Errors

    private const string ERR_COMMAND_SIZE = "Please type commands in the format: <TableName> <count>";
    private const string ERR_COMMAND_QUANTITY = "Quantity {0} not valid. Please use a whole number.";
    private const string ERR_COMMAND_TABLE_NOT_FOUND = "Table with name {0} not found";
    
    #endregion

    
    private const string MSG_TABLE_LOAD_SUCCESS = "Successfully loaded tables from {0}";
    private const string MSG_LOOT_DROP = "Dropped {0}";
    private const string PATH_DEFAULT_TABLE = "test_cases/loot_table.json";

    
    //TODO: Delete later
    private const string TEST_JSON_PATH = "C:\\Users\\jmfra\\OneDrive\\Desktop\\LootGen\\test_cases\\circular_reference.json";

    
    private const string HELP_TXT = @"Welcome to LootGenerator!

USAGE
1. Type commands in the format:
    <TableName> <count>
  For example:
    ""CurrencyTable 3""
  This will generate 3 drops of loot from the Currency table.
2. Type ""exit"" to close the program.
3. Type ""help"" to see this message again.
4. Type ""reload"" to load a new loot table file";

    #endregion
    
    private void DisplayInstructions()
    {
        Console.WriteLine(HELP_TXT);
    }
    
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

    private bool PromptLoadLootTables(bool testMode)
    {
        bool tablesParsed = false;
        while(!tablesParsed){
            string jsonPath = testMode ? TEST_JSON_PATH : PromptJSONPath();
            if (LootTableManager.Instance.TryLoadLootTables(jsonPath))
            {
                tablesParsed = true;
                Console.WriteLine(MSG_TABLE_LOAD_SUCCESS, jsonPath);
            }
            else if (testMode)
            {
                break;
            }
        }

        return tablesParsed;
    }

    public void Execute(bool testMode)
    {
        //Load loot data tables
        if (!PromptLoadLootTables(testMode))
        {
            Console.WriteLine("Failed to load loot tables");
            return;
        }

        //Display instructions
        DisplayInstructions();
        
        //Execute main loop
        var complete = false;
        while (!complete)
        {
            complete = ExecuteStep();
        }
    }
    
    //Gather user input and attempt to process instructions
    //Return true when program complete
    private bool ExecuteStep()
    {
        var input = Console.ReadLine();

        //Check for common commands
        switch (input)
        {
            case CMD_EXIT:
                return true;
            case CMD_HELP:
                Console.WriteLine(HELP_TXT);
                return false;
            case CMD_RELOAD:
                PromptLoadLootTables(false);
                DisplayInstructions();
                return false;
        }

        if (TryParseTableCommand(input, out var tableName, out var count))
        {
            if(LootTableManager.Instance.TryGetLootTable(tableName, out var table)) 
            {
                var lootCollection = table.GenerateLoot(count);
                foreach (var loot in lootCollection)
                {
                    Console.WriteLine(MSG_LOOT_DROP, loot);
                }
            }
            else
            {
                Console.WriteLine(string.Format(ERR_COMMAND_TABLE_NOT_FOUND, tableName));
            }
        }
        
        return false;
    }

    private bool TryParseTableCommand(string input, out string tableName, out int quantity)
    {
        tableName = "";
        quantity = 0;
        
        // Try parsing the string as a command
        string[] subs = input.Split(' ');
        if (subs.Length != 2)
        {
            Console.WriteLine(ERR_COMMAND_SIZE);
            return false;
        }

        tableName = subs[0];
        string countStr = subs[1];

        if (!int.TryParse(countStr, out quantity))
        {
            Console.WriteLine(ERR_COMMAND_QUANTITY);
            return false;
        }

        return true;
    }
}