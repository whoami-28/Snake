using System;

namespace Snake
{
    public enum GameMode { Normal, Teleport, NoAcceleration, Immortal }
    public enum RenderStyle { Normal, Retro }
    public enum FruitView { Apple, Banana, Grape, Melon, Peach, Pear }
    public static class GlobalSettings
    {
        public static int GameTimeInterval { get; set; } = 150;
        public static GameMode SelectedMode { get; set; } = GameMode.Normal;
        public static RenderStyle CurrentRenderStyle { get; set; } = RenderStyle.Normal;
        public static FruitView CurrentFruitStyle { get; set; } = FruitView.Apple;
        public static bool IsMusicEnabled { get; set; } = true;
    }
}