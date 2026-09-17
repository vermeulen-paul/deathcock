# Countdown Clock & 8-Hour Workday Tracker

A modern .NET console application built with Spectre.Console and a companion static web app that render a live ticking countdown clock down to **November 30, 17:00** with precision metrics for both total calendar countdown and 8-hour workday countdown.

---

## Features HUE HUE2

- **Live Workday Countdown Clock**: The main clock ticks down the **remaining business work time** broken down into **8-hour Workdays**, **Hours**, **Minutes**, **Seconds**, and **Tenths**.
- **Target Handling**: Automatically targets November 30 at 17:00:00 of the current year (or next year if already passed).
- **Total Calendar Countdown**:
  - Full calendar days, hours, minutes, and seconds remaining.
  - Total days, hours, minutes, and seconds remaining metrics.
- **8-Hour Workday Countdown**:
  - **Workdays Remaining (8h/day)**: Active working hours (09:00–17:00, Mon–Fri) divided by 8 hours.
  - **Business Working Hours Left**: Exact business hours left between now and Nov 30 17:00.
  - **Mon-Fri Calendar Days Count**: Number of weekdays remaining.
  - **Standard 40h Work Weeks Left**: Standard full work weeks remaining.
  - **Raw 24/7 Hours in 8h Shifts**: Total remaining calendar hours divided by 8 hours.
- **Active Work Hours Badge**: Live status indicator (09:00 - 17:00 Mon-Fri).
- **Graceful Controls**: Press `Q`, `ESC`, or `Ctrl+C` to quit cleanly in the console app.

---

## How to Run

### Live Interactive Terminal App (.NET)
```bash
dotnet run
```

### Static Web Application
Open [index.html](index.html) in any web browser, or launch with a local server:
```bash
# In your terminal or by double clicking index.html
start index.html
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
