﻿using System.Collections;
using System.Data;
using Dapper;
using Infrastructure.Repository.Interface;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Models;
using Models.Days;
using Models.Dto.Metrics;
using Models.Util;

namespace Infrastructure.Repository;

public class MetricRepository : IMetricRepository
{
    private readonly IDbConnection _db;
    private readonly ICalendarDayRepository _calendarDayRepository;

    public MetricRepository(IDbConnection db, ICalendarDayRepository calendarDayRepository)
    {
        _db = db;
        _calendarDayRepository = calendarDayRepository;
    }

    public async Task<List<Metrics>> GetAllMetrics()
    {
        var sql = @"SELECT M.*, MV.* 
                    FROM ""Metrics"" M
                    LEFT JOIN public.""MetricValue"" MV on M.""Id"" = MV.""MetricsId""";
        var list = await _db.QueryAsync<Metrics, MetricValue, Metrics>(sql,
            (metrics, value) =>
            {
                metrics.Values.Add(value);
                return metrics;
            });
        return list.GroupBy(m => m.Id).Select(g =>
        {
            var groupedMetric = g.First();
            groupedMetric.Values = g.Select(p => p.Values.FirstOrDefault()).Where(p => p != null).ToList() ?? new List<MetricValue>();
            return groupedMetric;
        }).ToList();
    }
    
    public async Task<IEnumerable<CalendarDay>> GetMetricsForCalendarDays(Guid userId, DateTimeOffset fromDate, DateTimeOffset toDate)
    {
        var sql = @"SELECT * FROM ""CalendarDay""
                LEFT JOIN ""CalendarDayMetric"" CDM on ""CalendarDay"".""Id"" = CDM.""CalendarDayId""
                LEFT JOIN ""MetricValue"" MV on CDM.""MetricValueId"" = MV.""Id""
                LEFT JOIN ""Metrics"" M on M.""Id"" = CDM.""MetricsId""
        WHERE CAST(""Date"" AS DATE) >= @from AND CAST(""Date"" AS DATE) <= @to
                AND ""UserId"" = @userId";

        var result = await _db.QueryAsync<CalendarDay, CalendarDayMetric, MetricValue, Metrics, CalendarDay>(
            sql,
            (calendarDay, calendarDayMetric, metricValue, metrics) =>
            {
                // I'm assuming these relationships exist in your models, adapt accordingly if not
                calendarDayMetric.MetricValue = metricValue;
                calendarDayMetric.Metrics = metrics;
                calendarDay.SelectedMetrics.Add(calendarDayMetric);
                return calendarDay;
            },
            new { from = fromDate.Date, to = toDate.Date, userId },
            // Specify the columns at which the returned rows should be split up into different objects
            splitOn: "CalendarDayId,MetricValueId,Id");

        return result;
    }
    
    public async Task<ICollection<CalendarDayMetric>> Get(Guid userId, DateTimeOffset date)
    {
        var calendarDay = await _calendarDayRepository.GetByDate(userId, date);
        var sql = $@"SELECT
                CDM.*,
                ""Metrics"".*,
                MV.*
                FROM ""CalendarDayMetric"" CDM
                    INNER JOIN ""MetricValue"" MV ON CDM.""MetricValueId"" = MV.""Id""
                    INNER JOIN ""Metrics"" on ""Metrics"".""Id"" = CDM.""MetricsId""    
                WHERE CDM.""CalendarDayId""=@calendarDayId";
        if (calendarDay is null)
        {
            return new List<CalendarDayMetric>();
        }

        // The result: Id, CalendarDayId, MetricsId, MetricValueId, Id, Name, Id, Name, MetricsId
        var metrics = await _db.QueryAsync<CalendarDayMetric, Metrics, MetricValue, CalendarDayMetric>(
            sql,
            (calendarDayMetrics, metrics, metricValue) =>
            {
                metrics.Values = new List<MetricValue>() { metricValue };
                calendarDayMetrics.Metrics = metrics;
                calendarDayMetrics.MetricValue = metricValue;
                return calendarDayMetrics;
            }, splitOn: "Id, Id",

            param: new { calendarDayId = calendarDay.Id });
        return metrics.ToList();
    }
    
    

    public async Task UploadMetricForADay(Guid calendarDayId, List<MetricRegisterMetricDto> metrics)
    {
        // Delete all metrics for the day, if there are any
        var sql = @"DELETE FROM ""CalendarDayMetric"" WHERE ""CalendarDayId""=@calendarDayId";
        await _db.ExecuteAsync(sql, new { calendarDayId });

        // Insert new metrics for the day
        sql = @"INSERT INTO ""CalendarDayMetric"" (""Id"",""CalendarDayId"", ""MetricsId"", ""MetricValueId"") VALUES (@Id, @calendarDayId, @metricsId, @metricValueId)";
        foreach (var metricsDto in metrics)
        {
            await _db.ExecuteAsync(sql, new { Id = Guid.NewGuid(), calendarDayId, metricsId = metricsDto.MetricsId, metricValueId = metricsDto.MetricValueId });
        }
    }
}