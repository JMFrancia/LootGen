using System;
using System.Collections.Generic;

namespace main.LootSystem;

public class LootSystem
{
    private LootTableParser _lootTableParser;
    private List<LootTable> _lootTableData;

    private const string CMD_EXIT = "exit";
    private const string CMD_HELP = "help";
    private const string CMD_RELOAD = "reload";

    private const string ERR_COMMAND_SIZE = "Please type commands in the format: <TableName> <count>";
    private const string ERR_COMMAND_QUANTITY = "Quantity {0} not valid. Please use a whole number.";
    private const string ERR_COMMAND_TABLE_NOT_FOUND = "Table with name {0} not found";

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

    public LootSystem()
    {
        _lootTableParser = new LootTableParser();
    }

    private void ParseLootTableData()
    {
        _lootTableData = _lootTableParser.ParseLootTables();
    }

    private void DisplayInstructions()
    {
        Console.WriteLine(HELP_TXT);
    }

    public void Execute()
    {
        //Load loot data tables
        ParseLootTableData();
        
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
                ParseLootTableData();
                DisplayInstructions();
                return false;
        }
        
        if (!TryParseTableCommand(input, out string lootTableStr, out int count))
        {
            return false;
        }

        GenerateLootFromData(_lootTableData, lootTableStr, count);
        return false;
    }

    private void GenerateLootFromData(List<LootTable> lootTables, string tableName, int quantity)
    {
        bool foundTable = false;
        foreach (var lootTable in lootTables)
        {
            if (lootTable.TableName == tableName)
            {
                lootTable.GenerateLoot(quantity);
                return;
            }
        }

        Console.WriteLine(String.Format(ERR_COMMAND_TABLE_NOT_FOUND, tableName));
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