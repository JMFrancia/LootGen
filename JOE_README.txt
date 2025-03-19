Thanks for taking the time to review my take-home test!

A few notes on coding choices I made: 

- I moved the code that was originally in the Main.cs file into LootIO (for main loop) and LootTableManager (for parsing the loot tables). I did this because I have a personal dislike of having lots of code in the main file (I'm not hardline about it, but that's how I decided to do it this time). 

- I added a "debug" command that can be used to toggle whether or not debug messages are displayed. It's off by default. 

- To modify the UniqueRandom cooldown, change the SettingsManager.UNIQUE_RANDOM_COOLDOWN value.

- Since I added a lot of test case JSON, I renamed "loot_table.json" to "valid_loot_table.json" for clarity

- I chose to store the cooldown rate of UniqueRandom table entries in the UniqueRandom table rather than in the entries themselves. My thinking was that a future optimization may be to have multiple tables refer to the same entries, so wouldn't want a table drop from one to affect cooldown on another.

- I really, desperately wanted to have two separate classes for LootTable, one for each mode, with a shared abstract base class. That feels so much better to me architecture-wise. But since we were already deserializing into the provided LootTable class, I didn't want to create the deserialized LootTable object, then generate another one after that and discard the original, without a convesration with the team. Plus it added more time/scope. But please do know this bugged me a lot! LootTable really should have been 3 classes (base, random, uniquerandom). 


Some other things I would have done given more time: 

1. Test Runner (since I kept running tests on the same JSONs over and over I would have loved to automate that)

2. Move all string constants for a single place for organization / localization

3. Better detail on error messages for circular table entry references, such printing out entire path of a detected cycle (IE TreasureChestTable -> EquipmentTable -> CurrencyTable -> TreasureChestTable)



Thank you for your consideration!