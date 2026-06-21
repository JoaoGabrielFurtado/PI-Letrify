
public class CriarMetaDto
{
    public string Tipo { get; set; } = string.Empty;
    public int ValorAlvo { get; set; }
    public string Periodicidade { get; set; } = "Diaria";
}

public class CheckInDto
{
    public int ValorRegistrado { get; set; }
}