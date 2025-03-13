using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

public class Program
{
    static List<LootTable> parseLootTables()
    {
        Console.WriteLine("Enter loot table file name (or press RETURN to use loot_table.json):");
        string jsonPath = Console.ReadLine();
        if (jsonPath == "")
        {
            jsonPath = "test_cases/loot_table.json";
        }

        string jsonString = "";
        List<LootTable> lootTables = new List<LootTable>();
        try
        {
            jsonString = File.ReadAllText(jsonPath);
        }
        catch
        {
            Console.WriteLine("Error reading file " + jsonPath + ".");
            return null;
        }

        try
        {
            lootTables = JsonSerializer.Deserialize<List<LootTable>>(jsonString);
        }
        catch
        {
            Console.WriteLine("Error parsing data file " + jsonPath + ". JSON malformed.");
            return null;
        }

        foreach (LootTable table in lootTables)
        {
            ValidationResult tableResult = table.ValidateTable();
            if (!tableResult.IsValid)
            {
                Console.WriteLine("Error parsing table in " + jsonPath + ": " + tableResult.ErrorMessage);
                return null;
            }
        }

        return lootTables;
    }

    // displays helpful text to stdout
    static void printHelp()
    {
        Console.WriteLine(@"Welcome to LootGenerator!

USAGE
1. Type commands in the format:
    <TableName> <count>
  For example:
    ""CurrencyTable 3""
  This will generate 3 drops of loot from the Currency table.
2. Type ""exit"" to close the program.
3. Type ""help"" to see this message again.
4. Type ""reload"" to load a new loot table file");
    }

    public static void Main(string[] args)
    {
        // Parse the default loot tables
        List<LootTable> lootTables = parseLootTables();
        if (lootTables == null)
        {
            return;
        }

        // Give the user some instructions
        printHelp();

        // User input loop
        while (true)
        {
            string input = Console.ReadLine();

            // Special cases for command input
            if (input == "exit")
            {
                return;
            }
            if (input == "help")
            {
                printHelp();
                continue;
            }
            if (input == "reload")
            {
                lootTables = parseLootTables();
                if (lootTables == null)
                {
                    return;
                }
                printHelp();
                continue;
            }

            // Try parsing the string as a command
            string[] subs = input.Split(' ');
            string lootTableStr = subs[0];
            string countStr = subs[1];

            // Try parse count
            int count = int.Parse(countStr);

            bool foundTable = false;
            foreach (LootTable lootTable in lootTables)
            {
                if (lootTable.TableName == lootTableStr)
                {
                    lootTable.GenerateLoot(count);
                    foundTable = true;
                }
            }

            if (!foundTable)
            {
                Console.WriteLine("No such table: " + lootTableStr);
            }
        }
    }
}