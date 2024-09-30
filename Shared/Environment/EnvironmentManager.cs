using System;
using System.Collections.Generic;
using System.IO;

public static class EnvironmentManager
{
    private static readonly Dictionary<string, string> _variables = new Dictionary<string, string>();
    private static readonly string filePath = "Environment\\config.txt"; // Define your local file path here

    // Public method to get a variable by name
    public static string GetVariable(string variableName)
    {
        if (!_variables.ContainsKey(variableName))
        {
            // Try to load from environment variables first
            string value = Environment.GetEnvironmentVariable(variableName);

            // If not found in environment variables, load from file
            if (string.IsNullOrEmpty(value))
            {
                LoadVariablesFromFile();
                if (_variables.ContainsKey(variableName))
                {
                    value = _variables[variableName];
                }
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"{variableName} is not set in environment variables or the local file.");
            }

            _variables[variableName] = value;
        }

        return _variables[variableName];
    }

    // Private method to load all variables from the local file
    private static void LoadVariablesFromFile()
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found at {filePath}");
        }

        foreach (var line in File.ReadAllLines(filePath))
        {
            if (!string.IsNullOrWhiteSpace(line) && line.Contains('='))
            {
                var keyValue = line.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();
                    if (!_variables.ContainsKey(key))
                    {
                        _variables[key] = value;
                    }
                }
            }
        }
    }
}
