// A single loot Entry in a LootTable
public class LootEntry
{
  // Display name of this Entry
  public string EntryName { get; set; }

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
}