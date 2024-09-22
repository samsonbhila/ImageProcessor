using System.Text;
using System.Drawing;
using Newtonsoft.Json;
using ImageProcessor.Data;
using ImageProcessor.Enums;
using ImageProcessor.Models;
using System.Drawing.Drawing2D;
using ImageProcessor.Dto.Request;
using ImageProcessor.Dto.Response;
using Microsoft.EntityFrameworkCore;
using ImageProcessor.Dto.Integration;
using ImageProcessor.Services.Contracts;

namespace ImageProcessor.Services.Implementation;

public class ImageService: IimageService
{
    private readonly string? _coreApiUrl;
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageService> _logger;

    public ImageService(
        AppDbContext context, 
        ILogger<ImageService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
        ) {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        
        _coreApiUrl = configuration["CoreAPI:Url"];
        string token = configuration["CoreAPI:Key"];
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer { token }");
    }
    public async Task<OrderResponse> ProcessOrder(ProcessOrderRequest request)
    {
        ICollection<OrderItem> orderItems = new List<OrderItem>();

        foreach (var item in request.OrderItems)
        {
            orderItems.Add(new OrderItem()
            {
                Id = Guid.NewGuid(),
                OrderItemId = item.Id,
                OrderItemStatus = OrderStatus.Pending,
                CardType = item.CardType,
                EmploymentType = item.EmploymentType,
                ExpiryDate = item.ExpiryDate,
                FilePath = item.FilePath,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        Guid OrderId = Guid.NewGuid();
        
        Order order = new Order()
        {
            Id = OrderId,
            OrderId = request.Id,
            CustomerNo = request.CustomerNo,
            MemberCompanyName = request.MemberCompanyName,
            Status = OrderStatus.Pending,
            OrderItems = orderItems,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Queued order for processing { OrderId.ToString() }");
        
        return new OrderResponse()
        {
            QueueId = OrderId.ToString()
        };
    }

    public async Task<QueueStatus> GetQueueStatus(string Id)
    {
        Order order = await _context.Orders
            .Include("OrderItems")
            .SingleAsync(o => o.Id.ToString() == Id);

        List<QueueItem> queueItems = new List<QueueItem>();

        foreach (var item in order.OrderItems)
        {
            queueItems.Add(new QueueItem()
            {
                Id = item.Id.ToString(),
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                FilePath = item.FilePath,
                OrderItemStatus = item.OrderItemStatus.ToString()
            });
        }

        return new QueueStatus()
        {
            Id = order.Id.ToString(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderId = order.OrderId,
            Status = order.Status.ToString(),
            OrderItems = queueItems
        };
    }

    public async Task CallBackCoreApiWebHook(OrderFeedback orderFeedback)
    {
        var content = new StringContent(
            JsonConvert.SerializeObject(orderFeedback), 
            Encoding.UTF8, "application/json"
        );

        var response = await _httpClient.PostAsync($"{_coreApiUrl}/order/feedback", content);
        
        _logger.LogInformation($"Feedback Host: {_coreApiUrl}/order/feedback");
        _logger.LogInformation(JsonConvert.SerializeObject(orderFeedback));
        _logger.LogInformation("Response: " + response.ReasonPhrase);
    }
    public async Task<string> GenerateCardFront(string guid, string imagePath, string employmentType, string memberCompanyName, DateTime expiryDate)
    {
        using (Image baseDesign = Image.FromFile(GetTemplateUrl(employmentType)))
        using (Image employeePhoto = Image.FromFile(imagePath))
        using (Bitmap circularPhoto = MakeCircularImage(employeePhoto))
        using (Image barCode = Image.FromFile($"Temp/{guid}-barcode.png"))
        using (Image qrCOde = Image.FromFile($"Temp/{guid}-qrcode.png"))
        using (Graphics graphics = Graphics.FromImage(baseDesign))
        {
            graphics.DrawImage(circularPhoto, new Rectangle(new Point(50, 300), new Size(250, 250)));
            graphics.DrawImage(qrCOde, new Rectangle(new Point(50, 580), new Size(200, 200)));
            graphics.DrawImage(barCode, new Rectangle(new Point(240, 640), new Size(400, 100)));

            // Add text
            using (Font font = new Font("Arial", 8))
            {
                SolidBrush brush = new SolidBrush(Color.White);
                Point nameLocation = new Point(70, 800); 
                graphics.DrawString(memberCompanyName, font, brush, nameLocation);
            }
            
            //Add Expiry date
            using (Font font = new Font("Arial", 7))
            {
                SolidBrush brush = new SolidBrush(Color.White);
                Point nameLocation = new Point(115, 47); 
                graphics.DrawString(expiryDate.ToShortDateString(), font, brush, nameLocation);
            }

            // Save the final ID card image
            baseDesign.Save($"Temp/{guid}-front.png");
        }
    
        return $"Temp/{guid}-front.png";
    }

    private string GetTemplateUrl(string employmentType)
    {
        return $"CardTemplates/{employmentType.ToLower()}-card-front.png";
    }
    private Bitmap MakeCircularImage(Image image)
    {
        Bitmap circularBitmap = new Bitmap(image.Width, image.Height);
        using (Graphics graphics = Graphics.FromImage(circularBitmap))
        {
            Rectangle bounds = new Rectangle(0, 0, image.Width, image.Height);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(bounds);
                graphics.SetClip(path);
                graphics.DrawImage(image, bounds);
            }
        }
        return circularBitmap;
    }
}