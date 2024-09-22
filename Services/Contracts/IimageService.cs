using ImageProcessor.Dto.Integration;
using ImageProcessor.Enums;
using ImageProcessor.Dto.Request;
using ImageProcessor.Dto.Response;

namespace ImageProcessor.Services.Contracts;

public interface IimageService
{
    public Task<OrderResponse> ProcessOrder(ProcessOrderRequest request);
    public Task<QueueStatus> GetQueueStatus(string Id);
    public Task CallBackCoreApiWebHook(OrderFeedback orderFeedback);
    public Task<string> GenerateCardFront(string guid, string imagePath, string cardType, string memberCompanyName, DateTime expiryDate);
}