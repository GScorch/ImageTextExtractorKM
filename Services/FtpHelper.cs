using System;
using System.IO;
using System.Net;

public class FtpHelper
{
    public bool UploadFolder(string localFolderPath, string ftpServerUrl, string ftpUsername, string ftpPassword, string Baseroot)
    {
        try
        { 
            
            string ftpDirectoryPathCreator = (ftpServerUrl + localFolderPath).Replace("\\", "/");//.Replace(":/","://");
            CreateFtpDirectory(ftpDirectoryPathCreator, ftpUsername, ftpPassword);

            var chceckFile = (Baseroot + localFolderPath).Replace("\\", "/");//.Replace(":/","://");
            // Upload each file in the folder
            foreach (string file in Directory.GetFiles(chceckFile))
            {
                UploadFile(file, ftpDirectoryPathCreator, ftpUsername, ftpPassword);
            }

            // Recurse through subdirectories
            foreach (string directory in Directory.GetDirectories(localFolderPath))
            {
                string directoryName = Path.GetFileName(directory);
                //string ftpDirectoryPath = (ftpDirectoryPathCreator + directoryName).Replace("\\", "/").Replace(":/","://");

                // Create directory on FTP if it does not exist
                //CreateFtpDirectory(ftpDirectoryPath, ftpUsername, ftpPassword);

                // Recursively upload files and subfolders
                UploadFolder(directory, ftpDirectoryPathCreator, ftpUsername, ftpPassword, Baseroot);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading folder: {ex.Message}");
            return false;
        }
    }

    private void UploadFile(string localFilePath, string ftpServerUrl, string ftpUsername, string ftpPassword)
    {
        try
        {
            string ftpFilePath = ftpServerUrl + "/" + Path.GetFileName(localFilePath);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = false;

            byte[] fileContents = File.ReadAllBytes(localFilePath);
            request.ContentLength = fileContents.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine($"Uploaded file: {localFilePath} to {ftpFilePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
        }
    }

    private void CreateFtpDirectory(string ftpDirectoryPath, string ftpUsername, string ftpPassword)
    {
        var uri = new Uri(ftpDirectoryPath);
        string[] segments = uri.AbsolutePath.Trim('/').Split('/');

        string baseUrl = $"{uri.Scheme}://{uri.Host}";
        string currentPath = baseUrl;

        foreach (var segment in segments)
        {
            currentPath += "/" + segment;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Console.WriteLine($"[+] Created: {currentPath}");
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    Console.WriteLine($"[-] Already exists: {currentPath}");
                }
                else
                {
                    Console.WriteLine($"[!] Error creating: {currentPath} - {response.StatusDescription}");
                    throw;
                }
            }
        }
    }
}