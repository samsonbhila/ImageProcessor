using System.Collections.Generic;
using System;

namespace ImageProcessor.Dto.Response;

public record QueueStatus
{
    public string Id { get; set; }
    public long OrderId { get; set; }
    public string Status { get; set; }
    public ICollection<QueueItem> OrderItems { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record QueueItem
{
    public string Id { get; set; }
    public string FilePath { get; set; }
    public string OrderItemStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}