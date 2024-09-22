using ImageProcessor.Enums;
using System.Collections.Generic;
using System;

namespace ImageProcessor.Dto.Integration;

public record OrderFeedback
{
    public long Id { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public List<OrderItemFeedback> OrderItemFeedback;
}

public record OrderItemFeedback
{
    public long Id { get; set; }
    public string CardFront { get; set; }
    public string CardGuid { get; set; }
    public DateTime ExpiryDate { get; set; }
    public OrderStatus OrderStatus { get; set; }
}