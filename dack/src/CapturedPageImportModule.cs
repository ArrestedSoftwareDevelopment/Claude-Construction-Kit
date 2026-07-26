using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dack;

public sealed record CapturedPageFrame(
    Texture2D Texture,
    Vector2I PixelSize,
    string SourceName,
    Rect2[] TextPlatforms
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

            Image clone = Image.CreateFromData(
                source.GetWidth(),
                source.GetHeight(),
                false,
                source.GetFormat(),
                source.GetData()
            );
            clone.Convert(Image.Format.Rgba8);

            return new CapturedPageFrame(
                ImageTexture.CreateFromImage(clone),
                new Vector2I(clone.GetWidth(), clone.GetHeight()),
                Path.GetFileName(filePath),
                DetectTextPlatforms(clone)
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

    private static void AddTextBand(List<Rect2> platforms, int startY, int endY, int minX, int maxX, int darkPixels)
    {
        int height = endY - startY + 1;
        int width = maxX - minX + 1;

        if (height is < 2 or > 32 || width < 30 || darkPixels < 30)
            return;

        platforms.Add(new Rect2(minX, startY + height - 1, width, 3));
    }

    private static bool IsDarkTextPixel(Color pixel)
    {
        if (pixel.A < 0.5f)
            return false;

        float luminance = pixel.R * 0.2126f + pixel.G * 0.7152f + pixel.B * 0.0722f;
        return luminance < 0.33f;
    }
}
