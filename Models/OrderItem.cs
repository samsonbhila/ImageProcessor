using System.ComponentModel.DataAnnotations;
using ImageProcessor.Enums;

namespace ImageProcessor.Models;

public class OrderItem: ModelBase
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public long OrderItemId { get; set; }
    [MaxLength(1000)]
    public string FilePath { get; set; }
    [MaxLength(255)]
    public string CardType { get; set; }
    [MaxLength(255)]
    public string EmploymentType { get; set; }
    public DateTime ExpiryDate { get; set; }
    public OrderStatus OrderItemStatus { get; set; }
    [MaxLength(1000)]
    public string StatusError { get; set; } = String.Empty;
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}