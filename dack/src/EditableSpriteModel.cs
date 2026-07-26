using Godot;
using System;
using System.IO;

namespace Dack;

/// <summary>
/// One editable pixel model may be bound to several actors. Editing the model
/// updates every bound actor; duplicating it is the explicit "fork" operation.
/// </summary>
public sealed class EditableSpriteModel
{
    public const int CanvasSize = 32;
    public const string SourceSheetPath =
        "res://assets/third_party/stickman-pack-v0.1/thin-idle-sheet.png";

    public Image Pixels { get; }
    public ImageTexture Texture { get; }
    public event Action? Changed;

    private EditableSpriteModel(Image pixels)
    {
        Pixels = pixels;
        Texture = ImageTexture.CreateFromImage(Pixels);
    }

    public static EditableSpriteModel CreateInitial(out bool loadedThirdPartyAsset)
    {
        loadedThirdPartyAsset = false;
        string localPath = ProjectSettings.GlobalizePath(SourceSheetPath);

        if (File.Exists(localPath))
        {
            Image sheet = Image.LoadFromFile(localPath);
            if (!sheet.IsEmpty() && sheet.GetHeight() > 0 && sheet.GetWidth() >= sheet.GetHeight())
            {
                int frameSize = sheet.GetHeight();
                Image firstFrame = sheet.GetRegion(new Rect2I(0, 0, frameSize, frameSize));
                firstFrame.Convert(Image.Format.Rgba8);
                MakeNearWhiteTransparent(firstFrame);
                firstFrame.Resize(CanvasSize, CanvasSize, Image.Interpolation.Nearest);
                loadedThirdPartyAsset = true;
                return new EditableSpriteModel(firstFrame);
            }
        }

        return CreateProcedural();
    }

    public static EditableSpriteModel CreateProcedural()
    {
        Image image = Image.CreateEmpty(
            CanvasSize,
            CanvasSize,
            false,
            Image.Format.Rgba8
        );
        image.Fill(Colors.Transparent);
        DrawProceduralFigure(image);
        return new EditableSpriteModel(image);
    }

    public EditableSpriteModel Fork()
    {
        Image clone = Image.CreateFromData(
            Pixels.GetWidth(),
            Pixels.GetHeight(),
            false,
            Pixels.GetFormat(),
            Pixels.GetData()
        );
        return new EditableSpriteModel(clone);
    }

    public void SetPixel(Vector2I cell, Color color)
    {
        if (cell.X < 0 || cell.Y < 0 || cell.X >= CanvasSize || cell.Y >= CanvasSize)
            return;

        if (Pixels.GetPixelv(cell).IsEqualApprox(color))
            return;

        Pixels.SetPixelv(cell, color);
        PublishChange();
    }

    public void ResetToProcedural()
    {
        Pixels.Fill(Colors.Transparent);
        DrawProceduralFigure(Pixels);
        PublishChange();
    }

    public Rect2 GetOpaqueBounds(int padding = 1)
    {
        int minX = CanvasSize;
        int minY = CanvasSize;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < CanvasSize; y++)
        {
            for (int x = 0; x < CanvasSize; x++)
            {
                if (Pixels.GetPixel(x, y).A <= 0.01f)
                    continue;

                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        if (maxX < minX || maxY < minY)
            return new Rect2(Vector2.Zero, new Vector2(CanvasSize, CanvasSize));

        minX = Math.Max(0, minX - padding);
        minY = Math.Max(0, minY - padding);
        maxX = Math.Min(CanvasSize - 1, maxX + padding);
        maxY = Math.Min(CanvasSize - 1, maxY + padding);

        return new Rect2(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private void PublishChange()
    {
        Texture.Update(Pixels);
        Changed?.Invoke();
    }

    private static void MakeNearWhiteTransparent(Image image)
    {
        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                Color pixel = image.GetPixel(x, y);
                if (pixel.R > 0.95f && pixel.G > 0.95f && pixel.B > 0.95f)
                    image.SetPixel(x, y, Colors.Transparent);
            }
        }
    }

    private static void DrawProceduralFigure(Image image)
    {
        Color ink = new("#181A1F");

        for (int angle = 0; angle < 360; angle += 12)
        {
            float radians = Mathf.DegToRad(angle);
            Plot(image, new Vector2I(
                16 + Mathf.RoundToInt(Mathf.Cos(radians) * 4f),
                7 + Mathf.RoundToInt(Mathf.Sin(radians) * 4f)
            ), ink);
        }

        DrawLine(image, new Vector2I(16, 11), new Vector2I(16, 21), ink);
        DrawLine(image, new Vector2I(16, 14), new Vector2I(9, 18), ink);
        DrawLine(image, new Vector2I(16, 14), new Vector2I(23, 17), ink);
        DrawLine(image, new Vector2I(16, 21), new Vector2I(10, 29), ink);
        DrawLine(image, new Vector2I(16, 21), new Vector2I(23, 29), ink);
    }

    private static void DrawLine(Image image, Vector2I start, Vector2I end, Color color)
    {
        int x = start.X;
        int y = start.Y;
        int dx = Math.Abs(end.X - start.X);
        int dy = -Math.Abs(end.Y - start.Y);
        int stepX = start.X < end.X ? 1 : -1;
        int stepY = start.Y < end.Y ? 1 : -1;
        int error = dx + dy;

        while (true)
        {
            Plot(image, new Vector2I(x, y), color);
            if (x == end.X && y == end.Y)
                break;

            int doubled = 2 * error;
            if (doubled >= dy)
            {
                error += dy;
                x += stepX;
            }
            if (doubled <= dx)
            {
                error += dx;
                y += stepY;
            }
        }
    }

    private static void Plot(Image image, Vector2I point, Color color)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= CanvasSize || point.Y >= CanvasSize)
            return;

        image.SetPixelv(point, color);
    }
}
