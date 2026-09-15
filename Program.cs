using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace CountdownClock;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "Countdown Clock - Target: Nov 30, 17:00";

        bool runOnce = false;
        DateTime? customTarget = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--once", StringComparison.OrdinalIgnoreCase) ||
                args[i].Equals("--snapshot", StringComparison.OrdinalIgnoreCase))
            {
                runOnce = true;
            }
            else if ((args[i].Equals("--target", StringComparison.OrdinalIgnoreCase) ||
                      args[i].Equals("-t", StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length)
            {
                if (DateTime.TryParse(args[++i], out var parsedTarget))
                {
                    customTarget = parsedTarget;
                }
            }
        }

        if (runOnce)
        {
            var now = DateTime.Now;
            var target = customTarget ?? TimeMetrics.GetUpcomingTarget(now);
            var metrics = new TimeMetrics(now, target);
            var layout = BuildDashboard(metrics);
            AnsiConsole.Write(layout);
            return;
        }

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        // Listen for 'q' or 'ESC' key in background to exit cleanly
        _ = Task.Run(() =>
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                        {
                            cts.Cancel();
                            break;
                        }
                    }
                }
                catch
                {
                    // In environments where Console.KeyAvailable might throw
                }
                Thread.Sleep(50);
            }
        });

        AnsiConsole.Clear();
        AnsiConsole.Cursor.Hide();

        try
        {
            await AnsiConsole.Live(CreatePlaceholderLayout())
                .AutoClear(false)
                .Overflow(VerticalOverflow.Crop)
                .StartAsync(async ctx =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        var now = DateTime.Now;
                        var target = customTarget ?? TimeMetrics.GetUpcomingTarget(now);
                        var metrics = new TimeMetrics(now, target);

                        var layout = BuildDashboard(metrics);
                        ctx.UpdateTarget(layout);

                        try
                        {
                            await Task.Delay(100, cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                });
        }
        finally
        {
            AnsiConsole.Cursor.Show();
            AnsiConsole.MarkupLine("[bold yellow]Countdown stopped. Goodbye![/]");
        }
    }

    private static IRenderable CreatePlaceholderLayout()
    {
        return new Panel(new Text("Initializing Countdown Clock..."))
            .BorderColor(Color.Cyan1)
            .RoundedBorder();
    }

    public static IRenderable BuildDashboard(TimeMetrics metrics)
    {
        var rootGrid = new Grid().AddColumn(new GridColumn().PadLeft(1).PadRight(1));

        // 1. Header with Live Status
        var headerTable = new Table().Border(TableBorder.None).HideHeaders().Expand();
        headerTable.AddColumn(new TableColumn("Left").LeftAligned());
        headerTable.AddColumn(new TableColumn("Right").RightAligned());

        string workStatusBadge = metrics.IsCurrentlyWorkHours
            ? "[bold white on darkgreen] ● WORK HOURS ACTIVE (09:00 - 17:00) [/]"
            : "[bold grey on grey23] ○ OUTSIDE WORK HOURS [/]";

        headerTable.AddRow(
            new Markup("[bold cyan]⏳ COUNTDOWN CLOCK & 8H WORKDAY TRACKER[/]"),
            new Markup(workStatusBadge)
        );

        headerTable.AddRow(
            new Markup($"[dim]Current Time:[/] [bold white]{metrics.Now:yyyy-MM-dd HH:mm:ss.ff (dddd)}[/]"),
            new Markup($"[dim]Target Deadline:[/] [bold yellow]{metrics.Target:MMMM dd, yyyy @ 17:00:00 (dddd)}[/]")
        );

        rootGrid.AddRow(new Panel(headerTable)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Cyan3)
            .Header("[bold cyan] TARGET: NOVEMBER 30 @ 17:00 [/]", Justify.Center));

        // 2. Big Clock Display (Remaining Work Time in 8h Workdays)
        var clockGrid = new Grid();
        clockGrid.AddColumn(new GridColumn().Centered());

        if (metrics.HasReachedTarget)
        {
            clockGrid.AddRow(new Markup("[bold red blink]🎯 TARGET DEADLINE REACHED! 🎯[/]"));
        }
        else
        {
            var clockDisplay = new Table().Border(TableBorder.Rounded).BorderColor(Color.Green);
            clockDisplay.AddColumns(
                new TableColumn("[bold green]WORKDAYS (8h)[/]").Centered(),
                new TableColumn("[bold green]HOURS[/]").Centered(),
                new TableColumn("[bold green]MINUTES[/]").Centered(),
                new TableColumn("[bold green]SECONDS[/]").Centered(),
                new TableColumn("[bold green]TENTHS[/]").Centered()
            );

            clockDisplay.AddRow(
                $"[bold white on darkgreen]     {metrics.RemainingFullWorkdays:D3}     [/]",
                $"[bold white on darkgreen]   {metrics.RemainingWorkHoursWithinDay:D2}   [/]",
                $"[bold white on darkgreen]   {metrics.RemainingWorkMinutes:D2}   [/]",
                $"[bold white on darkgreen]   {metrics.RemainingWorkSeconds:D2}   [/]",
                $"[bold grey on darkgreen]   {metrics.RemainingWorkMilliseconds / 100:D1}0   [/]"
            );

            clockGrid.AddRow(new Align(clockDisplay, HorizontalAlignment.Center));
        }

        rootGrid.AddRow(new Panel(clockGrid)
            .Border(BoxBorder.Heavy)
            .BorderColor(Color.Green)
            .Header("[bold green] 💼 REMAINING WORK TIME (8H WORKDAYS) [/]", Justify.Center));

        // 3. Metric Breakdown Cards
        var summaryGrid = new Grid();
        summaryGrid.AddColumn(new GridColumn().PadRight(1));
        summaryGrid.AddColumn(new GridColumn().PadLeft(1));

        // Left Card: Total Calendar Countdown
        var totalTimeTable = new Table().Border(TableBorder.Simple).Expand();
        totalTimeTable.AddColumn(new TableColumn("[bold cyan]Calendar Countdown Metric[/]"));
        totalTimeTable.AddColumn(new TableColumn("[bold cyan]Value[/]").RightAligned());

        totalTimeTable.AddRow("[bold white]Time Remaining[/]", $"[bold cyan]{metrics.TotalRemaining.Days}d {metrics.TotalRemaining.Hours}h {metrics.TotalRemaining.Minutes}m {metrics.TotalRemaining.Seconds}s[/]");
        totalTimeTable.AddRow("[grey]Total Days Remaining[/]", $"[bold white]{metrics.TotalRemaining.TotalDays:N2}[/] days");
        totalTimeTable.AddRow("[grey]Total Hours Remaining[/]", $"[bold white]{metrics.TotalRemaining.TotalHours:N1}[/] hours");
        totalTimeTable.AddRow("[grey]Total Minutes Remaining[/]", $"[bold white]{metrics.TotalRemaining.TotalMinutes:N0}[/] min");
        totalTimeTable.AddRow("[grey]Total Seconds Remaining[/]", $"[bold white]{metrics.TotalRemaining.TotalSeconds:N0}[/] sec");

        var totalTimePanel = new Panel(totalTimeTable)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .Header("[bold cyan] 🌐 Total Calendar Countdown [/]", Justify.Left);

        // Right Card: 8-Hour Workday Countdown
        var workdayTable = new Table().Border(TableBorder.Simple).Expand();
        workdayTable.AddColumn(new TableColumn("[bold green]Workday Countdown (8h/day)[/]"));
        workdayTable.AddColumn(new TableColumn("[bold green]Value[/]").RightAligned());

        workdayTable.AddRow(
            "[bold white]Workdays Remaining (8h/day)[/]",
            $"[bold green]{metrics.RemainingWorkdays:N2}[/] [dim]workdays[/]"
        );
        workdayTable.AddRow(
            "[grey]Business Hours Left (09-17)[/]",
            $"[bold green]{metrics.RemainingBusinessTime.TotalHours:N1}[/] [dim]hours ({metrics.RemainingBusinessTime.Hours}h {metrics.RemainingBusinessTime.Minutes}m {metrics.RemainingBusinessTime.Seconds}s)[/]"
        );
        workdayTable.AddRow(
            "[grey]Mon-Fri Business Days Left[/]",
            $"[bold green]{metrics.RemainingBusinessDaysCount}[/] [dim]calendar days[/]"
        );
        workdayTable.AddRow(
            "[grey]Standard 40h Work Weeks Left[/]",
            $"[bold white]{metrics.RemainingWorkWeeks:N2}[/] [dim]work weeks[/]"
        );
        workdayTable.AddRow(
            "[grey]Raw 24/7 Hours in 8h Shifts[/]",
            $"[bold white]{metrics.RemainingRaw8hShifts:N2}[/] [dim]shifts (8h)[/]"
        );

        var workdayPanel = new Panel(workdayTable)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green)
            .Header("[bold green] 💼 8-Hour Workday Countdown [/]", Justify.Left);

        summaryGrid.AddRow(totalTimePanel, workdayPanel);
        rootGrid.AddRow(summaryGrid);

        // 4. Controls / Footer
        rootGrid.AddRow(new Align(
            new Markup("[dim grey]Controls: Press [/][bold white]Q[/][dim grey] or [/][bold white]ESC[/][dim grey] or [/][bold white]Ctrl+C[/][dim grey] to quit. Live precision: 100ms.[/]"),
            HorizontalAlignment.Center
        ));

        return rootGrid;
    }
}

