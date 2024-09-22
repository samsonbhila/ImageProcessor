using ImageProcessor.Enums;
using System.Collections.Generic;
using System;

namespace ImageProcessor.Dto.Request;

public record ProcessOrderRequest
{
    public long Id { get; set; }
    public long CustomerNo { get; set; }
    public string MemberCompanyName { get; set; }
    public List<OrderItemsRequest> OrderItems { get; set; }
}

public record OrderItemsRequest
{
    public long Id { get; set; }
    public string CardType { get; set; }
    public string EmploymentType { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string FilePath { get; set; }
}