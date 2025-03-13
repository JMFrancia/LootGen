Tic Toc Studios Programming Test

Problem statement:
Write a console application that generates loot to stdout from a defined set 
of loot tables, per user request.

Every LootTable has the following attributes:
* TableType: Defines the behavior of how table entries are selected.
Valid table types are "Random" and "UniqueRandom".
* TableName: The name of the loot table. Used to select this table for loot 
generation.
* TableEntryCollection: A collection of "LootEntry"s. A loot table may contain 
[1..N] loot entries.

Every LootEntry has the following attributes:
* EntryType: Defines the behavior of the table entry: how loot is generated. 
Valid EntryTypes are "Item" and "Table".
* EntryName: The name of the item or table.
* MinDrops: An integer representing the minimum number of times the entry
will generate loot upon selection.
* MaxDrops: An integer representing the maximum number of times the entry
will generate loot upon selection.
* SelectionWeight: A real number (float or double) representing the weighted
chance of selection of the given entry.

When creating loot from a "Random" loot table, each LootEntry will be 
selected at random according to its SelectionWeight. If all the entries have 
the same weight, they each have the same chance of selection.  If entry A's 
weight is twice that of entry B, entry A is twice as likely to be selected as 
entry B.

Example set of drops from a "Random" loot table in loot_table.json:
>CurrencyTable 10
Dropped 977 silver
Dropped 2290 copper
Dropped 1 platinum
Dropped 18 gold

Creating loot from a "UniqueRandom" loot table works in a similar way, except 
that once a LootEntry is selected, that entry will not be selected 
again for a given set of drops. You can think of a set of drops as an 
invocation of the table to drop a certain number of items, whether directly 
via user input, or indirectly (via a "Table" EntryType, see details 
below).
So, concretely, that means a single invocation such as "EquipmentTable 4"
will produce each entry one time, with a random number of drops in between 
the selected entry's MinDrops and MaxDrops.  But a second invocation of
EquipmentTable 4 could produce the same four entries again.

Example drops from a "UniqueRandom" loot table in loot_table.json:
>EquipmentTable 3
Dropped 1 GoldRing
Dropped 1 IronSword
Dropped 2 SilverRing

>EquipmentTable 4
Dropped 1 GoldRing
Dropped 2 IronSword
Dropped 1 SilverRing
Dropped 3 WoodenShield

When generating drops for an "Item" EntryType, the desired item is 
generated in a random amount between MinDrops and MaxDrops, inclusive.
When generating drops for a "Table" EntryType, the specified table generates
loot a randomized number of times between MinDrops and MaxDrops, inclusive.

Along with this README document, your project includes the following resources:

* A sample data file (loot_table.json)
* A file describing example user I/O for the sample data (loot_example.txt)
* Some starter code

The provided starter code handles *some* but not *all* of the necessary json
file parsing and validation. Please modify the starter code as you see fit to
satisfy the requirements of this programming assignment.

Your application should initialize the table data, then prompt the user to 
enter the name of a loot table and a number of drops (space delimited), and 
generate the loot to stdout.  The user can do this any number of times, until 
they type "exit", at which point the program ends.

You must combine multiple drops of the same type into a single output.  For
example, if you rolled "CurrencyTable 2" and the results were
two gold drops, one of quantity 50 and one of quantity 20, the output should be
"Dropped 70 gold".  You should combine results even if the results span
multiple tables.

HOW YOUR WORK WILL BE EVALUATED:

When you submit your completed assignment, we will review your code carefully,
and we will test it against various json files. Our files differ from the one
provided but follow the schema outlined above. 

We will evaluate your code for:
 - functionality
 - error handling
 - code design, style, and readability
 - optimization

This test is meant to explore the edge cases found in real world work.
Use your imagination - how would this be used? What could go wrong?

If you would like to use a debugger outside of replit, you are welcome to develop your solution in an IDE of your choice, but remember when you are finished the solution needs to work in replit itself.

Good luck!