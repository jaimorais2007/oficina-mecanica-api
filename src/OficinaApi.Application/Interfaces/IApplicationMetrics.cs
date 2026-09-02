using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Application.Interfaces;

public interface IApplicationMetrics
{
    void CalculateServiceOrderStatusMeanTimeMetric(ServiceOrder serviceOrder);
}
