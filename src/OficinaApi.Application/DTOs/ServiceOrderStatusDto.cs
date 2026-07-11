using OficinaApi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OficinaApi.Application.DTOs
{
    public class ServiceOrderStatusDto
    {
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }
    }
}
