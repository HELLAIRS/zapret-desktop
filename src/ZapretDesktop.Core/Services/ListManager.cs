namespace ZapretDesktop.Core.Services;

public class ListManager
{
    private readonly string _listsDir;

    public ListManager()
    {
        _listsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lists");
        Directory.CreateDirectory(_listsDir);
    }

    public async Task<string> ReadListAsync(string fileName)
    {
        var filePath = Path.Combine(_listsDir, fileName);
        if (!File.Exists(filePath)) return string.Empty;

        return await File.ReadAllTextAsync(filePath);
    }

    public async Task SaveListAsync(string fileName, string content)
    {
        var filePath = Path.Combine(_listsDir, fileName);
        await File.WriteAllTextAsync(filePath, content);
    }
}