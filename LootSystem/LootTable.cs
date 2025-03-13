using System;
using System.Collections.Generic;

// A collection of loot entries for generating loot
public class LootTable
{
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
  public void GenerateLoot(int count)
  {
    Console.WriteLine("Drop " + count + " from " + TableName + ".");

    // FIXME
  }
}