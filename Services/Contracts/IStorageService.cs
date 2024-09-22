namespace ImageProcessor.Services.Contracts;

public interface IStorageService
{
    Task<string> GetImage(string filePath);
    Task UploadImage(string localFilePath, string remoteFilePath);
    void CropImage(string localFilePath);
}