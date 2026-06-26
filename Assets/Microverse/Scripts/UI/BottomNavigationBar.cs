using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Microverse.UI
{
    public class BottomNavigationBar
    {
        private readonly GameObject root;
        public GameObject Root => root;
        private readonly Action<string> onSelect;
        private readonly Func<string, string> getText;
        private readonly Dictionary<string, TextMeshProUGUI> labels = new Dictionary<string, TextMeshProUGUI>();
        private string selected = "home";

        public BottomNavigationBar(Transform parent, Action<string> onSelect, Func<string, string> getText)
        {
            this.onSelect = onSelect;
            this.getText = getText;
            root = UiFactory.Panel("BottomNavigation", parent, new Color(0.01f, 0.04f, 0.10f, 0.96f), 28);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.offsetMin = new Vector2(28f, 18f);
            rect.offsetMax = new Vector2(-28f, 142f);

            HorizontalLayoutGroup layout = root.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            AddTab("home", "nav.home");
            AddTab("profile", "nav.profile");
        }

        public void SetSelected(string id)
        {
            selected = id;
            for (int i = 0; i < root.transform.childCount; i++)
            {
                Transform child = root.transform.GetChild(i);
                bool active = child.name == id;
                Image image = child.GetComponent<Image>();
                image.color = active ? new Color(0.0f, 0.24f, 0.48f, 0.95f) : new Color(0f, 0f, 0f, 0f);
                TextMeshProUGUI label = child.GetComponentInChildren<TextMeshProUGUI>();
                label.color = active ? MicroverseTheme.Cyan : MicroverseTheme.Text;
            }
        }

        public void RefreshLabels()
        {
            foreach (KeyValuePair<string, TextMeshProUGUI> entry in labels)
            {
                entry.Value.text = getText(entry.Key);
            }
        }

        private void AddTab(string id, string textKey)
        {
            Button button = UiFactory.Button(id, root.transform, getText(textKey), () =>
            {
                selected = id;
                SetSelected(id);
                onSelect(id);
            }, new Color(0f, 0f, 0f, 0f), MicroverseTheme.Text, id == "scan" ? 24 : 20);
            button.name = id;
            labels[textKey] = button.GetComponentInChildren<TextMeshProUGUI>();
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = id == "scan" ? new Vector2(190f, 110f) : new Vector2(160f, 96f);
        }
    }
}
