using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dack;

public sealed record CapturedPageFrame(
    Texture2D Texture,
    Image Image,
    Image OriginalImage,
    Vector2I PixelSize,
    string SourceName,
    Rect2[] TextPlatforms,
    Rect2[] TextBricks,
    Rect2[] TextWords,
    Rect2[] TextLines
);

public static class CapturedPageImportModule
{
    private static readonly string[] CandidatePaths =
    [
        "res://../Screenshot 2026-07-26 174658.png",
        "res://assets/captured/current-page.png"
    ];

    public static CapturedPageFrame? TryLoadDefault()
    {
        foreach (string resourcePath in CandidatePaths)
        {
            string filePath = ProjectSettings.GlobalizePath(resourcePath);
            if (!File.Exists(filePath))
                continue;

            Image source = Image.LoadFromFile(filePath);
            if (source.IsEmpty())
                continue;

            Image original = Image.CreateFromData(
                source.GetWidth(),
                source.GetHeight(),
                false,
                source.GetFormat(),
                source.GetData()
            );
            original.Convert(Image.Format.Rgba8);

            Image clone = Image.CreateFromData(
                original.GetWidth(),
                original.GetHeight(),
                false,
                original.GetFormat(),
                original.GetData()
            );
            clone.Convert(Image.Format.Rgba8);

            return new CapturedPageFrame(
                ImageTexture.CreateFromImage(clone),
                clone,
                original,
                new Vector2I(clone.GetWidth(), clone.GetHeight()),
                Path.GetFileName(filePath),
                DetectTextPlatforms(original),
                DetectTextBricks(original),
                DetectTextWords(original),
                DetectTextLines(original)
            );
        }

        return null;
    }

    private static Rect2[] DetectTextPlatforms(Image image)
    {
        List<Rect2> platforms = [];
        bool inBand = false;
        int bandStartY = 0;
        int bandMinX = image.GetWidth();
        int bandMaxX = 0;
        int bandDarkPixels = 0;

        for (int y = 0; y < image.GetHeight(); y++)
        {
            int rowMinX = image.GetWidth();
            int rowMaxX = 0;
            int rowDarkPixels = 0;

            for (int x = 0; x < image.GetWidth(); x++)
            {
                if (!IsDarkTextPixel(image.GetPixel(x, y)))
                    continue;

                rowDarkPixels++;
                rowMinX = Math.Min(rowMinX, x);
                rowMaxX = Math.Max(rowMaxX, x);
            }

            bool textRow = rowDarkPixels >= 18 && rowMaxX - rowMinX >= 24;
            if (textRow)
            {
                if (!inBand)
                {
                    inBand = true;
                    bandStartY = y;
                    bandMinX = rowMinX;
                    bandMaxX = rowMaxX;
                    bandDarkPixels = 0;
                }

                bandMinX = Math.Min(bandMinX, rowMinX);
                bandMaxX = Math.Max(bandMaxX, rowMaxX);
                bandDarkPixels += rowDarkPixels;
            }
            else if (inBand)
            {
                AddTextBand(platforms, bandStartY, y - 1, bandMinX, bandMaxX, bandDarkPixels);
                inBand = false;
            }
        }

        if (inBand)
            AddTextBand(platforms, bandStartY, image.GetHeight() - 1, bandMinX, bandMaxX, bandDarkPixels);

        return platforms.ToArray();
    }

    private static Rect2[] DetectTextBricks(Image image)
    {
        List<Rect2> bricks = [];
        bool inBand = false;
        int bandStartY = 0;
        int bandMinX = image.GetWidth();
        int bandMaxX = 0;
        int bandDarkPixels = 0;

        for (int y = 0; y < image.GetHeight(); y++)
        {
            int rowMinX = image.GetWidth();
            int rowMaxX = 0;
            int rowDarkPixels = 0;

            for (int x = 0; x < image.GetWidth(); x++)
            {
                if (!IsDarkTextPixel(image.GetPixel(x, y)))
                    continue;

                rowDarkPixels++;
                rowMinX = Math.Min(rowMinX, x);
                rowMaxX = Math.Max(rowMaxX, x);
            }

            bool textRow = rowDarkPixels >= 18 && rowMaxX - rowMinX >= 24;
            if (textRow)
            {
                if (!inBand)
                {
                    inBand = true;
                    bandStartY = y;
                    bandMinX = rowMinX;
                    bandMaxX = rowMaxX;
                    bandDarkPixels = 0;
                }

                bandMinX = Math.Min(bandMinX, rowMinX);
                bandMaxX = Math.Max(bandMaxX, rowMaxX);
                bandDarkPixels += rowDarkPixels;
            }
            else if (inBand)
            {
                AddTextBricks(bricks, image, bandStartY, y - 1, bandMinX, bandMaxX, bandDarkPixels);
                inBand = false;
            }
        }

        if (inBand)
            AddTextBricks(bricks, image, bandStartY, image.GetHeight() - 1, bandMinX, bandMaxX, bandDarkPixels);

        return bricks.ToArray();
    }

    private static Rect2[] DetectTextWords(Image image)
    {
        List<Rect2> words = [];
        bool inBand = false;
        int bandStartY = 0;
        int bandMinX = image.GetWidth();
        int bandMaxX = 0;
        int bandDarkPixels = 0;

        for (int y = 0; y < image.GetHeight(); y++)
        {
            int rowMinX = image.GetWidth();
            int rowMaxX = 0;
            int rowDarkPixels = 0;

            for (int x = 0; x < image.GetWidth(); x++)
            {
                if (!IsDarkTextPixel(image.GetPixel(x, y)))
                    continue;

                rowDarkPixels++;
                rowMinX = Math.Min(rowMinX, x);
                rowMaxX = Math.Max(rowMaxX, x);
            }

            bool textRow = rowDarkPixels >= 18 && rowMaxX - rowMinX >= 24;
            if (textRow)
            {
                if (!inBand)
                {
                    inBand = true;
                    bandStartY = y;
                    bandMinX = rowMinX;
                    bandMaxX = rowMaxX;
                    bandDarkPixels = 0;
                }

                bandMinX = Math.Min(bandMinX, rowMinX);
                bandMaxX = Math.Max(bandMaxX, rowMaxX);
                bandDarkPixels += rowDarkPixels;
            }
            else if (inBand)
            {
                AddTextRuns(words, image, bandStartY, y - 1, bandMinX, bandMaxX, bandDarkPixels, 3, 220);
                inBand = false;
            }
        }

        if (inBand)
            AddTextRuns(words, image, bandStartY, image.GetHeight() - 1, bandMinX, bandMaxX, bandDarkPixels, 3, 220);

        return words.ToArray();
    }

    private static Rect2[] DetectTextLines(Image image)
    {
        List<Rect2> lines = [];
        bool inBand = false;
        int bandStartY = 0;
        int bandMinX = image.GetWidth();
        int bandMaxX = 0;
        int bandDarkPixels = 0;

        for (int y = 0; y < image.GetHeight(); y++)
        {
            int rowMinX = image.GetWidth();
            int rowMaxX = 0;
            int rowDarkPixels = 0;

            for (int x = 0; x < image.GetWidth(); x++)
            {
                if (!IsDarkTextPixel(image.GetPixel(x, y)))
                    continue;

                rowDarkPixels++;
                rowMinX = Math.Min(rowMinX, x);
                rowMaxX = Math.Max(rowMaxX, x);
            }

            bool textRow = rowDarkPixels >= 18 && rowMaxX - rowMinX >= 24;
            if (textRow)
            {
                if (!inBand)
                {
                    inBand = true;
                    bandStartY = y;
                    bandMinX = rowMinX;
                    bandMaxX = rowMaxX;
                    bandDarkPixels = 0;
                }

                bandMinX = Math.Min(bandMinX, rowMinX);
                bandMaxX = Math.Max(bandMaxX, rowMaxX);
                bandDarkPixels += rowDarkPixels;
            }
            else if (inBand)
            {
                AddTextLine(lines, bandStartY, y - 1, bandMinX, bandMaxX, bandDarkPixels);
                inBand = false;
            }
        }

        if (inBand)
            AddTextLine(lines, bandStartY, image.GetHeight() - 1, bandMinX, bandMaxX, bandDarkPixels);

        return lines.ToArray();
    }

    private static void AddTextBand(List<Rect2> platforms, int startY, int endY, int minX, int maxX, int darkPixels)
    {
        int height = endY - startY + 1;
        int width = maxX - minX + 1;

        if (height is < 2 or > 32 || width < 30 || darkPixels < 30)
            return;

        platforms.Add(new Rect2(minX, startY + height - 1, width, 3));
    }

    private static void AddTextBricks(List<Rect2> bricks, Image image, int startY, int endY, int minX, int maxX, int darkPixels)
    {
        AddTextRuns(bricks, image, startY, endY, minX, maxX, darkPixels, 1, 42);
    }

    private static void AddTextRuns(List<Rect2> runs, Image image, int startY, int endY, int minX, int maxX, int darkPixels, int gapColumns, int maxRunWidth)
    {
        int height = endY - startY + 1;
        int width = maxX - minX + 1;

        if (height is < 2 or > 32 || width < 30 || darkPixels < 30)
            return;

        int runStartX = -1;
        int lastDarkX = -1;
        int emptyColumns = 0;

        for (int x = minX; x <= maxX; x++)
        {
            bool darkColumn = false;
            for (int y = startY; y <= endY; y++)
            {
                if (!IsDarkTextPixel(image.GetPixel(x, y)))
                    continue;

                darkColumn = true;
                break;
            }

            if (darkColumn)
            {
                if (runStartX < 0)
                    runStartX = x;

                lastDarkX = x;
                emptyColumns = 0;
            }
            else if (runStartX >= 0)
            {
                emptyColumns++;
                if (emptyColumns > gapColumns)
                {
                    AddTextRun(runs, image, runStartX, lastDarkX, startY, endY, maxRunWidth);
                    runStartX = -1;
                    lastDarkX = -1;
                    emptyColumns = 0;
                }
            }
        }

        if (runStartX >= 0)
            AddTextRun(runs, image, runStartX, lastDarkX, startY, endY, maxRunWidth);
    }

    private static void AddTextRun(List<Rect2> runs, Image image, int startX, int endX, int startY, int endY, int maxRunWidth)
    {
        int width = endX - startX + 1;
        if (width < 2 || width > maxRunWidth)
            return;

        int exactMinX = endX;
        int exactMaxX = startX;
        int exactMinY = endY;
        int exactMaxY = startY;
        int darkPixels = 0;

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                if (!IsDarkTextPixel(image.GetPixel(x, y)))
                    continue;

                exactMinX = Math.Min(exactMinX, x);
                exactMaxX = Math.Max(exactMaxX, x);
                exactMinY = Math.Min(exactMinY, y);
                exactMaxY = Math.Max(exactMaxY, y);
                darkPixels++;
            }
        }

        if (darkPixels == 0)
            return;

        runs.Add(new Rect2(
            exactMinX,
            exactMinY,
            Math.Max(1, exactMaxX - exactMinX + 1),
            Math.Max(1, exactMaxY - exactMinY + 1)
        ));
    }

    private static void AddTextLine(List<Rect2> lines, int startY, int endY, int minX, int maxX, int darkPixels)
    {
        int height = endY - startY + 1;
        int width = maxX - minX + 1;

        if (height is < 2 or > 32 || width < 30 || darkPixels < 30)
            return;

        lines.Add(new Rect2(minX - 1, startY - 1, width + 2, Math.Max(6, height + 2)));
    }

    private static bool IsDarkTextPixel(Color pixel)
    {
        if (pixel.A < 0.5f)
            return false;

        float luminance = pixel.R * 0.2126f + pixel.G * 0.7152f + pixel.B * 0.0722f;
        return luminance < 0.33f;
    }
}
