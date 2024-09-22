using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Drawing;
using Emgu.CV.Structure;
using Azure.Storage.Blobs;
using ImageProcessor.Services.Contracts;

namespace ImageProcessor.Services.Implementation;

public class StorageService: IStorageService
{
    private readonly BlobContainerClient _blobContainerClient;
    
    public StorageService(IConfiguration configuration)
    {
        _blobContainerClient = new BlobContainerClient(
            configuration["ConnectionStrings:StorageConnection"], 
            configuration["Storage:Container"]
        );
    }
    public async Task<string> GetImage(string filePath)
    {
        if (!Directory.Exists("Temp"))
        {
            Directory.CreateDirectory("Temp");
        }
        
        BlobClient blobClient = _blobContainerClient.GetBlobClient(filePath);
        string[] parts = filePath.Split("/");
        
        string savePath = "Temp/" + parts.Last();

        // Download the blob's contents and save it to a file
        await blobClient.DownloadToAsync(savePath);
        
        return savePath;
    }

    public async Task UploadImage(string localFilePath, string remoteFilePath)
    {
        BlobClient blobClient = _blobContainerClient.GetBlobClient(remoteFilePath);
        using FileStream uploadFileStream = File.OpenRead(localFilePath);
        await blobClient.UploadAsync(uploadFileStream, overwrite: true);
        uploadFileStream.Close();
    }

    public  void CropImage(string localFilePath)
    {
        string faceHaarCascadePath = "haarcascade_frontalface_default.xml";

        // Load the image
        using (Image<Bgr, byte> image = new Image<Bgr, byte>(localFilePath))
        {
            // Load the cascade classifier
            CascadeClassifier faceCascade = new CascadeClassifier(faceHaarCascadePath);

            // Convert the image to grayscale for processing
            using (Image<Gray, byte> grayImage = image.Convert<Gray, byte>())
            {
                // Detect faces
                Rectangle[] facesDetected = faceCascade.DetectMultiScale(
                    grayImage,
                    1.1,
                    10,
                    new Size(20, 20),
                    Size.Empty);

                // Proceed if faces are detected
                if (facesDetected.Length > 0)
                {
                    // For simplicity, we'll use the first detected face
                    Rectangle faceRect = facesDetected[0];

                    // Optionally, add some padding around the face to make the ID photo look better
                    int padding = 50;
                    Rectangle cropRect = new Rectangle(
                        Math.Max(faceRect.X - padding, 0),
                        Math.Max(faceRect.Y - padding, 0),
                        Math.Min(faceRect.Width + 2 * padding, image.Width),
                        Math.Min(faceRect.Height + 2 * padding, image.Height));

                    // Crop the image
                    Image<Bgr, byte> croppedImage = image.GetSubRect(cropRect);

                    // Save the cropped image
                    croppedImage.Save(localFilePath);
                }
            }
        }
    }
}