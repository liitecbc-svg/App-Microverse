using System.Collections.Generic;
using Microverse.Data;
using UnityEngine;

namespace Microverse.UI
{
    public static class BiologyVisualFactory
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite CreateBackground()
        {
            const string key = "microverse-background";
            if (Cache.ContainsKey(key))
            {
                return Cache[key];
            }

            const int width = 512;
            const int height = 768;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Random.InitState(401);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float vertical = (float)y / height;
                    float radial = Vector2.Distance(new Vector2(x / (float)width, vertical), new Vector2(0.52f, 0.58f));
                    Color color = Color.Lerp(MicroverseTheme.Background, new Color(0.02f, 0.08f, 0.17f, 1f), Mathf.Clamp01(1.1f - radial * 1.8f));
                    color += new Color(0f, 0.02f, 0.04f, 0f) * vertical;
                    texture.SetPixel(x, y, color);
                }
            }

            for (int i = 0; i < 130; i++)
            {
                int x = Random.Range(0, width);
                int y = Random.Range(0, height);
                int radius = Random.Range(1, 3);
                Color star = Color.Lerp(MicroverseTheme.Cyan, MicroverseTheme.Purple, Random.value);
                star.a = Random.Range(0.25f, 0.85f);
                DrawDisc(texture, x, y, radius, star, true);
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite CreateModelSprite(BiologicalModel model, int size = 512)
        {
            string key = model.Id + "-" + size;
            if (Cache.ContainsKey(key))
            {
                return Cache[key];
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Random.InitState(model.VisualSeed);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            Vector2 center = new Vector2(size * 0.5f, size * 0.52f);
            float rx = model.IsElongated ? size * 0.36f : size * 0.34f;
            float ry = model.IsElongated ? size * 0.19f : size * 0.34f;
            float angle = model.IsElongated ? Random.Range(-28f, 28f) : 0f;

            DrawEllipse(texture, center, rx, ry, angle, model.PrimaryColor, model.SecondaryColor);
            DrawRing(texture, center, rx, ry, angle, Color.white * 0.9f, 3);

            int organelles = model.IsElongated ? 16 : 24;
            for (int i = 0; i < organelles; i++)
            {
                Vector2 local = Random.insideUnitCircle * 0.78f;
                Vector2 point = Rotate(new Vector2(local.x * rx, local.y * ry), angle) + center;
                Color color = Color.Lerp(model.SecondaryColor, Color.white, Random.Range(0.05f, 0.45f));
                DrawDisc(texture, Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), Random.Range(9, 23), color, true);
            }

            Color nucleusColor = Color.Lerp(model.SecondaryColor, MicroverseTheme.Purple, 0.55f);
            DrawDisc(texture, Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y), Mathf.RoundToInt(size * 0.095f), nucleusColor, true);
            DrawDisc(texture, Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y), Mathf.RoundToInt(size * 0.048f), Color.Lerp(nucleusColor, Color.white, 0.25f), true);

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            Cache[key] = sprite;
            return sprite;
        }

        private static void DrawEllipse(Texture2D texture, Vector2 center, float rx, float ry, float angle, Color primary, Color secondary)
        {
            int width = texture.width;
            int height = texture.height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 rotated = Rotate(new Vector2(x - center.x, y - center.y), -angle);
                    float d = (rotated.x * rotated.x) / (rx * rx) + (rotated.y * rotated.y) / (ry * ry);
                    if (d <= 1f)
                    {
                        float edge = Mathf.SmoothStep(1f, 0.82f, d);
                        float glow = Mathf.Clamp01(1f - d);
                        Color color = Color.Lerp(primary, secondary, Mathf.PingPong(glow * 1.4f, 1f));
                        color = Color.Lerp(color, Color.white, Mathf.Clamp01((0.18f - Mathf.Abs(d - 0.78f)) * 2.2f));
                        color.a = Mathf.Lerp(0.48f, 0.95f, edge);
                        BlendPixel(texture, x, y, color);
                    }
                }
            }
        }

        private static void DrawRing(Texture2D texture, Vector2 center, float rx, float ry, float angle, Color color, int thickness)
        {
            int width = texture.width;
            int height = texture.height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 rotated = Rotate(new Vector2(x - center.x, y - center.y), -angle);
                    float d = (rotated.x * rotated.x) / (rx * rx) + (rotated.y * rotated.y) / (ry * ry);
                    if (Mathf.Abs(d - 1f) < thickness * 0.01f)
                    {
                        Color c = color;
                        c.a = Mathf.Clamp01(0.85f - Mathf.Abs(d - 1f) * 8f);
                        BlendPixel(texture, x, y, c);
                    }
                }
            }
        }

        private static void DrawDisc(Texture2D texture, int cx, int cy, int radius, Color color, bool glow)
        {
            int minX = Mathf.Max(0, cx - radius);
            int maxX = Mathf.Min(texture.width - 1, cx + radius);
            int minY = Mathf.Max(0, cy - radius);
            int maxY = Mathf.Min(texture.height - 1, cy + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) / radius;
                    if (d <= 1f)
                    {
                        float alpha = glow ? Mathf.Clamp01(1f - d * d) : 1f;
                        Color c = Color.Lerp(color, Color.white, Mathf.Clamp01(1f - d) * 0.28f);
                        c.a *= alpha;
                        BlendPixel(texture, x, y, c);
                    }
                }
            }
        }

        private static Vector2 Rotate(Vector2 value, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(value.x * cos - value.y * sin, value.x * sin + value.y * cos);
        }

        private static void BlendPixel(Texture2D texture, int x, int y, Color color)
        {
            Color previous = texture.GetPixel(x, y);
            Color blended = Color.Lerp(previous, color, color.a);
            blended.a = Mathf.Clamp01(previous.a + color.a);
            texture.SetPixel(x, y, blended);
        }

        public static System.Collections.IEnumerator DownloadPreviewTextureRoutine(string url, System.Action<Sprite> callback)
        {
            using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
                    if (texture != null)
                    {
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        callback?.Invoke(sprite);
                    }
                }
                else
                {
                    Debug.LogWarning("[Supabase] Failed to download preview image (" + url + "): " + request.error);
                }
            }
        }
    }
}
