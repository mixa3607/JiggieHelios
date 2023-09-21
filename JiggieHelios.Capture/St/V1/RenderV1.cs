namespace JiggieHelios.Capture.St.V1;

public class RenderV1
{
    public List<RenderSetV1> Sets { get; set; } = new List<RenderSetV1>();
    public Image<Rgba32>? Canvas { get; set; }
}