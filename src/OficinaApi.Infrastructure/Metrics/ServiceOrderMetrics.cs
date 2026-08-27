using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Entities;

namespace OficinaApi.Infrastructure.Metrics;

public class ApplicationMetrics(
    TimeProvider timeProvider,
    ILogger<ApplicationMetrics> logger
) : IApplicationMetrics
{

    public static readonly Meter ServiceOrderStatusMeanTime = new("ServiceOrder.StatusMeanTime", "1.0.0");
    public static readonly Histogram<double> ServiceOrderStatusMeanTimeHistogram = ServiceOrderStatusMeanTime.CreateHistogram<double>(
        name: "service_order_status_mean_time_in_days",
        unit: "d",
        description: "Tempo médio em que uma ordem de serviço permaneceu em um estado");

    public void CalculateServiceOrderStatusMeanTimeMetric(ServiceOrder serviceOrder)
    {
        try
        {            
            var status = serviceOrder.StatusHistory;

            if(status.Count <= 0)
            {
                logger.LogWarning("Metric {MetricName} could not be collected. Insufficient Status in service order", ServiceOrderStatusMeanTime.Name);
                return;
            }
            var lastStatus = status.OrderDescending().Skip(1).First();

            var durationInDays = (timeProvider.GetUtcNow() - lastStatus.CreatedAt).TotalDays;

            ServiceOrderStatusMeanTimeHistogram.Record(
                durationInDays,
                new KeyValuePair<string, object?>("id", serviceOrder.Id.ToString()),
                new KeyValuePair<string, object?>("status", lastStatus.Status.ToString()),
                new KeyValuePair<string, object?>("entity_type", nameof(ServiceOrder))
            );
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex, "Exception at metric {MetricName} collect.", ServiceOrderStatusMeanTime.Name);
        }
    }
}
