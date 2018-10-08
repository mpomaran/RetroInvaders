using Microsoft.Xna.Framework;

namespace MPP.game
{
    class Constans
    {
        /*
        public static Color MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR = new Color(22f / 255f, 29f / 255f, 77f / 255f, 0.5f);
        public static Color MENU_SELECTED_BUTTON_BACKGROUND_COLOR = new Color(200f / 255f, 200f / 255f, 227f / 255f, 0.5f);
        public static Color MENU_ACTIVE_TEXT_COLOR = Color.White;
        public static Color MENU_NON_ACTIVE_TEXT_COLOR = new Color(1f, 1f, 1f, 0.5f);
        public static string MENU_FONT_NAME = "MainMenuFont";
         */
        public static Color MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR = new Color(50f / 255f, 40f / 255f, 3f / 255f, 0.5f);
        public static Color MENU_SELECTED_BUTTON_BACKGROUND_COLOR = new Color(214f / 255f, 157f / 255f, 3f / 255f, 0.5f);
        public static Color MAIN_SPRITE_COLOR = new Color(250f / 255f, 230f / 255f, 100f / 255f, 1f);
        public static Color MAIN_MISSLE_COLOR = new Color(250f / 255f, 230f / 255f, 200f / 255f, 1f);
        public static Color MAIN_EXPLOSION_COLOR = MAIN_SPRITE_COLOR;
        public static Color MAIN_PLAYER_EXPLOSION_COLOR = MAIN_SPRITE_COLOR;
        public static Color MENU_ACTIVE_TEXT_COLOR = Color.White;
        public static Color MENU_NON_ACTIVE_TEXT_COLOR = new Color(1f, 1f, 1f, 0.5f);
        public static string MENU_FONT_NAME = "MainMenuFont";
        public const int STANDARD_BUTTON_WIDTH = 480;
        public const int STANDARD_BUTTON_HEIGHT = 100;

        /* TODO: dobrac palete kolorow */
        public static readonly Color[] BATTLESCREEN_BACKGROUND_COLORS = { new Color(0.8f, 0.3f, 03f), new Color(0.3f, 0.9f, 0.5f), new Color(0.1f, 0.8f, 0.3f), new Color(0.1f, 0.3f, 0.8f), new Color(0.8f, 0.3f, 0.8f), 
                                                                        new Color (0.9f, 0.1f, 0.5f), new Color (0.8f, 0.3f, 0.4f), new Color (0.3f, 0.7f, 0.3f), 
                                                                        new Color (0.8f, 0.8f, 0.3f), new Color (0.1f, 0.1f, 0.9f),};
    
        public const int ENEMY_ROWS = 5;
        public const int ENEMY_COLS = 5;

    }
}
