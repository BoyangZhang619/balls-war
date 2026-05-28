namespace BallsWar.App.Rendering;

public enum Language { Chinese, English }

public static class Strings
{
    public static Language Current = Language.Chinese;

    private static readonly Dictionary<string, string[]> _dict = new()
    {
        ["title"] = ["弹球战争", "Balls War"],
        ["subtitle"] = ["实时自动战斗模拟", "Real-Time Auto-Battle Strategy"],
        ["start_game"] = ["开始游戏", "START GAME"],
        ["settings"] = ["设置", "SETTINGS"],
        ["quit"] = ["退出程序", "QUIT"],
        ["back"] = ["返回", "BACK"],
        ["start"] = ["开始", "START"],
        ["paused"] = ["已暂停", "PAUSED"],
        ["continue"] = ["继续模拟", "Continue"],
        ["return_home"] = ["返回主页（不保存）", "Return to Home (no save)"],
        ["game_over"] = ["游戏结束", "GAME OVER"],
        ["wins"] = ["胜出！", " Wins!"],
        ["speed"] = ["速度", "Speed"],
        ["restart_hint"] = ["按 R 重新开始", "Press R to Restart"],
        ["configure"] = ["配置参数并开始游戏", "Configure and start the game"],
        ["click_edit"] = ["点击数值编辑，或方向键操作", "Click a value to type, or use arrow keys"],

        // Setup labels
        ["factions"] = ["阵营数", "Factions"],
        ["balls_per_arena"] = ["每阵营球数", "Balls per Arena"],
        ["value_cap"] = ["数值上限", "Value Cap"],
        ["grid_size"] = ["网格大小", "Grid Size"],
        ["starting_health"] = ["初始生命", "Starting Health"],
        ["shotgun_pellets"] = ["霰弹基数", "Shotgun Pellets"],
        ["spread_angle"] = ["散布角度", "Spread Angle"],
        ["pellet_speed"] = ["弹丸速度", "Pellet Speed"],
        ["bounces"] = ["反弹次数", "Bounces"],
        ["firing_rotation"] = ["炮台转速", "Firing Rotation"],
        ["bar_ratio"] = ["底部比例(S:B:H)", "Bar Ratio (S:B:H)"],

        // Area A labels
        ["shotgun_label"] = ["霰弹", "SHOTGUN"],
        ["shield_label"] = ["护盾", "SHIELD"],
        ["bigball_label"] = ["大球", "BIGBALL"],

        // Bottom bar
        ["nav_hint"] = ["[↑↓]导航 [←→]修改 [Enter]开始 [ESC]返回", "[Up/Down] Nav [Left/Right] Chg [Enter] Start [ESC] Back"],
        ["help_bar"] = ["[Space]暂停 [1-6]速度 [Q/E]加减 [ESC]暂停菜单", "[Space] Pause [1-6] Speed [Q/E]+/- [ESC] Menu"],
        ["colors"] = ["颜色", "Colors"],
        ["language"] = ["语言", "Language"],
    };

    public static string Get(string key)
    {
        if (_dict.TryGetValue(key, out var vals))
            return vals[Current == Language.Chinese ? 0 : 1];
        return key;
    }

    // Format faction name with localized prefix
    public static string FactionName(int i) => Current == Language.Chinese ? $"阵营{i + 1}" : $"F{i + 1}";
}
