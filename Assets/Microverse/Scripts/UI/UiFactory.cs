using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Microverse.UI
{
    public static class UiFactory
    {
        public static GameObject Panel(string name, Transform parent, Color color, int radius = 18)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = RoundedSpriteFactory.RoundedRect(color, radius);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            return go;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, string value, int size, FontStyles style, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        public static Button Button(string name, Transform parent, string label, UnityAction action, Color color, Color textColor, int fontSize = 24)
        {
            GameObject go = Panel(name, parent, color, 22);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<UnityEngine.UI.Image>();
            button.transition = Selectable.Transition.ColorTint;
            button.colors = ButtonColors(color);
            button.onClick.AddListener(action);

            TextMeshProUGUI text = Text("Label", go.transform, label, fontSize, FontStyles.Bold, textColor, TextAlignmentOptions.Center);
            Stretch(text.rectTransform, 14, 8);
            return button;
        }

        public static UnityEngine.UI.Image Image(string name, Transform parent, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = true;
            return image;
        }

        public static TMP_InputField Input(string name, Transform parent, string placeholder)
        {
            GameObject root = Panel(name, parent, new Color(0.03f, 0.08f, 0.17f, 0.95f), 22);
            TMP_InputField input = root.AddComponent<TMP_InputField>();
            input.targetGraphic = root.GetComponent<UnityEngine.UI.Image>();

            TextMeshProUGUI text = Text("Text", root.transform, string.Empty, 22, FontStyles.Normal, MicroverseTheme.Text);
            Stretch(text.rectTransform, 22, 6);
            text.margin = new Vector4(10, 0, 10, 0);

            TextMeshProUGUI placeholderText = Text("Placeholder", root.transform, placeholder, 22, FontStyles.Normal, MicroverseTheme.DimText);
            Stretch(placeholderText.rectTransform, 22, 6);
            placeholderText.margin = new Vector4(10, 0, 10, 0);

            input.textComponent = text;
            input.placeholder = placeholderText;
            return input;
        }

        public static void Stretch(RectTransform rect, float horizontal = 0f, float vertical = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(horizontal, vertical);
            rect.offsetMax = new Vector2(-horizontal, -vertical);
        }

        public static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static ColorBlock ButtonColors(Color normal)
        {
            return new ColorBlock
            {
                normalColor = normal,
                highlightedColor = Color.Lerp(normal, Color.white, 0.12f),
                pressedColor = Color.Lerp(normal, Color.black, 0.2f),
                selectedColor = Color.Lerp(normal, MicroverseTheme.Cyan, 0.12f),
                disabledColor = new Color(normal.r, normal.g, normal.b, 0.35f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };
        }
    }
}
