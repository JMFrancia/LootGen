// A single loot Entry in a LootTable

using System;
using System.Collections.Generic;

public class LootEntry
{
  private const string ERR_ENTRYTYPE = "Invalid table entry type {0}";
  
  // Display name of this Entry
  public string EntryName { get; set; }
  
  private enum LootEntryType
  {
    Item,
    Table
  }

  public string EntryType
  {
    set => SetEntryType(value);
  }
    
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
  
  public ValidationResult ValidateEntry()
  {
    if (SelectionWeight <= 0.0f)
    {
      return ValidationResult.Error("Invalid SelectionWeight value: " + SelectionWeight + " in EntryName: " + EntryName);
    }

    // TODO: guard against other kinds of invalid property values and add more test cases
    return ValidationResult.Valid();
  }

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

  private Loot GenerateItemLoot()
  {
    return new Loot(EntryName, GetRandomDropAmount());
  }

  private List<Loot> GenerateTableLoot(string tableName)
  {
    if(!LootTableManager.Instance.TryGetLootTable(tableName, out var table))
      return null;

    return table.GenerateLoot(GetRandomDropAmount());
  }

  private int GetRandomDropAmount()
  {
    Random rnd = new Random();
    return rnd.Next(MinDrops, MaxDrops);
  }
  
  private void SetEntryType(string entryType)
  {
    if (!Enum.TryParse(entryType, out _entryType))
    {
      Console.WriteLine(ERR_ENTRYTYPE, entryType);
    }
  }
}