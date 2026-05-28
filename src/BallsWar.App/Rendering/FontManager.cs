using Raylib_cs;

namespace BallsWar.App.Rendering;

public static class FontManager
{
    private static Font _font;
    private static bool _loaded;

    public static void Load()
    {
        string[] names = { "SourceHanSansSC-Regular.otf", "SourceHanSansSC-Medium.otf" };
        string[] dirs =
        {
            AppDomain.CurrentDomain.BaseDirectory,
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."),
            "."
        };

        foreach (var dir in dirs)
        {
            foreach (var name in names)
            {
                string path = System.IO.Path.Combine(dir, name);
                if (System.IO.File.Exists(path))
                {
                    // Only load codepoints actually used in our UI strings
                    var used = CollectUsedCodepoints();
                    _font = Raylib.LoadFontEx(path, 32, used, used.Length);
                    _loaded = true;
                    return;
                }
            }
        }
    }

    /// Collect all unique codepoints from all localizable strings.
    private static int[] CollectUsedCodepoints()
    {
        var set = new HashSet<int>();
        // ASCII
        for (int cp = 32; cp <= 126; cp++) set.Add(cp);

        // Chinese chars from Strings
        string[] allText =
        {
            "弹球战争实时自动战斗模拟开始游戏设置退出程序返回开始已暂停继续模拟返回主页不保存",
            "游戏结束胜出速度按重新开始配置参数并开始游戏点击数值编辑或方向键操作",
            "阵营数每阵球数数值上限网格大小初始生命霰弹基数散布角度弹丸速度反弹次数炮台转速底部比例",
            "霰弹护盾大球颜色语言导航修改暂停菜单加减",
            "中文简体繁体"
        };
        foreach (var s in allText)
            foreach (char c in s)
                set.Add(c);

        return set.ToArray();
    }

    public static unsafe void DrawStr(string text, int x, int y, int fontSize, Color color)
    {
        if (!_loaded) { Raylib.DrawText(text, x, y, fontSize, color); return; }
        Raylib.DrawTextEx(_font, text, new System.Numerics.Vector2(x, y), fontSize, 1, color);
    }

    public static int MeasureStr(string text, int fontSize)
    {
        if (!_loaded) return Raylib.MeasureText(text, fontSize);
        var size = Raylib.MeasureTextEx(_font, text, fontSize, 1);
        return (int)size.X;
    }
}
