﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

using LaquaiLib.Extensions;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a handler for a progress display on the current window's taskbar icon.
/// </summary>
public class TaskbarProgress
{
    private static TaskbarProgress? _instance;

    private readonly TaskbarItemInfo? _taskbar;

    /// <summary>
    /// Initializes a new <see cref="TaskbarProgress"/> with reference to a specified <paramref name="window"/>.
    /// </summary>
    /// <param name="window">The <see cref="Window"/> the taskbar icon of which is to display progress.</param>
    internal TaskbarProgress(Window window)
    {
        window.TaskbarItemInfo ??= new TaskbarItemInfo();
        _taskbar = window.TaskbarItemInfo;

        _taskbar.ProgressState = TaskbarItemProgressState.Normal;
        _taskbar.ProgressValue = 0;
    }

    /// <summary>
    /// Creates or directly returns an existing instance of <see cref="TaskbarProgress"/> with reference to a specified <paramref name="window"/>.
    /// </summary>
    /// <remarks>
    /// The application calling this method or using its return value must own the specified <paramref name="window"/>.
    /// </remarks>
    /// <param name="window">The <see cref="Window"/> the taskbar icon of which is to display progress.</param>
    /// <returns>A <see cref="TaskbarProgress"/> instance.</returns>
    public static TaskbarProgress GetInstance(Window window)
    {
        _instance ??= new TaskbarProgress(window);
        return _instance;
    }

    /// <summary>
    /// Creates or directly returns an existing instance of <see cref="TaskbarProgress"/> with reference to a <see cref="Window"/> identified by its <paramref name="pointer"/>.
    /// </summary>
    /// <remarks>
    /// The application calling this method or using its return value must own the <see cref="Window"/> pointed to by <paramref name="pointer"/>.
    /// </remarks>
    /// <param name="pointer">The pointer to the <see cref="Window"/> the taskbar icon of which is to display progress.</param>
    /// <returns>A <see cref="TaskbarProgress"/> instance.</returns>
    public static TaskbarProgress GetInstance(nint pointer)
    {
        if (HwndSource.FromHwnd(pointer).RootVisual is Window target)
        {
            _instance ??= new TaskbarProgress(target);
            return _instance;
        }
        else
        {
            throw new ArgumentException("Window pointer was 0 or resolved to null.", nameof(pointer));
        }
    }

    /// <summary>
    /// Creates or directly returns an existing instance of <see cref="TaskbarProgress"/> with reference to the <see cref="Window"/> of the calling process.
    /// </summary>
    /// <returns>A <see cref="TaskbarProgress"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the calling context's window handle was 0. Usually occurs when this method is called from a non-WPF context.</exception>
    public static TaskbarProgress GetInstance()
    {
        if (_instance == null)
        {
            try
            {
                if (HwndSource.FromHwnd(Process.GetCurrentProcess().MainWindowHandle).RootVisual is Window target)
                {
                    _instance ??= new TaskbarProgress(target);
                }
                else
                {
                    throw new InvalidOperationException("Cannot set taskbar progress information when calling from a non-WPF context (calling context's window handle was 0).");
                }
            }
            catch (ArgumentException argEx)
            {
                throw new InvalidOperationException("Cannot set taskbar progress information when calling from a non-WPF context (calling context's window handle was 0).", argEx);
            }
        }
        return _instance;
    }

    /// <summary>
    /// Resets the state of the taskbar progress bar and the <see cref="TaskbarProgress"/> instance that is kept internally. Only one <see cref="TaskbarProgress"/> instance may exist at a time.
    /// </summary>
    public static void ResetInstance()
    {
        _instance?.SetValue(1d);
        _instance?.SetState(TaskbarItemProgressState.None);

        _instance = null;
    }

    /// <summary>
    /// Sets the state of the taskbar progress visual.
    /// </summary>
    /// <param name="state">The new state of the taskbar progress visual.</param>
    /// <returns>The value of the <see cref="TaskbarItemInfo.ProgressState"/> property after the attempted set operation.</returns>
    public TaskbarItemProgressState SetState(TaskbarItemProgressState state)
    {
        _taskbar!.ProgressState = state;
        return _taskbar.ProgressState;
    }
    /// <summary>
    /// Sets a new value for the taskbar progress bar.
    /// </summary>
    /// <param name="percent">The new value for the taskbar progress bar. Must be between 0 and 100. Values outside this range are clamped.</param>
    /// <returns>The value of the <see cref="TaskbarItemInfo.ProgressValue"/> property after the attempted set operation.</returns>
    public double SetValue(int percent)
    {
        _taskbar!.ProgressValue = int.Clamp(percent, 0, 100) / 100d;
        return _taskbar.ProgressValue;
    }
    /// <summary>
    /// Sets a new value for the taskbar progress bar.
    /// </summary>
    /// <param name="value">The new value for the taskbar progress bar. Must be between 0 and 1. Values outside this range are clamped.</param>
    /// <returns>The value of the <see cref="TaskbarItemInfo.ProgressValue"/> property after the attempted set operation.</returns>
    public double SetValue(double value)
    {
        _taskbar!.ProgressValue = double.Clamp(value, 0, 1);
        return _taskbar.ProgressValue;
    }
    /// <summary>
    /// Increases the value of the taskbar progress bar by a specified amount.
    /// </summary>
    /// <param name="value">The amount to increase the value of the taskbar progress bar by. If the given value would cause the progress bar's value to fall outside range (0-1), the value is clamped.</param>
    /// <returns>The value of the <see cref="TaskbarItemInfo.ProgressValue"/> property after the attempted set operation.</returns>
    public double IncreaseValue(double value)
    {
        _taskbar!.ProgressValue = double.Clamp(_taskbar.ProgressValue + value, 0, 1);
        return _taskbar.ProgressValue;
    }
    /// <summary>
    /// Decreases the value of the taskbar progress bar by a specified amount.
    /// </summary>
    /// <param name="value">The amount to decrease the value of the taskbar progress bar by. If the given value would cause the progress bar's value to fall outside range (0-1), the value is clamped.</param>
    /// <returns>The value of the <see cref="TaskbarItemInfo.ProgressValue"/> property after the attempted set operation.</returns>
    public double DecreaseValue(double value)
    {
        _taskbar!.ProgressValue = double.Clamp(_taskbar.ProgressValue - value, 0, 1);
        return _taskbar.ProgressValue;
    }

    /// <summary>
    /// Animates towards a specified progress <paramref name="target"/> within a specified <paramref name="duration"/>.
    /// </summary>
    /// <param name="target">The value to animate progress towards.</param>
    /// <param name="duration">The amount of time for the animation to take in milliseconds. It may not be possible to obey this (exactly) in all cases.</param>
    /// <param name="steps">The number of steps to take to reach the target value. This is ignored if greater than <paramref name="duration"/>.</param>
    /// <returns>A <see cref="Task"/> that completes when the animation has finished.</returns>
    public async Task AnimateToValueAsync(double target, int duration, int steps = 100)
    {
        var from = _taskbar!.ProgressValue;
        var to = double.Clamp(target, 0, 1);

        // Can't use any of the animation classes because the animation just never starts
        steps = steps > duration ? duration : steps;
        var wait = TimeSpan.FromMilliseconds(duration) / steps;

        static double RoundToMultiple(double value, double multiple) => Math.Round(value / multiple) * multiple;

        var values =
            Miscellaneous.Range(from, to, (to - from) / steps)
                     .Select(d => RoundToMultiple(d, 0.05))
                     .ToArray();
        // Consolidate the values
        List<KeyValuePair<double, TimeSpan>> sequence = [];
        foreach (var value in values.Distinct())
        {
            sequence.Add(new KeyValuePair<double, TimeSpan>(value, wait * values.Count(d => d == value)));
        }

        foreach (var (value, time) in sequence)
        {
            _taskbar.ProgressValue = value;
            await Task.Delay(time);
        }

        _taskbar!.ProgressValue = to;
    }

    /// <summary>
    /// Gets the current value of the taskbar progress bar.
    /// </summary>
    /// <returns>The current value of the taskbar progress bar.</returns>
    public double GetValue() => _taskbar!.ProgressValue;
}
