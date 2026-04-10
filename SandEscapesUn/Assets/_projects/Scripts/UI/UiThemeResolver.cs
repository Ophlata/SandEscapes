using UnityEngine;

namespace SandEscapes.UI
{
    /// <summary>
    /// Resolves sprites and fonts: component override → <see cref="GameUiTheme"/> field → neutral quad / legacy font.
    /// </summary>
    public static class UiThemeResolver
    {
        static Font _cachedLegacyFont;

        public static Sprite Sprite(Sprite componentOverride, GameUiTheme theme, Sprite themeField)
        {
            if (componentOverride != null)
                return componentOverride;
            if (themeField != null)
                return themeField;
            if (theme != null && theme.neutralQuadSprite != null)
                return theme.neutralQuadSprite;
            return UiBuiltInSprites.White;
        }

        public static Font Font(Font componentFont, GameUiTheme theme)
        {
            if (componentFont != null)
                return componentFont;
            if (theme != null && theme.uiFont != null)
                return theme.uiFont;
            return LegacyFont();
        }

        public static Font LegacyFont()
        {
            if (_cachedLegacyFont != null)
                return _cachedLegacyFont;
            _cachedLegacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_cachedLegacyFont == null)
                _cachedLegacyFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return _cachedLegacyFont;
        }
    }
}
