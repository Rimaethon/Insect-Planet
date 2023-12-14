using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace M_Studio.Colourful_Hierarchy_Category_GameObject.Editor
{
    public class TextToTextMeshPro : UnityEditor.Editor
    {
        private static bool _recursive;
        private static bool _applyOnDisabled;
        private static bool _removeEffectComponents;

        [MenuItem("Tools/Text To TextMeshPro", false, 4000)]
        private static void DoIt()
        {
            if (TMP_Settings.defaultFontAsset == null)
            {
                EditorUtility.DisplayDialog("ERROR!", "Assign a default font asset in project settings!", "OK", "");
                return;
            }

            _recursive = EditorUtility.DisplayDialog("Text To TextMeshPro",
                "Process all the children of the selected objects recursively?", "Yes", "No");
            if (_recursive)
                _applyOnDisabled =
                    EditorUtility.DisplayDialog("Text To TextMeshPro", "Process disabled objects too?", "Yes", "No");

            _removeEffectComponents = EditorUtility.DisplayDialog("Text To TextMeshPro",
                "Remove obsolete component effects Shadow and Outline?\n\nThis components have no effect on TextMeshPro and can be safely removed.\nTo apply shadows and outlines on TextMeshPro components, use different materials instead.",
                "Yes", "No");

            foreach (var gameObject in Selection.gameObjects)
                if (_recursive)
                {
                    var childs = gameObject.GetComponentsInChildren<Transform>(_applyOnDisabled);
                    foreach (var ch in childs)
                        ConvertTextToTextMeshPro(ch.gameObject);
                }
                else
                {
                    ConvertTextToTextMeshPro(gameObject);
                }
        }

        private static void ConvertTextToTextMeshPro(GameObject target)
        {
            if (_removeEffectComponents)
            {
                var sha = target.GetComponents<Shadow>();
                foreach (var cmp in sha) Undo.DestroyObjectImmediate(cmp);
                var outl = target.GetComponents<Outline>();
                foreach (var cmp in outl) Undo.DestroyObjectImmediate(cmp);
            }

            var uiText = target.GetComponent<Text>();
            if (uiText == null) return;


            var settings = GetTextMeshProSettings(uiText);

            Undo.RecordObject(target, "Text To TextMeshPro");
            Undo.DestroyObjectImmediate(uiText);

            var tmp = target.AddComponent<TextMeshProUGUI>();
            tmp.enabled = settings.Enabled;
            tmp.fontStyle = settings.FontStyle;
            tmp.fontSize = settings.FontSize;
            tmp.fontSizeMin = settings.FontSizeMin;
            tmp.fontSizeMax = settings.FontSizeMax;
            tmp.lineSpacing = settings.LineSpacing;
            tmp.richText = settings.EnableRichText;
            tmp.enableAutoSizing = settings.EnableAutoSizing;
            tmp.alignment = settings.TextAlignmentOptions;
            tmp.enableWordWrapping = settings.WrappingEnabled;
            tmp.overflowMode = settings.TextOverflowModes;
            tmp.text = settings.Text;
            tmp.color = settings.Color;
            tmp.raycastTarget = settings.RayCastTarget;

            Debug.Log(target.name + " converted to TextMeshProUGUI");
        }

        private static TextMeshProSettings GetTextMeshProSettings(Text uiText)
        {
            if (uiText == null)
            {
                EditorUtility.DisplayDialog("ERROR!", "You must select a Unity UI Text Object to convert.", "OK", "");
                return null;
            }

            return new TextMeshProSettings
            {
                Enabled = uiText.enabled,
                FontStyle = FontStyleToFontStyles(uiText.fontStyle),
                FontSize = uiText.fontSize,
                FontSizeMin = uiText.resizeTextMinSize,
                FontSizeMax = uiText.resizeTextMaxSize,
                LineSpacing = uiText.lineSpacing,
                EnableRichText = uiText.supportRichText,
                EnableAutoSizing = uiText.resizeTextForBestFit,
                TextAlignmentOptions = TextAnchorToTextAlignmentOptions(uiText.alignment),
                WrappingEnabled = HorizontalWrapModeToBool(uiText.horizontalOverflow),
                TextOverflowModes = VerticalWrapModeToTextOverflowModes(uiText.verticalOverflow),
                Text = uiText.text,
                Color = uiText.color,
                RayCastTarget = uiText.raycastTarget
            };
        }

        private static bool HorizontalWrapModeToBool(HorizontalWrapMode overflow)
        {
            return overflow == HorizontalWrapMode.Wrap;
        }

        private static TextOverflowModes VerticalWrapModeToTextOverflowModes(VerticalWrapMode verticalOverflow)
        {
            return verticalOverflow == VerticalWrapMode.Truncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
        }

        private static FontStyles FontStyleToFontStyles(FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case FontStyle.Normal:
                    return FontStyles.Normal;

                case FontStyle.Bold:
                    return FontStyles.Bold;

                case FontStyle.Italic:
                    return FontStyles.Italic;

                case FontStyle.BoldAndItalic:
                    return FontStyles.Bold | FontStyles.Italic;
            }

            Debug.LogWarning("Unhandled font style " + fontStyle);
            return FontStyles.Normal;
        }

        private static TextAlignmentOptions TextAnchorToTextAlignmentOptions(TextAnchor textAnchor)
        {
            switch (textAnchor)
            {
                case TextAnchor.UpperLeft:
                    return TextAlignmentOptions.TopLeft;

                case TextAnchor.UpperCenter:
                    return TextAlignmentOptions.Top;

                case TextAnchor.UpperRight:
                    return TextAlignmentOptions.TopRight;

                case TextAnchor.MiddleLeft:
                    return TextAlignmentOptions.Left;

                case TextAnchor.MiddleCenter:
                    return TextAlignmentOptions.Center;

                case TextAnchor.MiddleRight:
                    return TextAlignmentOptions.Right;

                case TextAnchor.LowerLeft:
                    return TextAlignmentOptions.BottomLeft;

                case TextAnchor.LowerCenter:
                    return TextAlignmentOptions.Bottom;

                case TextAnchor.LowerRight:
                    return TextAlignmentOptions.BottomRight;
            }

            Debug.LogWarning("Unhandled text anchor " + textAnchor);
            return TextAlignmentOptions.TopLeft;
        }

        public class TextMeshProSettings
        {
            public Color Color;
            public bool EnableAutoSizing;
            public bool Enabled;
            public bool EnableRichText;
            public float FontSize;
            public float FontSizeMax;
            public float FontSizeMin;
            public FontStyles FontStyle;
            public float LineSpacing;
            public bool RayCastTarget;
            public string Text;
            public TextAlignmentOptions TextAlignmentOptions;
            public TextOverflowModes TextOverflowModes;
            public bool WrappingEnabled;
        }
    }
}