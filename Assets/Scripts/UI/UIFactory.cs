namespace KobnexaRoyal
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Small uGUI construction helpers used by the lobby UI. No TextMeshPro dependency.</summary>
    public static class UIFactory
    {
        private static Font cachedFont;

        public static Font Font
        {
            get
            {
                if (cachedFont == null)
                {
                    cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return cachedFont;
            }
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            return rt;
        }

        public static RectTransform FullRect(Transform parent, string name)
        {
            return Rect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        public static GameObject Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color color, bool goldBorder)
        {
            RectTransform rt = Rect(parent, name, aMin, aMax, oMin, oMax);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            if (goldBorder)
            {
                Outline outline = rt.gameObject.AddComponent<Outline>();
                outline.effectColor = RoyalColors.RoyalGold;
                outline.effectDistance = new Vector2(1.5f, -1.5f);
            }
            return rt.gameObject;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color, TextAnchor anchor, bool bold)
        {
            RectTransform rt = FullRect(parent, name);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = Font; t.text = text; t.fontSize = size; t.color = color; t.alignment = anchor;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Text LabelIn(Transform parent, string name, string text, int size, Color color, TextAnchor anchor, bool bold, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
        {
            RectTransform rt = Rect(parent, name, aMin, aMax, oMin, oMax);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = Font; t.text = text; t.fontSize = size; t.color = color; t.alignment = anchor;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(Transform parent, string name, string label, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color normalColor, int fontSize)
        {
            RectTransform rt = Rect(parent, name, aMin, aMax, oMin, oMax);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = normalColor;
            Button btn = rt.gameObject.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = normalColor;
            cb.highlightedColor = Color.Lerp(normalColor, Color.white, 0.22f);
            cb.pressedColor = RoyalColors.RoyalGold;
            cb.selectedColor = Color.Lerp(normalColor, Color.white, 0.12f);
            cb.disabledColor = new Color(0.25f, 0.27f, 0.35f, 1f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            Text t = Label(rt, "Text", label, fontSize, RoyalColors.TextWhite, TextAnchor.MiddleCenter, true);
            t.raycastTarget = false;
            return btn;
        }

        public static void SetButtonText(Button btn, string label, Color color)
        {
            if (btn == null) return;
            Text t = btn.GetComponentInChildren<Text>();
            if (t != null) { t.text = label; t.color = color; }
        }

        /// <summary>Progress bar: background panel + fill whose right anchor = value01.</summary>
        public sealed class ProgressBar
        {
            public GameObject Root;
            public Image Fill;
            public void Set(float value01)
            {
                if (Fill == null) return;
                RectTransform rt = Fill.rectTransform;
                float v = Mathf.Clamp01(value01);
                rt.anchorMax = new Vector2(v, 1f);
            }
        }

        public static ProgressBar Bar(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color fillColor, float initial)
        {
            GameObject bg = Panel(parent, name, aMin, aMax, oMin, oMax, new Color(0.02f, 0.03f, 0.07f, 0.95f), false);
            RectTransform fillRt = Rect(bg.transform, "Fill", new Vector2(0f, 0f), new Vector2(Mathf.Clamp01(initial), 1f), new Vector2(1f, 1f), new Vector2(-1f, -1f));
            Image fill = fillRt.gameObject.AddComponent<Image>();
            fill.color = fillColor; fill.raycastTarget = false;
            ProgressBar bar = new ProgressBar(); bar.Root = bg; bar.Fill = fill;
            return bar;
        }

        public static Slider Slider(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, float initial)
        {
            RectTransform rt = Rect(parent, name, aMin, aMax, oMin, oMax);
            GameObject bg = Panel(rt, "Background", new Vector2(0f, 0.35f), new Vector2(1f, 0.65f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.07f, 1f), false);
            bg.GetComponent<Image>().raycastTarget = false;
            RectTransform fillArea = Rect(rt, "Fill Area", new Vector2(0f, 0.35f), new Vector2(1f, 0.65f), new Vector2(2f, 0f), new Vector2(-2f, 0f));
            RectTransform fillRt = Rect(fillArea, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image fillImg = fillRt.gameObject.AddComponent<Image>(); fillImg.color = RoyalColors.RoyalGold; fillImg.raycastTarget = false;
            RectTransform handleArea = Rect(rt, "Handle Slide Area", Vector2.zero, Vector2.one, new Vector2(6f, 0f), new Vector2(-6f, 0f));
            RectTransform handleRt = Rect(handleArea, "Handle", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(-10f, -2f), new Vector2(10f, 2f));
            Image handleImg = handleRt.gameObject.AddComponent<Image>(); handleImg.color = RoyalColors.RoyalGoldBright;
            Slider slider = rt.gameObject.AddComponent<Slider>();
            slider.fillRect = fillRt; slider.handleRect = handleRt; slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = Mathf.Clamp01(initial);
            return slider;
        }

        public static Toggle Toggle(Transform parent, string name, string label, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, bool initial)
        {
            RectTransform rt = Rect(parent, name, aMin, aMax, oMin, oMax);
            GameObject box = Panel(rt, "Box", new Vector2(0f, 0.1f), new Vector2(0f, 0.9f), new Vector2(0f, 0f), new Vector2(34f, 0f), RoyalColors.NavyPanelLight, true);
            GameObject check = Panel(box.transform, "Checkmark", new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f), Vector2.zero, Vector2.zero, RoyalColors.RoyalGold, false);
            check.GetComponent<Image>().raycastTarget = false;
            LabelIn(rt, "Label", label, 20, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, new Vector2(0f, 0f), Vector2.one, new Vector2(44f, 0f), Vector2.zero);
            Toggle toggle = rt.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = box.GetComponent<Image>();
            toggle.graphic = check.GetComponent<Image>();
            toggle.isOn = initial;
            return toggle;
        }

        public static Dropdown Dropdown(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, IList<string> options, int initial)
        {
            RectTransform rt = Rect(parent, name, aMin, aMax, oMin, oMax);
            Image img = rt.gameObject.AddComponent<Image>(); img.color = RoyalColors.NavyPanelLight;
            Dropdown dd = rt.gameObject.AddComponent<Dropdown>();
            Text caption = LabelIn(rt, "Label", string.Empty, 20, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-30f, 0f));
            dd.captionText = caption;
            LabelIn(rt, "Arrow", "v", 20, RoyalColors.RoyalGoldBright, TextAnchor.MiddleCenter, true, new Vector2(1f, 0f), Vector2.one, new Vector2(-28f, 0f), Vector2.zero);

            GameObject templateGo = new GameObject("Template", typeof(RectTransform));
            templateGo.transform.SetParent(rt, false);
            RectTransform templateRt = (RectTransform)templateGo.transform;
            templateRt.anchorMin = new Vector2(0f, 0f); templateRt.anchorMax = new Vector2(1f, 0f);
            templateRt.pivot = new Vector2(0.5f, 1f); templateRt.offsetMin = new Vector2(0f, -140f); templateRt.offsetMax = new Vector2(0f, 0f);
            Image templateImg = templateGo.AddComponent<Image>(); templateImg.color = RoyalColors.NavyPanel;
            ScrollRect scroll = templateGo.AddComponent<ScrollRect>();
            RectTransform viewport = Rect(templateGo.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image vpImg = viewport.gameObject.AddComponent<Image>(); vpImg.color = new Color(1f, 1f, 1f, 0.02f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            RectTransform content = Rect(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 36f));
            content.pivot = new Vector2(0.5f, 1f);
            GameObject itemGo = new GameObject("Item", typeof(RectTransform));
            itemGo.transform.SetParent(content, false);
            RectTransform itemRt = (RectTransform)itemGo.transform;
            itemRt.anchorMin = new Vector2(0f, 0.5f); itemRt.anchorMax = new Vector2(1f, 0.5f);
            itemRt.sizeDelta = new Vector2(0f, 34f);
            Image itemBg = itemGo.AddComponent<Image>(); itemBg.color = new Color(0.05f, 0.08f, 0.18f, 1f);
            Toggle itemToggle = itemGo.AddComponent<Toggle>(); itemToggle.targetGraphic = itemBg;
            GameObject itemCheck = Panel(itemGo.transform, "Item Checkmark", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(4f, 6f), new Vector2(26f, -6f), RoyalColors.RoyalGold, false);
            itemCheck.GetComponent<Image>().raycastTarget = false;
            itemToggle.graphic = itemCheck.GetComponent<Image>();
            Text itemLabel = LabelIn(itemGo.transform, "Item Label", string.Empty, 19, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, Vector2.zero, Vector2.one, new Vector2(34f, 0f), Vector2.zero);
            scroll.content = content; scroll.viewport = viewport;
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
            dd.template = templateRt; dd.itemText = itemLabel;
            templateGo.SetActive(false);

            dd.options.Clear();
            for (int i = 0; i < options.Count; i++) dd.options.Add(new Dropdown.OptionData(options[i]));
            dd.value = Mathf.Clamp(initial, 0, Mathf.Max(0, options.Count - 1));
            dd.RefreshShownValue();
            return dd;
        }

        /// <summary>Vertical scroll list. Returns the content RectTransform to add rows into.</summary>
        public static RectTransform ScrollList(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, float rowSpacing)
        {
            RectTransform rt = Rect(parent, name, aMin, aMax, oMin, oMax);
            Image bg = rt.gameObject.AddComponent<Image>(); bg.color = new Color(0.03f, 0.05f, 0.12f, 0.65f);
            ScrollRect scroll = rt.gameObject.AddComponent<ScrollRect>();
            RectTransform viewport = Rect(rt, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image vpImg = viewport.gameObject.AddComponent<Image>(); vpImg.color = new Color(1f, 1f, 1f, 0.02f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            RectTransform content = Rect(viewport, "Content", new Vector2(0f, 1f), Vector2.one, Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(0.5f, 1f);
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = rowSpacing; layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childControlWidth = true; layout.childControlHeight = false;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content; scroll.viewport = viewport;
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            return content;
        }

        public static GameObject Row(Transform content, string name, float height, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(content, false);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height; le.minHeight = height;
            Image img = go.AddComponent<Image>(); img.color = color;
            return go;
        }
    }
}
