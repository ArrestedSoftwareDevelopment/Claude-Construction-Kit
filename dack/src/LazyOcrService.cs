using Godot;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Dack;

public sealed class LazyOcrService
{
    private static readonly string[] TesseractCandidates =
    [
        @"C:\Program Files\Tesseract-OCR\tesseract.exe",
        @"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe",
        "tesseract"
    ];

    private readonly ConcurrentDictionary<string, string> _labels = [];
    private readonly ConcurrentDictionary<string, byte> _queued = [];
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _resolvedTesseractPath;
    private volatile bool _tesseractUnavailable;
    private int _activeJobs;

    public string StatusText
    {
        get
        {
            if (_tesseractUnavailable)
                return "OCR UNAVAILABLE";

            if (_activeJobs > 0)
                return "OCR READING";

            if (_labels.Count > 0)
                return $"OCR {_labels.Count}";

            if (_queued.Count > 0)
                return "OCR QUEUED";

            return "OCR LAZY";
        }
    }

    public bool TryGetLabel(Rect2 region, out string label)
    {
        return _labels.TryGetValue(GetKey(region), out label!);
    }

    public void QueueRegion(Rect2 region, Image sample)
    {
        if (_tesseractUnavailable)
            return;

        string key = GetKey(region);
        if (_labels.ContainsKey(key) || !_queued.TryAdd(key, 0))
            return;

        _ = Task.Run(() => RecognizeAsync(key, sample));
    }

    private async Task RecognizeAsync(string key, Image sample)
    {
        await _gate.WaitAsync();
        Interlocked.Increment(ref _activeJobs);
        string inputPath = Path.Combine(Path.GetTempPath(), $"dack-ocr-{Guid.NewGuid():N}.png");
        try
        {
            if (sample.SavePng(inputPath) != Error.Ok)
                return;

            string? tesseractPath = ResolveTesseractPath();
            if (tesseractPath is null)
            {
                _tesseractUnavailable = true;
                return;
            }

            ProcessStartInfo start = new(tesseractPath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            start.ArgumentList.Add(inputPath);
            start.ArgumentList.Add("stdout");
            start.ArgumentList.Add("--psm");
            start.ArgumentList.Add("8");
            start.ArgumentList.Add("-l");
            start.ArgumentList.Add("eng");

            using Process process = Process.Start(start)!;
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                return;

            string label = NormalizeLabel(output);
            if (label.Length > 0)
                _labels[key] = label;
        }
        catch (Win32Exception)
        {
            _tesseractUnavailable = true;
        }
        catch (InvalidOperationException)
        {
            _tesseractUnavailable = true;
        }
        finally
        {
            _queued.TryRemove(key, out _);
            Interlocked.Decrement(ref _activeJobs);
            _gate.Release();
            TryDelete(inputPath);
        }
    }

    private static string NormalizeLabel(string raw)
    {
        string text = Regex.Replace(raw.ToUpperInvariant(), @"[^A-Z0-9'-]+", "");
        return text.Length > 18 ? text[..18] : text;
    }

    private string? ResolveTesseractPath()
    {
        if (_resolvedTesseractPath is not null)
            return _resolvedTesseractPath;

        foreach (string candidate in TesseractCandidates)
        {
            if (candidate.Contains('\\') && !File.Exists(candidate))
                continue;

            _resolvedTesseractPath = candidate;
            return candidate;
        }

        return null;
    }

    private static string GetKey(Rect2 region)
    {
        return $"{Mathf.RoundToInt(region.Position.X)}:{Mathf.RoundToInt(region.Position.Y)}:{Mathf.RoundToInt(region.Size.X)}:{Mathf.RoundToInt(region.Size.Y)}";
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }
}
