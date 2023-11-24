﻿using Infrastructure.Adapters;
using Models;
using Models.Days;
using Models.Dto.Metrics;
using Models.Util;

namespace Infrastructure.Repository.Interface;

public interface IMetricRepository
{
    Task<List<Metrics>> GetAllMetrics();
    Task<IEnumerable<CalendarDayAdapter>> GetMetricsForCalendarDays(Guid userId, DateTimeOffset fromDate, DateTimeOffset toDate);
    Task<ICollection<CalendarDayMetric>> Get(Guid userId, DateTimeOffset date);
    Task SaveMetrics(Guid calendarDayId, List<MetricRegisterMetricDto> metrics);
}
