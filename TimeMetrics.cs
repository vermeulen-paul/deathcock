using System;

namespace CountdownClock;

public class TimeMetrics
{
    public static readonly TimeSpan WorkDayStart = new(9, 0, 0);   // 09:00 AM
    public static readonly TimeSpan WorkDayEnd = new(17, 0, 0);    // 05:00 PM (8h workday)
    public const double HoursPerWorkDay = 8.0;

    public DateTime Now { get; }
    public DateTime Target { get; }

    public bool HasReachedTarget => Now >= Target;
    public TimeSpan TotalRemaining => HasReachedTarget ? TimeSpan.Zero : Target - Now;

    // Remaining business metrics
    public TimeSpan RemainingBusinessTime { get; }
    public int RemainingBusinessDaysCount { get; }
    public bool IsCurrentlyWorkHours { get; }
    public double RemainingWorkdays => RemainingBusinessTime.TotalHours / HoursPerWorkDay;
    public double RemainingRaw8hShifts => TotalRemaining.TotalHours / HoursPerWorkDay;
    public double RemainingWorkWeeks => RemainingBusinessTime.TotalHours / 40.0;

    // Remaining work time components (8h workdays + hours + mins + secs)
    public int RemainingFullWorkdays => (int)Math.Floor(RemainingBusinessTime.TotalHours / HoursPerWorkDay);
    public int RemainingWorkHoursWithinDay => (int)Math.Floor(RemainingBusinessTime.TotalHours % HoursPerWorkDay);
    public int RemainingWorkMinutes => RemainingBusinessTime.Minutes;
    public int RemainingWorkSeconds => RemainingBusinessTime.Seconds;
    public int RemainingWorkMilliseconds => RemainingBusinessTime.Milliseconds;

    public TimeMetrics(DateTime now, DateTime target)
    {
        Now = now;
        Target = target;

        var (remTime, remDays, isWorkHours) = CalculateBusinessDuration(Now, Target);
        RemainingBusinessTime = remTime;
        RemainingBusinessDaysCount = remDays;
        IsCurrentlyWorkHours = isWorkHours;
    }

    public static (TimeSpan businessDuration, int businessDaysCount, bool isStartInWorkHours) CalculateBusinessDuration(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            return (TimeSpan.Zero, 0, false);
        }

        bool isStartInWorkHours = IsWorkDay(start.DayOfWeek) &&
                                  start.TimeOfDay >= WorkDayStart &&
                                  start.TimeOfDay < WorkDayEnd;

        double totalBusinessSeconds = 0;
        int businessDaysCount = 0;

        DateTime currentDay = start.Date;
        DateTime endDay = end.Date;

        while (currentDay <= endDay)
        {
            if (IsWorkDay(currentDay.DayOfWeek))
            {
                businessDaysCount++;

                DateTime dayWorkStart = currentDay.Add(WorkDayStart);
                DateTime dayWorkEnd = currentDay.Add(WorkDayEnd);

                DateTime effectiveStart = (currentDay == start.Date && start > dayWorkStart)
                    ? (start > dayWorkEnd ? dayWorkEnd : start)
                    : dayWorkStart;

                DateTime effectiveEnd = (currentDay == endDay && end < dayWorkEnd)
                    ? (end < dayWorkStart ? dayWorkStart : end)
                    : dayWorkEnd;

                if (effectiveEnd > effectiveStart)
                {
                    totalBusinessSeconds += (effectiveEnd - effectiveStart).TotalSeconds;
                }
            }

            currentDay = currentDay.AddDays(1);
        }

        return (TimeSpan.FromSeconds(totalBusinessSeconds), businessDaysCount, isStartInWorkHours);
    }

    public static bool IsWorkDay(DayOfWeek dayOfWeek)
    {
        return dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
    }

    public static DateTime GetUpcomingTarget(DateTime now)
    {
        var targetThisYear = new DateTime(now.Year, 11, 30, 17, 0, 0);
        if (now <= targetThisYear)
        {
            return targetThisYear;
        }

        return new DateTime(now.Year + 1, 11, 30, 17, 0, 0);
    }
}
