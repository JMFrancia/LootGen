using System;
using System.Collections.Generic;
using System.Linq;

// A collection of loot entries for generating loot
public class LootTable
{
  private const int UNIQUE_RANDOM_COOLDOWN = 3;
  
  private const string ERR_TABLETYPE = "Invalid table type {0}";
  
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

  private void SetTableType(string tableType)
  {
    if (!Enum.TryParse(tableType, out _tableType))
    {
        Console.WriteLine(ERR_TABLETYPE, tableType);
    }
  }

  private Dictionary<LootEntry, int> _uniqueRandomEntryCooldownDict = new();

  private LootTableType _tableType;

  // The key used by TableEntries to look up this LootTable
  public string TableName { get; set; }

  // The loot entries that can be found in this table
  public List<LootEntry> TableEntryCollection { get; set; }

  public ValidationResult ValidateTable()
  {
    var cachedEntries = new HashSet<string>();
    foreach (LootEntry entry in TableEntryCollection)
    {
      //Check if missing a name (doing this in Table rather than Entry so that we can give more detailed error msg)
      if (string.IsNullOrEmpty(entry.EntryName))
      {
        return ValidationResult.Error(string.Format("LootEntry is missing a name in table {0}", TableName));
      }
      
      //Check if entry is a duplicate
      if (cachedEntries.Contains(entry.EntryName))
      {
        return ValidationResult.Error(string.Format("Duplicate entry {0} found in table {1}", entry.EntryName, TableName));
      }
      else
      {
        cachedEntries.Add(entry.EntryName);
      }

      //Validate entry
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

  //Generates and returns loot based on weighted random
  private List<Loot> GetWeightedRandomLoot()
  {
    float totalSelectionWeight = TableEntryCollection.Sum(entry => entry.SelectionWeight);
    var target = new Random().NextSingle() * totalSelectionWeight;
    float runningSum = 0;
    
    for (int n = 0; n < TableEntryCollection.Count; n++)
    {
      runningSum += TableEntryCollection[n].SelectionWeight;
      if (target < runningSum)
      {
        return TableEntryCollection[n].GenerateLoot();
      }
    }

    return TableEntryCollection.Last().GenerateLoot();
  }

  //Returns true if a UniqueRandom loot entry is available to be generated
  private bool UniqueRandomEntryAvailable()
  {
    return TableEntryCollection.Count > _uniqueRandomEntryCooldownDict.Count;
  }

  //Accelerate cooldowns until at least one loot entry type is avilable
  private void ForceExpireCooldown()
  {
    while (!UniqueRandomEntryAvailable())
    {
      UpdateUniqueRandomCooldowns();
    }
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
    var collection = GetUniqueRandomTableEntryCollection();

    //If only one UniqueRandom entry available, just return that
    if (collection.Count == 1)
    {
      return collection[0].GenerateLoot();
    }

    //Otherwise, select weighted random from list of avialable UniqueRandom entries
    float totalSelectionWeight = collection.Sum(entry => entry.SelectionWeight);
    float target = new Random().NextSingle() * totalSelectionWeight;
    float runningSum = 0;
    for (int n = 0; n < collection.Count; n++)
    {
      runningSum += collection[n].SelectionWeight;
      if (target < runningSum)
      {
        _uniqueRandomEntryCooldownDict.Add(collection[n], UNIQUE_RANDOM_COOLDOWN);
        return collection[n].GenerateLoot();
      }
    }

    _uniqueRandomEntryCooldownDict.Add(collection.Last(), UNIQUE_RANDOM_COOLDOWN);
    return collection.Last().GenerateLoot();
  }

  private List<LootEntry> GetUniqueRandomTableEntryCollection()
  {
    return TableEntryCollection.Where(entry => !_uniqueRandomEntryCooldownDict.ContainsKey(entry)).ToList();
  }

  private float GetTotalUniqueRandomSelectionWeight()
  {
    if (_uniqueRandomEntryCooldownDict.Count == TableEntryCollection.Count)
    {
      return 0f;
    }

    return GetUniqueRandomTableEntryCollection()
      .Sum(entry => entry.SelectionWeight);
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

  // Generates loot from this table a given number of times
  public List<Loot> GenerateLoot(int count)
  {
    var combinedLoot = new Dictionary<string, Loot>();
    Console.WriteLine("DEBUG: Generating " + count + " loot drops from " + TableName);
    for (int n = 0; n < count; n++)
    {
      var lootList = _tableType switch
      {
        LootTableType.Random => GetWeightedRandomLoot(),
        LootTableType.UniqueRandom => GetWeightedUniqueRandomLoot()
      };
      
      //Merge into like loot if any exists
      foreach(var loot in lootList) {
        if (combinedLoot.ContainsKey(loot.Name))
        {
          Console.WriteLine("DEBUG: Merging " + loot + " into " + combinedLoot[loot.Name]);
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
  
  
  
}