using System.Collections.Generic;
using UnityEngine;

namespace Microverse.UI
{
    public static class RoundedSpriteFactory
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite RoundedRect(Color color, int radius = 16, int size = 64)
        {
            string key = "rect-" + ColorUtility.ToHtmlStringRGBA(color) + "-" + radius + "-" + size;
            if (Cache.ContainsKey(key))
            {
                return Cache[key];
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            float r = Mathf.Clamp(radius, 1, size / 2);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x < r ? r : (x > size - r ? size - r : x);
                    float py = y < r ? r : (y > size - r ? size - r : y);
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(px, py));
                    float alpha = Mathf.Clamp01(r + 0.5f - distance);
                    Color pixel = color;
                    pixel.a *= alpha;
                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite Circle(Color color, int size = 96)
        {
            string key = "circle-" + ColorUtility.ToHtmlStringRGBA(color) + "-" + size;
            if (Cache.ContainsKey(key))
            {
                return Cache[key];
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.48f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float normalized = Vector2.Distance(new Vector2(x, y), center) / radius;
                    float alpha = Mathf.Clamp01(1f - Mathf.SmoothStep(0.9f, 1f, normalized));
                    float light = Mathf.Lerp(1.25f, 0.55f, normalized);
                    Color pixel = color * light;
                    pixel.a = color.a * alpha;
                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite RoundedRectBorder(Color border, Color fill, float borderThickness, int radius = 16, int width = 128, int height = 128)
        {
            string key = "rect-border-" + ColorUtility.ToHtmlStringRGBA(border) + "-" + ColorUtility.ToHtmlStringRGBA(fill) + "-" + borderThickness + "-" + radius + "-" + width + "x" + height;
            if (Cache.ContainsKey(key))
            {
                return Cache[key];
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            float r = Mathf.Clamp(radius, 1, Mathf.Min(width, height) / 2f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float px = x < r ? r : (x > width - r ? width - r : x);
                    float py = y < r ? r : (y > height - r ? height - r : y);
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(px, py));
                    
                    float insideAlpha = Mathf.Clamp01(r + 0.5f - distance);
                    float borderInnerDistance = r - borderThickness;
                    float borderInnerAlpha = Mathf.Clamp01(borderInnerDistance + 0.5f - distance);
                    
                    Color pixel = Color.Lerp(fill, border, insideAlpha - borderInnerAlpha);
                    pixel.a = fill.a * borderInnerAlpha + border.a * (insideAlpha - borderInnerAlpha);
                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            Cache[key] = sprite;
            return sprite;
        }
    }
}
