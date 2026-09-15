# Countdown Clock & 8-Hour Workday Tracker

A modern .NET console application built with Spectre.Console that renders a live ticking countdown clock down to **November 30, 17:00** with precision metrics for both total calendar time and 8-hour workdays.

---

## Features

- **Live Workday Countdown Clock**: The main clock ticks down the **remaining business work time** broken down into **8-hour Workdays**, **Hours**, **Minutes**, **Seconds**, and **Tenths**.
- **Target Handling**: Automatically targets November 30 at 17:00:00 of the current year (or next year if already passed).
- **Total Calendar Time**:
  - Full calendar days, hours, minutes, and seconds remaining.
  - Elapsed calendar time tracker.
- **8-Hour Workday Breakdown**:
  - **Actual Business Workdays Remaining**: Active working hours (09:00–17:00, Mon–Fri) divided by 8 hours.
  - **Business Working Hours Left**: Exact business hours left between now and Nov 30 17:00.
  - **Mon-Fri Calendar Days Count**: Number of weekdays left.
  - **Workdays Spent (8h)**: Business working hours completed.
  - **Raw 24/7 Hours in 8h Shifts**: Total calendar hours divided by 8 hours.
- **Visual Progress Bars**:
  - Calendar Year Progress towards November 30.
  - Workdays Completed vs Total Workdays.
- **Active Work Hours Badge**: Live status indicator (09:00 - 17:00 Mon-Fri).
- **Graceful Controls**: Press `Q`, `ESC`, or `Ctrl+C` to quit cleanly.

---

## How to Run

### Live Interactive Clock
```bash
dotnet run
```

### Single Snapshot Frame
```bash
dotnet run -- --once
```

### Custom Target Date (for testing)
```bash
dotnet run -- --target "2026-11-30 17:00"
```

---

## Running Unit Tests

```bash
dotnet test
```
