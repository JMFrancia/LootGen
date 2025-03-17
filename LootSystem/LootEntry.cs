// A single loot Entry in a LootTable

using System;
using System.Collections.Generic;

public class LootEntry
{
  private const string ERR_ENTRYTYPE = "Invalid table entry type {0}";
  
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
    get => _entryType.ToString();
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
  
  //Validates the entry
  public ValidationResult ValidateEntry()
  {
    if (SelectionWeight <= 0.0f)
    {
      return ValidationResult.Error("Invalid SelectionWeight value: " + SelectionWeight + " in EntryName: " + EntryName);
    }

    // TODO: guard against other kinds of invalid property values and add more test cases
    return ValidationResult.Valid();
  }

  //Generates and returns a list of Loot based on the EntryType
  public List<Loot> GenerateLoot()
  {
    List<Loot> result = new List<Loot>();
    switch (_entryType)
    {
      case LootEntryType.Item:
        result.Add(GenerateItemLoot());
        break;
      case LootEntryType.Table:
        result.AddRange(GenerateTableLoot(EntryName));
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
    if (!Enum.TryParse(entryType, out _entryType))
    {
      Console.WriteLine(ERR_ENTRYTYPE, entryType);
    }
  }
}