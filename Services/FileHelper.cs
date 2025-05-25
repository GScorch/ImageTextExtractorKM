using System;
using System.IO;

public class FileHelper
{
    private readonly IWebHostEnvironment _env;

    public FileHelper(IWebHostEnvironment env)
    {
        _env = env;
    }
    public void SaveStringToFile(string content, string directoryPath, string fileName)
    {
        try
        {
            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Create the full path for the text file
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the content to the file
            File.WriteAllText(filePath, content);

            Console.WriteLine($"File saved to: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving file: {ex.Message}");
        }
    } 
    
    public void DeleteFolderRoot(string folderName)
    {
        // Combine the wwwroot path with the folder name
        string folderPath = Path.Combine(_env.WebRootPath, folderName);

        if (Directory.Exists(folderPath))
        {
            try
            {
                TryDeleteFolder(folderPath);
                Console.WriteLine("Folder and contents deleted.");
            }
            catch (IOException ex)
            {
                Console.WriteLine( $"Error deleting folder: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Folder not found.");
        }
    }
    bool TryDeleteFolder(string path, int retries = 3)
{
    for (int i = 0; i < retries; i++)
    {
        try
        {
            Directory.Delete(path, recursive: true);
            return true;
        }
        catch (IOException)
        {
            Thread.Sleep(100); // wait and try again
        }
    }
    return false;
}
}

