using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

// Prompt for directory path
Console.WriteLine("Enter the core_2 directory path. e.g. SomethingSomething\\Soulash 2\\data\\mods\\core_2");
string rootDirectory = Console.ReadLine();

if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory))
{
    Console.WriteLine("Invalid directory path.");
    return;
}

Console.WriteLine("Enter the output JS file path:");
string outputPath = Console.ReadLine();

try
{
    JsonObject mergedJson = MergeJsonFilesRecursively(rootDirectory, rootDirectory);

    // Wrap JSON in a JavaScript variable
    string jsVariableName = "mergedJsonData";
    string jsContent = $"const {jsVariableName} = {mergedJson.ToJsonString(new JsonSerializerOptions { WriteIndented = true })};";

    File.WriteAllText(outputPath, jsContent);
    Console.WriteLine($"Merged JSON written as a JS variable to: {outputPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

// Method to merge JSON files recursively
JsonObject MergeJsonFilesRecursively(string directory, string rootDirectory)
{
    var result = new JsonObject();

    // Process JSON files in the current directory
    foreach (string filePath in Directory.GetFiles(directory, "*.json"))
    {
        string relativePath = Path.GetRelativePath(rootDirectory, filePath);
        string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar);

        AddFileToJson(result, pathParts, File.ReadAllText(filePath));
    }

    // Recursively process subdirectories
    foreach (string subDirectory in Directory.GetDirectories(directory))
    {
        string folderName = Path.GetFileName(subDirectory); // Use folder name, not full relative path

        if (!result.ContainsKey(folderName))
        {
            result[folderName] = new JsonObject(); // Create a new object for the folder
        }

        JsonObject nestedResult = (JsonObject)result[folderName]!;
        JsonObject subDirectoryResult = MergeJsonFilesRecursively(subDirectory, rootDirectory);

        // Merge results without duplicating the folder name
        foreach (var property in subDirectoryResult)
        {
            // Clone the node to avoid the "already has a parent" issue
            nestedResult[property.Key] = property.Value!.Deserialize<JsonNode>();
        }
    }

    return result;
}

// Method to add a file to the JSON object
void AddFileToJson(JsonObject root, string[] pathParts, string fileContent)
{
    JsonObject current = root;
    for (int i = 0; i < pathParts.Length - 1; i++)
    {
        if (!current.TryGetPropertyValue(pathParts[i], out JsonNode? nestedNode))
        {
            nestedNode = new JsonObject();
            current[pathParts[i]] = nestedNode;
        }

        current = (JsonObject)nestedNode!;
    }

    string fileName = pathParts[^1];
    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

    try
    {
        JsonNode? jsonContent = JsonNode.Parse(fileContent);
        current[fileNameWithoutExtension] = jsonContent;
    }
    catch
    {
        Console.WriteLine($"Failed to parse JSON in file: {fileName}");
    }
}
