namespace ImageProcessor.Services.Contracts;

public interface ICodeGenerator
{
    Task GenerateQrCode(long customerNo, string guid);
    Task GenerateBarCode(long customerNo, string guid);
}