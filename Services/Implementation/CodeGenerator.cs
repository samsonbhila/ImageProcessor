using ZXing;
using QRCoder;
using ZXing.QrCode;
using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessor.Services.Contracts;

namespace ImageProcessor.Services.Implementation;

public class CodeGenerator: ICodeGenerator
{
    private readonly IStorageService _storageService;

    public CodeGenerator(IStorageService storageService)
    {
        _storageService = storageService;
    }
    public async Task GenerateQrCode(long customerNo, string guid)
    {
        if (!Directory.Exists("Temp"))
        {
            Directory.CreateDirectory("Temp");
        }
        
        using (var qrGenerator = new QRCodeGenerator())
        {
            var qrCodeData = qrGenerator.CreateQrCode(guid, QRCodeGenerator.ECCLevel.Q);
            using (var qrCode = new QRCode(qrCodeData))
            {
                using (Bitmap bitmap = qrCode.GetGraphic(20))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        File.WriteAllBytes($"Temp/{guid}-qrcode.png", stream.ToArray());
                        await _storageService.UploadImage($"Temp/{guid}-qrcode.png", $"qrcodes/{customerNo}/{guid}-qrcode.png");
                    }
                }
            }
        }
    }

    public async Task GenerateBarCode(long customerNo, string guid)
    {
        var barcodeWriter = new BarcodeWriterPixelData()
        {
            Format = BarcodeFormat.CODE_39,
            Options = new QrCodeEncodingOptions()
            {
                Height = 150,
                Width = 500,
                Margin = 15
            }
        };

        // Generate barcode 
        var pixelData = barcodeWriter.Write(guid);

        // Create a bitmap and save it to a file
        using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
        using (var ms = new MemoryStream())
        {
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                // We assume that the row stride of the bitmap is aligned to 4 bytes multiplied by the width of the image
                System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }

            // Save to a file for demonstration purposes
            bitmap.Save($"Temp/{guid}-barcode.png", ImageFormat.Png);
            await _storageService.UploadImage($"Temp/{guid}-barcode.png", $"barcodes/{customerNo}/{guid}-barcode.png");
        }
    }
}