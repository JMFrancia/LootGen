using System;
using System.Collections.Generic;
using System.Linq;

// A collection of loot entries for generating loot
public class LootTable
{
  private const string ERR_TABLETYPE = "Invalid table type {0}";
  
  private enum LootTableType
  {
    Random,
    UniqueRandom
  }

  public string TableType
  {
    set => SetTableType(value);
  }

  private void SetTableType(string tableType)
  {
    if (!Enum.TryParse(tableType, out _tableType))
    {
        Console.WriteLine(ERR_TABLETYPE, tableType);
    }
  }

  private LootTableType _tableType;

  // The key used by TableEntries to look up this LootTable
  public string TableName { get; set; }

  // The loot entries that can be found in this table
  public List<LootEntry> TableEntryCollection { get; set; }

  public ValidationResult ValidateTable()
  {
    foreach (LootEntry entry in TableEntryCollection)
    {
      ValidationResult entryResult = entry.ValidateEntry();
      if (!entryResult.IsValid)
      {
        return entryResult;
      }
    }
    return ValidationResult.Valid();
  }

  // Generates loot from this table a given number of times
  public List<Loot> GenerateLoot(int count)
  {
    var combinedLoot = new Dictionary<string, Loot>();
    
    if (_tableType == LootTableType.Random)
    {
      var rnd = new Random();
      for (int n = 0; n < count; n++)
      {
        var randomLootEntry = TableEntryCollection[rnd.Next(TableEntryCollection.Count)].GenerateLoot();
        foreach (var loot in randomLootEntry)
        {
          if (combinedLoot.ContainsKey(loot.Name))
          {
            combinedLoot[loot.Name].TryMerge(loot);
          }
          else
          {
            combinedLoot[loot.Name] = loot;
          }
        }
      }

    }

    return combinedLoot.Values.ToList();
  }
  
}