using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dack;

public sealed record PlayfieldRecommendation(
    string Playset,
    float Score,
    string Reason,
    string SuggestedConstruction
);

public sealed record PlayfieldProfile(
    float TextDensity,
    float HorizontalContinuity,
    float VerticalConnectivity,
    float GridRegularity,
    float OpenSpaceRatio,
    float BackgroundConfidence,
    float Destructibility,
    float ObjectRepetition,
    IReadOnlyList<PlayfieldRecommendation> Recommendations
)
{
    public string Summary()
    {
        string metrics =
            $"Text {TextDensity:P0}  •  horizontal {HorizontalContinuity:P0}  •  vertical {VerticalConnectivity:P0}\n"
            + $"Grid {GridRegularity:P0}  •  open space {OpenSpaceRatio:P0}  •  background {BackgroundConfidence:P0}";

        if (Recommendations.Count == 0)
            return metrics + "\n\nNo strong playset recommendation yet.";

        IEnumerable<string> ranked = Recommendations.Select((recommendation, index) =>
            $"{index + 1}. {recommendation.Playset}  {recommendation.Score:P0}\n"
            + $"   {recommendation.Reason}\n"
            + $"   Add: {recommendation.SuggestedConstruction}");
        return metrics + "\n\n" + string.Join("\n\n", ranked);
    }
}

public static class PlayfieldProfiler
{
    public static PlayfieldProfile Analyze(
        Image image,
        IReadOnlyList<Rect2> textPlatforms,
        IReadOnlyList<Rect2> textBricks,
        IReadOnlyList<Rect2> textWords,
        IReadOnlyList<Rect2> textLines,
        IReadOnlyList<Rect2> bonusAnchors)
    {
        float width = Mathf.Max(1f, image.GetWidth());
        float height = Mathf.Max(1f, image.GetHeight());
        float area = width * height;
        float lineArea = Mathf.Min(area, textLines.Sum(rect => rect.Size.X * Mathf.Max(rect.Size.Y, 3f)));
        float textDensity = Clamp01(lineArea / area * 5.5f);
        float horizontalContinuity = textLines.Count == 0
            ? 0f
            : Clamp01(textLines.Average(rect => rect.Size.X / width));
        float verticalSpan = textLines.Count < 2
            ? 0f
            : (textLines.Max(rect => rect.GetCenter().Y) - textLines.Min(rect => rect.GetCenter().Y)) / height;
        float verticalConnectivity = Clamp01(verticalSpan * 0.65f + Mathf.Min(textLines.Count / 28f, 1f) * 0.35f);
        float gridRegularity = EstimateGridRegularity(textWords, textLines);
        float openSpaceRatio = Clamp01(1f - Mathf.Min(0.92f, lineArea / area * 2.8f));
        float backgroundConfidence = EstimateBackgroundConfidence(image);
        float destructibility = Clamp01(
            Mathf.Min(textBricks.Count / 220f, 1f) * 0.55f
            + Mathf.Min(textWords.Count / 70f, 1f) * 0.30f
            + backgroundConfidence * 0.15f);
        float objectRepetition = Clamp01(
            EstimateRepeatedWidths(textBricks) * 0.70f
            + Mathf.Min(bonusAnchors.Count / 10f, 1f) * 0.30f);

        List<PlayfieldRecommendation> recommendations = BuildRecommendations(
            textDensity,
            horizontalContinuity,
            verticalConnectivity,
            gridRegularity,
            openSpaceRatio,
            backgroundConfidence,
            destructibility,
            objectRepetition);

        return new PlayfieldProfile(
            textDensity,
            horizontalContinuity,
            verticalConnectivity,
            gridRegularity,
            openSpaceRatio,
            backgroundConfidence,
            destructibility,
            objectRepetition,
            recommendations);
    }

    private static List<PlayfieldRecommendation> BuildRecommendations(
        float text,
        float horizontal,
        float vertical,
        float grid,
        float open,
        float background,
        float destructibility,
        float repetition)
    {
        List<PlayfieldRecommendation> candidates =
        [
            Recommend(
                "Side-View Platformer",
                horizontal * 0.38f + vertical * 0.18f + text * 0.22f + destructibility * 0.22f,
                Strongest((horizontal, "continuous horizontal text rows"), (vertical, "useful vertical progression"), (text, "dense document terrain")),
                vertical < 0.45f ? "ladders or elevators between isolated rows" : "start, goal, and a few traversal hazards"),
            Recommend(
                "Brickbat",
                destructibility * 0.42f + text * 0.26f + repetition * 0.18f + background * 0.14f,
                Strongest((destructibility, "many viable letter/word targets"), (text, "high text density"), (repetition, "repeated target-sized objects")),
                "paddle zone, launch point, and HUD-safe whitespace"),
            Recommend(
                "Pinball",
                open * 0.38f + background * 0.22f + (1f - text) * 0.17f + repetition * 0.13f + vertical * 0.10f,
                Strongest((open, "room for ball travel"), (background, "recoverable background regions"), (vertical, "a useful vertical table span")),
                "table boundary, drain, two flippers, and a plunger"),
            Recommend(
                "Maze / Snake",
                grid * 0.35f + text * 0.22f + vertical * 0.20f + repetition * 0.13f + open * 0.10f,
                Strongest((grid, "regular cell-like structure"), (text, "glyphs and words can become tunnels or goals"), (vertical, "routes can span the page")),
                "grid confirmation, start cell, goals, and blocked regions"),
            Recommend(
                "Tower Defense / Escort",
                grid * 0.30f + open * 0.24f + repetition * 0.18f + horizontal * 0.14f + vertical * 0.14f,
                Strongest((grid, "repeatable placement structure"), (open, "available build space"), (repetition, "candidate build points")),
                "route, entry/exit markers, defend zone, and build sockets"),
            Recommend(
                "Racing / Route",
                open * 0.30f + horizontal * 0.28f + vertical * 0.16f + background * 0.14f + grid * 0.12f,
                Strongest((horizontal, "long lateral routes"), (open, "steering room"), (grid, "route-aligned structure")),
                "track/path confirmation, starting line, direction, and checkpoints")
        ];

        return candidates
            .OrderByDescending(candidate => candidate.Score)
            .Take(3)
            .ToList();
    }

    private static PlayfieldRecommendation Recommend(string playset, float score, string reason, string additions)
    {
        return new PlayfieldRecommendation(playset, Clamp01(score), reason, additions);
    }

    private static string Strongest(params (float score, string reason)[] evidence)
    {
        return string.Join(" and ", evidence.OrderByDescending(item => item.score).Take(2).Select(item => item.reason)) + ".";
    }

    private static float EstimateGridRegularity(IReadOnlyList<Rect2> words, IReadOnlyList<Rect2> lines)
    {
        if (words.Count < 4 || lines.Count < 2)
            return 0f;

        float heightMean = words.Average(rect => rect.Size.Y);
        float heightDeviation = words.Average(rect => Mathf.Abs(rect.Size.Y - heightMean)) / Mathf.Max(heightMean, 1f);
        float heightConsistency = 1f - Clamp01(heightDeviation);
        float leftAlignment = EstimateCoordinateRepetition(words.Select(rect => rect.Position.X));
        float rowAlignment = EstimateCoordinateRepetition(words.Select(rect => rect.Position.Y));
        return Clamp01(heightConsistency * 0.30f + leftAlignment * 0.35f + rowAlignment * 0.35f);
    }

    private static float EstimateCoordinateRepetition(IEnumerable<float> coordinates)
    {
        int[] buckets = coordinates
            .GroupBy(value => Mathf.RoundToInt(value / 6f))
            .Select(group => group.Count())
            .ToArray();
        if (buckets.Length == 0)
            return 0f;
        int total = buckets.Sum();
        int repeated = buckets.Where(count => count > 1).Sum();
        return Clamp01(repeated / (float)Mathf.Max(total, 1));
    }

    private static float EstimateRepeatedWidths(IReadOnlyList<Rect2> regions)
    {
        if (regions.Count < 4)
            return 0f;
        int repeated = regions
            .GroupBy(rect => Mathf.RoundToInt(rect.Size.X / 3f))
            .Where(group => group.Count() > 2)
            .Sum(group => group.Count());
        return Clamp01(repeated / (float)regions.Count);
    }

    private static float EstimateBackgroundConfidence(Image image)
    {
        int step = Mathf.Max(8, Mathf.Min(image.GetWidth(), image.GetHeight()) / 80);
        double difference = 0;
        int samples = 0;
        for (int y = step; y < image.GetHeight(); y += step)
        {
            for (int x = step; x < image.GetWidth(); x += step)
            {
                Color current = image.GetPixel(x, y);
                Color left = image.GetPixel(x - step, y);
                Color above = image.GetPixel(x, y - step);
                difference += ColorDistance(current, left) + ColorDistance(current, above);
                samples += 2;
            }
        }

        if (samples == 0)
            return 0.5f;
        float averageDifference = (float)(difference / samples);
        return Clamp01(1f - averageDifference * 2.2f);
    }

    private static float ColorDistance(Color a, Color b)
    {
        return (Mathf.Abs(a.R - b.R) + Mathf.Abs(a.G - b.G) + Mathf.Abs(a.B - b.B)) / 3f;
    }

    private static float Clamp01(float value) => Mathf.Clamp(value, 0f, 1f);
}
