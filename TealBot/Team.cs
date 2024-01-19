namespace TealBot;

public class Team
{
    public int team_number { get; }
    public string key, nickname, name, city, state_prov, country;
    
    public Team(string key, int team_number, string name, string nickname="", string city="", string state_prov="", string country="")
    {
        this.key = key;
        this.team_number = team_number;
        this.nickname = nickname;
        this.name = name;
        this.city = city;
        this.state_prov = state_prov;
        this.country = country;
    }

    public static Team? DeserializeTeamJson(string json)
    {
        return JsonConvert.DeserializeObject<Team>(json);
    }
}