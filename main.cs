using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using main.LootSystem;

public class Program
{
    public static void Main(string[] args)
    {
        var lootSystem = new LootSystem();
        lootSystem.Execute();
    }
}