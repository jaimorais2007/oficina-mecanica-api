using System;

namespace OficinaApi.Application.DTOs;

public class PartDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public decimal Price { get; set; }
}

public class CreatePartDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int InitialQuantity { get; set; }
    public decimal Price { get; set; }
}

public class UpdateStockDto
{
    public int Quantity { get; set; }
}
