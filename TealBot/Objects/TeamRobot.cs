namespace TealBot.Objects;

public class TeamRobot(string key, string robot_name, string team_key, int year)
{
    public string key { get; } = key;
    public string robot_name { get; } = robot_name;
    public string team_key { get; } = team_key;
    public int year { get; } = year;
}