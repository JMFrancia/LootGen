using System;
using System.Collections.Generic;
using System.Linq;

namespace LootSystem;

/*
 * A collection of loot entries for generating loot
 */
public class LootTable
{
  private const string ERR_TABLE_EMPTY = "Table {0} is empty";
  private const string ERR_TABLE_NAME_MISSING = "Table is missing a name";
  private const string ERR_TABLETYPE = "Invalid table type for table {0}";
  private const string ERR_TABLETYPE_SET = "Invalid table type \"{0}\"";
  private const string ERR_DUPLICATE_TABLE = "Duplicate entry {0} found in table {1}";
  private const string ERR_ENTRY_NAME_MISSING = "LootEntry is missing a name in table {0}";
  
  private enum LootTableType
  {
    Random,
    UniqueRandom
  }

  //Deserializes TableType string into enum
  public string TableType
  {
    set => SetTableType(value);
  }

  // The key used by TableEntries to look up this LootTable
  public string TableName { get; set; }

  // The loot entries that can be found in this table
  public List<LootEntry> TableEntryCollection { get; set; }
  
  private void SetTableType(string tableType)
  {
    _tableTypeValid = Enum.TryParse(tableType, out _tableType);
    if (!_tableTypeValid)
    {
      Console.WriteLine(ERR_TABLETYPE_SET, tableType);
    }
  }

  private Dictionary<LootEntry, int> _uniqueRandomEntryCooldownDict = new();

  private LootTableType _tableType;
  private bool _tableTypeValid = true;

  /*
   * Validates table
   */
  public ValidationResult ValidateTable()
  {
    //Check that the table has a name
    if (string.IsNullOrEmpty(TableName))
    {
      return ValidationResult.Error(ERR_TABLE_NAME_MISSING);
    }

    //Check if the table type is valid
    if (!_tableTypeValid)
    {
      return ValidationResult.Error(string.Format(ERR_TABLETYPE, TableName));
    }

    //Check that the table is not empty
    if (TableEntryCollection.Count == 0)
    {
      return ValidationResult.Error(string.Format(ERR_TABLE_EMPTY, TableName));
    }

    //Validate individual entries in the table
    var cachedEntries = new HashSet<string>();
    foreach (LootEntry entry in TableEntryCollection)
    {
      //Check if missing a name (doing this in Table rather than Entry so that we can give more detailed error msg)
      if (string.IsNullOrEmpty(entry.EntryName))
      {
        return ValidationResult.Error(string.Format(ERR_ENTRY_NAME_MISSING, TableName));
      }
      
      //Check if entry is a duplicate
      if (cachedEntries.Contains(entry.EntryName))
      {
        return ValidationResult.Error(string.Format(ERR_DUPLICATE_TABLE, entry.EntryName, TableName));
      }
      else
      {
        cachedEntries.Add(entry.EntryName);
      }

      //Run entry-level validation
      ValidationResult entryResult = entry.ValidateEntry();
      if (!entryResult.IsValid)
      {
        return entryResult;
      }
    }
    
    return ValidationResult.Valid();
  }

  //Returns a list of all Table-type LootEntries (useful for checking circular refs)
  public List<LootTable> GetAllTableTypeEntries()
  {
    var result = new List<LootTable>();
    for (int n = 0; n < TableEntryCollection.Count; n++)
    {
      if (TableEntryCollection[n].GetEntryType == LootEntry.LootEntryType.Table &&
          LootTableManager.Instance.TryGetLootTable(TableEntryCollection[n].EntryName, out var table))
      {
        result.Add(table);
      }
    }

    return result;
  }
  
  // Generates loot from this table a given number of times
  public List<Loot> GenerateLoot(int count)
  {
    var combinedLoot = new Dictionary<string, Loot>();
    DisplayDebugMessage($"Generating {count} loot drops from {TableName}");
    for (int n = 0; n < count; n++)
    {
      var lootList = _tableType switch
      {
        LootTableType.Random => GetWeightedRandomLoot(),
        LootTableType.UniqueRandom => GetWeightedUniqueRandomLoot(),
        _ => new List<Loot>() //This will never happen, but its good to have a default just in case
      };
      
      //Merge into like loot if any exists
      foreach(var loot in lootList) {
        if (combinedLoot.ContainsKey(loot.Name))
        {
          DisplayDebugMessage($"Merging {loot} into {combinedLoot[loot.Name]}");
          var tmp = combinedLoot[loot.Name];
          if (tmp.TryMerge(loot))
          {
            combinedLoot[loot.Name] = tmp;
          }
        }
        else
        {
          combinedLoot[loot.Name] = loot;
        }
      }
    }

    return combinedLoot.Values.ToList();
  }
  
  //Generates and returns loot based on weighted random
  private List<Loot> GetWeightedRandomLoot()
  {
    //If only one entry available, just return that
    if (TableEntryCollection.Count == 1)
    {
      return TableEntryCollection[0].GenerateLoot();
    }
    
    return GetWeightedRandomEntry(TableEntryCollection).GenerateLoot();
  }
  
  //Generates and returns loot based on Unique Random
  private List<Loot> GetWeightedUniqueRandomLoot()
  {
    //Update cooldowns
    UpdateUniqueRandomCooldowns();
    
    //If no UniqueRandom entries available, force expire cooldown
    if (!UniqueRandomEntryAvailable())
    {
      ForceExpireCooldown();
    }
    var collection = GetUniqueRandomEntryCollection();

    //If only one UniqueRandom entry available, just return that
    if (collection.Count == 1)
    {
      DisplayDebugMessage(GetCollectionStatusString());
      _uniqueRandomEntryCooldownDict.Add(collection[0], SettingsManager.UNIQUE_RANDOM_COOLDOWN);
      return collection[0].GenerateLoot();
    }

    //Otherwise, select weighted random from list of avialable UniqueRandom entries
    var result = GetWeightedRandomEntry(collection);
    _uniqueRandomEntryCooldownDict.Add(result, SettingsManager.UNIQUE_RANDOM_COOLDOWN);
    return result.GenerateLoot();
  }

  //Returns a random entry based on selection weight
  private LootEntry GetWeightedRandomEntry(List<LootEntry> collection)
  {
    DisplayDebugMessage(GetCollectionStatusString());
    
    float totalSelectionWeight = collection.Sum(entry => entry.SelectionWeight);
    float target = new Random().NextSingle() * totalSelectionWeight;
    DisplayDebugMessage($"Random target weight: {target}");
    float runningSum = 0;
    for (int n = 0; n < collection.Count; n++)
    {
      runningSum += collection[n].SelectionWeight;
      if (target < runningSum)
      {
        DisplayDebugMessage($"Returning {collection[n].EntryName}");
        return collection[n];
      }
    }
    DisplayDebugMessage($"Returning {collection.Last().EntryName}");
    return collection.Last();
  }

  private string GetCollectionStatusString()
  {
    var result = $"Table: {TableName}\nType:{_tableType}\nSize:{TableEntryCollection.Count}\n";
    float runningTotal = 0;
    for (int n = 0; n < TableEntryCollection.Count; n++)
    {
      var entry = TableEntryCollection[n];
      runningTotal += entry.SelectionWeight;
      result += $"{n + 1}: {entry.EntryName}, selection weight: {entry.SelectionWeight}, Weight Target Range: {runningTotal - entry.SelectionWeight} -> {runningTotal}";

      if (_tableType == LootTableType.UniqueRandom && 
          _uniqueRandomEntryCooldownDict.ContainsKey(entry))
      {
        result += $", cooldown: {_uniqueRandomEntryCooldownDict[entry]}\n";
      }
      else
      {
        result += "\n";
      }
    }

    return result;
  }

  //Returns true if a UniqueRandom loot entry is available to be generated
  private bool UniqueRandomEntryAvailable()
  {
    return TableEntryCollection.Count > _uniqueRandomEntryCooldownDict.Count;
  }

  //Accelerate cooldowns until at least one loot entry type is avilable
  private void ForceExpireCooldown()
  {
    DisplayDebugMessage("Forcing cooldown expiration");
    while (!UniqueRandomEntryAvailable())
    {
      UpdateUniqueRandomCooldowns();
    }
  }
  
  //Returns the collection of entries that are NOT in cool down (for UniqueRandom mode)
  private List<LootEntry> GetUniqueRandomEntryCollection()
  {
    return TableEntryCollection.Where(entry => !_uniqueRandomEntryCooldownDict.ContainsKey(entry)).ToList();
  }
  
  //Updates cooldown counter for all loot entry items
  private void UpdateUniqueRandomCooldowns()
  {
    foreach (var entryKey in _uniqueRandomEntryCooldownDict.Keys)
    {
      _uniqueRandomEntryCooldownDict[entryKey]--;
      if (_uniqueRandomEntryCooldownDict[entryKey] <= 0)
      {
        _uniqueRandomEntryCooldownDict.Remove(entryKey);
      }
    }
  }
  
  //TODO: Merge this method and the duplicate one from Loot Entry, store elsewhere
  private void DisplayDebugMessage(string msg)
  {
    if (SettingsManager.DisplayDebugMessages)
    {
      Console.WriteLine($"DEBUG: {msg}");
    }
  }
  
}