namespace TealBot.Objects;

public class Event
{
    public string key, name, event_code, start_date, end_date;
    public string city = "", state_prov = "", country = "";
    public int event_type, year;
    public District district = new();
    
    public Event(string key, string name, string event_code, int event_type, District district, string city, string state_prov, string country, string start_date, string end_date, int year)
    {
        this.key = key;
        this.name = name;
        this.event_code = event_code;
        this.event_type = event_type;
        this.district = district;
        this.city = city;
        this.state_prov = state_prov;
        this.country = country;
        this.start_date = start_date;
        this.end_date = end_date;
        this.year = year;
    }
}