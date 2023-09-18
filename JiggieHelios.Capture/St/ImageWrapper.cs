using System.Runtime.InteropServices;
using FFMpegCore.Pipes;

namespace JiggieHelios.Capture.St;

public class ImageWrapper : IVideoFrame
{
    private readonly Image<Rgba32> _image;

    public ImageWrapper(Image<Rgba32> image)
    {
        Width = image.Width;
        Height = image.Height;
        Format = "rgba";
        _image = image;
    }

    public void Serialize(Stream pipe)
    {
        var r = _image.DangerousTryGetSinglePixelMemory(out var mem);
        var span = MemoryMarshal.Cast<Rgba32, byte>(mem.Span);
        pipe.Write(span);
    }

    public Task SerializeAsync(Stream pipe, CancellationToken token)
    {
        var r = _image.DangerousTryGetSinglePixelMemory(out var mem);
        var span = MemoryMarshal.Cast<Rgba32, byte>(mem.Span);
        pipe.Write(span);
        return Task.CompletedTask;
    }

    public int Width { get; }
    public int Height { get; }
    public string Format { get; }
}