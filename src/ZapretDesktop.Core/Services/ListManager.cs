namespace ZapretDesktop.Core.Services;

public class ListManager
{
    private readonly string _listsDirectory;

    public ListManager(string? customListsPath = null)
    {
        _listsDirectory = customListsPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "lists");
        EnsureDirectoryExists();
    }

    public IEnumerable<string> GetAvailableLists()
    {
        EnsureDirectoryExists();
        return Directory.GetFiles(_listsDirectory, "*.txt")
                        .Select(Path.GetFileName)
                        .Where(name => name != null)!;
    }

    public async Task AddCustomListAsync(string fileName, IEnumerable<string> domains)
    {
        EnsureDirectoryExists();
        if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".txt";
        }

        string filePath = Path.Combine(_listsDirectory, fileName);
        await File.WriteAllLinesAsync(filePath, domains);
    }

    public string BuildHostlistArgs(IEnumerable<string> selectedListNames)
    {
        var args = new List<string>();
        foreach (var listName in selectedListNames)
        {
            string fullPath = Path.Combine(_listsDirectory, listName);
            if (File.Exists(fullPath))
            {
                args.Add($"--hostlist=\"{fullPath}\"");
            }
        }
        return string.Join(" ", args);
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_listsDirectory))
        {
            Directory.CreateDirectory(_listsDirectory);
        }
    }
}