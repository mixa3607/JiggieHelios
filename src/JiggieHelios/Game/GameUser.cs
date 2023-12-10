namespace JiggieHelios.Game;

public class GameUser
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }

    public bool IsMe { get; set; }
}