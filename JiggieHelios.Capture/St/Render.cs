using System.Reflection;
using System.Runtime.CompilerServices;
using static JiggieHelios.Ws.Binary.Cmd.RotateBinaryCommand;

namespace JiggieHelios.Capture.St;

public class Render
{
    public List<RenderSet> Sets { get; set; } = new List<RenderSet>();
    public Image<Rgba32>? Canvas { get; set; }
}