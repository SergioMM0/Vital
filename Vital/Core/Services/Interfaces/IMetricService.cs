﻿using Models;
using Models.Days;
using Models.Dto.Cycle;
using Models.Dto.Metrics;
using Models.Util;

namespace Vital.Core.Services.Interfaces;

public interface IMetricService
{
    Task<List<Metrics>> GetAllMetrics();
    Task<IEnumerable<CalendarDay>> GetMetricsForCalendarDays(Guid userId, DateTimeOffset fromDate,
        DateTimeOffset toDate);
    Task<ICollection<CalendarDayMetric>> Get(Guid userId, DateTimeOffset date);
    Task<CalendarDayDto> UploadMetricForADay(Guid userId, List<MetricRegisterMetricDto> metricsDto, DateTimeOffset dateTimeOffset);
}
