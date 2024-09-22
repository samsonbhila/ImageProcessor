using Hangfire;
using ImageProcessor.Data;
using ImageProcessor.Enums;
using ImageProcessor.Models;
using Microsoft.EntityFrameworkCore;
using ImageProcessor.Dto.Integration;
using ImageProcessor.Services.Contracts;

namespace ImageProcessor.Jobs;

public class ImageProcessJob
{
    private readonly AppDbContext _context;
    private readonly IStorageService _storageService;
    private readonly IimageService _imageService;
    private readonly ILogger<ImageProcessJob> _logger;
    private readonly ICodeGenerator _codeGenerator;

    public ImageProcessJob(
        AppDbContext context, 
        ILogger<ImageProcessJob> logger, 
        IStorageService storageService, 
        IimageService imageService,
        ICodeGenerator codeGenerator
        )
    {
        _context = context;
        _logger = logger;
        _storageService = storageService;
        _imageService = imageService;
        _codeGenerator = codeGenerator;
    }
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public async Task ProcessImageQueue()
    {
        List<Order> orders = await _context
            .Orders
            .Include("OrderItems")
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync();

        foreach (Order order in orders)
        {
            // Update Order status to processing
            order.Status = OrderStatus.Processing;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            List<OrderItemFeedback> orderItemFeedback = new List<OrderItemFeedback>();
            
            foreach (var item in order.OrderItems)
            {
                string cardGuid = Guid.NewGuid().ToString();
                
                try
                {
                    //Update order item to processing
                    item.OrderItemStatus = OrderStatus.Processing;
                    item.UpdatedAt = DateTime.Now;
                    _context.OrderItems.Update(item);
                    await _context.SaveChangesAsync();

                    // Download image
                    _logger.LogInformation($"Download image { item.FilePath }");
                    string localImage = await _storageService.GetImage(item.FilePath);
                    _logger.LogInformation($"Image download complete");
                    
                    // Crop out image
                    _logger.LogInformation($"Cropping image { localImage }");
                    _storageService.CropImage(localImage);
                    _logger.LogInformation("Image crop complete");
                    
                    // Upload image
                    _logger.LogInformation($"Uploading image { item.FilePath }");
                    await _storageService.UploadImage(localImage, item.FilePath);
                    _logger.LogInformation("Image upload complete");
                    
                    // Generate QR Code
                    await _codeGenerator.GenerateQrCode(order.CustomerNo, cardGuid);
                    
                    //Generate BarCode
                    await _codeGenerator.GenerateBarCode(order.CustomerNo, cardGuid);
                    
                    //Generate Card Front
                    string cardFront = await _imageService.GenerateCardFront(cardGuid, localImage, item.EmploymentType, order.MemberCompanyName, item.ExpiryDate);
                    await _storageService.UploadImage(cardFront, $"idcards/{order.CustomerNo}/{cardGuid}-front.png");
                    
                    orderItemFeedback.Add(new OrderItemFeedback()
                    {
                        Id = item.OrderItemId,
                        CardGuid = cardGuid,
                        ExpiryDate = item.ExpiryDate,
                        CardFront = $"idcards/{order.CustomerNo}/{cardGuid}-front.png",
                        OrderStatus = OrderStatus.Complete,
                    });
                    
                    // Update order item status to complete
                    item.OrderItemStatus = OrderStatus.Complete;
                    _context.OrderItems.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    // Update order item status to failed
                    item.OrderItemStatus = OrderStatus.Failed;
                    item.StatusError = e.Message;
                    _logger.LogError(e.Message, e);
                    
                    orderItemFeedback.Add(new OrderItemFeedback()
                    {
                        Id = item.OrderItemId,
                        CardGuid = cardGuid,
                        ExpiryDate = item.ExpiryDate,
                        CardFront = "",
                        OrderStatus = OrderStatus.Failed,
                    });
                }

                _context.OrderItems.Update(item);
                await _context.SaveChangesAsync();
            }

            // Update status to complete
            order.Status = OrderStatus.Complete;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            OrderFeedback orderFeedback = new OrderFeedback()
            {
                Id = order.OrderId,
                OrderStatus = OrderStatus.Complete,
                OrderItemFeedback = orderItemFeedback
            };
            
            await _imageService.CallBackCoreApiWebHook(orderFeedback);
        }
    }
}