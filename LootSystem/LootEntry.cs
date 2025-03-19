// A single loot Entry in a LootTable

using System;
using System.Collections.Generic;

public class LootEntry
{
  private const string ERR_ENTRY_TYPE = "Invalid loot entry type \"{0}\"";
  private const string ERR_SELECTION_WEIGHT = "Invalid SelectionWeight value: {0} in EntryName: {1}";
  private const string ERR_DROP_RATE = "Invalid Min/Max drop rates in loot entry {0}: both values must be positive and min rate ({1}) must be lower than max rate ({2})";

  // Display name of this Entry
  public string EntryName { get; set; }
  
  public enum LootEntryType
  {
    Item,
    Table
  }

  //TODO: Do better
  public string EntryType
  {
    set => SetEntryType(value);
  }

  //TODO: Do better
  public LootEntryType GetEntryType => _entryType;
  
  private LootEntryType _entryType;

  // Minimum drop count for this loot entry
  // Should be greater than zero and less than or equal to MaxDrops
  public int MinDrops { get; set; }

  // Maximum drop count for this loot entry
  // Should be greater than or equal to MinDrops
  public int MaxDrops { get; set; }

  // Selection weight for this entry, in arbitrary units
  // Should be greater than 0.0
  public float SelectionWeight { get; set; }

  private bool _entryTypeValid = true;
  
  //Validates the entry
  public ValidationResult ValidateEntry()
  {
    //Validate that entry type is valid (though error probably already thrown at this point from SetEntryType())
    if (!_entryTypeValid)
    {
      return ValidationResult.Error(string.Format("Invalid EntryType value for Entry: {0}", EntryName));
    }

    //Validate selection weight
    if (SelectionWeight <= 0.0f)
    {
      return ValidationResult.Error(string.Format(ERR_SELECTION_WEIGHT, SelectionWeight, EntryName));
    }

    //Validate drop rate
    if (MinDrops < 0 ||
        MaxDrops < 0 ||
        MaxDrops < MinDrops)
    {
      return ValidationResult.Error(string.Format(ERR_DROP_RATE, EntryName, MinDrops, MaxDrops));
    }
    
    return ValidationResult.Valid();
  }

  //Generates and returns a list of Loot based on the EntryType
  public List<Loot> GenerateLoot()
  {
    List<Loot> result = new List<Loot>();
    switch (_entryType)
    {
      case LootEntryType.Item:
        var loot = GenerateItemLoot();
        Console.WriteLine("DEBUG: Adding loot item " + loot);
        result.Add(loot);
        break;
      case LootEntryType.Table:
        var lootList = GenerateTableLoot(EntryName);
        var msg = "Debug: Adding loot from table " + EntryName + ": ";
        foreach (var thisLoot in lootList)
        {
          msg += thisLoot + ", ";
        }
        Console.WriteLine(msg);
        result.AddRange(lootList);
        break;
    }
    return result;
  }

  //Generates loot from this item entry
  private Loot GenerateItemLoot()
  {
    return new Loot(EntryName, GetRandomDropAmount());
  }

  //Pulls table of specified name and generates loot from it
  private List<Loot> GenerateTableLoot(string tableName)
  {
    if(!LootTableManager.Instance.TryGetLootTable(tableName, out var table))
      return null;

    return table.GenerateLoot(GetRandomDropAmount());
  }

  //Returns a random int between MinDrops and MaxDrops
  private int GetRandomDropAmount()
  {
    Random rnd = new Random();
    return rnd.Next(MinDrops, MaxDrops);
  }
  
  //Sets the entry type enum from deserialized string
  private void SetEntryType(string entryType)
  {
    _entryTypeValid = Enum.TryParse(entryType, out _entryType);
    if (!_entryTypeValid)
    {
      Console.WriteLine(ERR_ENTRY_TYPE, entryType);
    }
  }
}