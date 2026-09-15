using System;
using CountdownClock;
using Xunit;

namespace CountdownClock.Tests;

public class TimeMetricsTests
{
    [Fact]
    public void GetUpcomingTarget_BeforeNov30_ReturnsSameYear()
    {
        var now = new DateTime(2026, 5, 10, 12, 0, 0);
        var target = TimeMetrics.GetUpcomingTarget(now);
        Assert.Equal(new DateTime(2026, 11, 30, 17, 0, 0), target);
    }

    [Fact]
    public void GetUpcomingTarget_AfterNov30_ReturnsNextYear()
    {
        var now = new DateTime(2026, 12, 1, 9, 0, 0);
        var target = TimeMetrics.GetUpcomingTarget(now);
        Assert.Equal(new DateTime(2027, 11, 30, 17, 0, 0), target);
    }

    [Fact]
    public void CalculateBusinessDuration_SingleWorkDay_ReturnsCorrectHours()
    {
        // Monday 09:00 to Monday 17:00 => 8 hours (1 workday)
        var start = new DateTime(2026, 11, 2, 9, 0, 0); // Monday
        var end = new DateTime(2026, 11, 2, 17, 0, 0);

        var (duration, daysCount, isWorkHours) = TimeMetrics.CalculateBusinessDuration(start, end);

        Assert.Equal(8.0, duration.TotalHours);
        Assert.Equal(1, daysCount);
        Assert.True(isWorkHours);
    }

    [Fact]
    public void CalculateBusinessDuration_AcrossWeekend_ExcludesWeekend()
    {
        // Friday 09:00 to Monday 17:00 => 2 workdays (16 hours), skipping Sat & Sun
        var friday = new DateTime(2026, 11, 6, 9, 0, 0);   // Friday
        var monday = new DateTime(2026, 11, 9, 17, 0, 0);  // Monday

        var (duration, daysCount, _) = TimeMetrics.CalculateBusinessDuration(friday, monday);

        Assert.Equal(16.0, duration.TotalHours);
        Assert.Equal(2, daysCount);
    }

    [Fact]
    public void CalculateBusinessDuration_WeekendStart_StartsNextWeekday()
    {
        // Saturday 12:00 to Monday 17:00 => 8 hours (Monday only)
        var saturday = new DateTime(2026, 11, 7, 12, 0, 0); // Saturday
        var monday = new DateTime(2026, 11, 9, 17, 0, 0);   // Monday

        var (duration, daysCount, isWorkHours) = TimeMetrics.CalculateBusinessDuration(saturday, monday);

        Assert.Equal(8.0, duration.TotalHours);
        Assert.Equal(1, daysCount);
        Assert.False(isWorkHours);
    }

    [Fact]
    public void TimeMetrics_CalculatesSpentAndRemainingWorkdays()
    {
        var baseline = new DateTime(2026, 11, 2, 9, 0, 0);  // Monday
        var now = new DateTime(2026, 11, 3, 13, 0, 0);       // Tuesday 13:00 (1 day + 4h = 12h = 1.5 workdays)
        var target = new DateTime(2026, 11, 4, 17, 0, 0);    // Wednesday 17:00 (target)

        var metrics = new TimeMetrics(now, target, baseline);

        Assert.Equal(1.5, metrics.SpentWorkdays, precision: 2);
        Assert.Equal(1.5, metrics.RemainingWorkdays, precision: 2);
        Assert.Equal(1, metrics.RemainingFullWorkdays);
        Assert.Equal(4, metrics.RemainingWorkHoursWithinDay);
        Assert.Equal(0, metrics.RemainingWorkMinutes);
        Assert.Equal(0, metrics.RemainingWorkSeconds);
        Assert.Equal(3.0, metrics.TotalPeriodWorkdays, precision: 2);
        Assert.Equal(50.0, metrics.WorkdayProgressPercentage, precision: 1);
    }
}

