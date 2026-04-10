using UnityEngine;

namespace SandEscapes.UI
{
    /// <summary>
    /// Single cached 1×1 white sprite used when no custom graphic is assigned.
    /// </summary>
    public static class UiBuiltInSprites
    {
        static Sprite _white;

        public static Sprite White
        {
            get
            {
                if (_white != null)
                    return _white;
                var tex = Texture2D.whiteTexture;
                _white = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                return _white;
            }
        }
    }
}
