using ImageProcessor.Enums;
using System.ComponentModel.DataAnnotations;

namespace ImageProcessor.Models;

public class Order: ModelBase
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public long OrderId { get; set; }
    public long CustomerNo { get; set; }
    public string MemberCompanyName { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}