namespace TealBot.Objects;

public class District
{
    public string? abbreviation, display_name, key;
    public int? year;

    public District(string abbreviation, string display_name, string key, int year)
    {
        this.abbreviation = abbreviation;
        this.display_name = display_name;
        this.key = key;
        this.year = year;
    }

    public District()
    {
        
    }
}