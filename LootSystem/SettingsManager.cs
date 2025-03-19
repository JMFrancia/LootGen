using System.IO;

namespace LootSystem;

public static class SettingsManager
{
    public const int UNIQUE_RANDOM_COOLDOWN = 3;

    public static bool DisplayDebugMessages = false;

    //For testing on my local machine to save time with prompts
    public const bool SKIP_PROMPTS_LOCAL_ONLY = false;
    private const string LOCAL_TEST_JSON_PATH =
        @"C:\Users\jmfra\OneDrive\Desktop\LootGen\test_cases\";
    private const string LOCAL_TEST_JSON = "large_unique_random.json";

    public static string LocalTestJsonPath => Path.Combine(LOCAL_TEST_JSON_PATH, LOCAL_TEST_JSON);
}