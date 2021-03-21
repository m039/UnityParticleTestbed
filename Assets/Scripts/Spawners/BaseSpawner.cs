using UnityEngine;

using m039.Common;
using static m039.Common.UIUtils;

namespace GP4
{
    public interface ISpawnerContext
    {
        BaseLivingEntityConfig LivingEntityConfig { get; }

        event System.Action OnLivingEntityDataChanged;

        bool GUIVisibility { get; }
    }

    public abstract class BaseSpawner : MonoBehaviour
    {
        #region Inspector

        public int numberOfEntities = 10;

        public float entetiesReferenceSpeed = 5f;

        public float entetiesReferenceScale = 0.5f;

        [Range(0, 1f)]
        public float entetiesReferenceAlpha = 1f;

        public bool useGizmos = true;

        #endregion

        IDrawer _drawer;

        public bool IsSelected { get; private set; } = false;

        protected ISpawnerContext Context { get; private set; }

        public void SetSelected(bool selected)
        {
            if (!Application.isPlaying)
                return;

            IsSelected = selected;

            if (selected)
            {
                OnSpawnerSelected();
            } else
            {
                OnSpawnerDeselected();
            }
        }

        protected virtual void OnEnable()
        {
            Context = GameScene.Instance;
            Context.OnLivingEntityDataChanged += OnLivingEntityDataChanged;
        }

        private void OnDisable()
        {
            if (Context == null)
                return;

            Context.OnLivingEntityDataChanged -= OnLivingEntityDataChanged;
            Context = null;    
        }

        public abstract void OnSpawnerSelected();

        public abstract void OnSpawnerDeselected();

        protected abstract int EntetiesCount { get; }

        protected virtual void OnLivingEntityDataChanged()
        {
        }

        protected interface IDrawer
        {
            void DrawStat(int index, string text);

            void DrawStatFrame(int numberOfStats);

            void DrawName(string name);

            void DrawGetNumber(string label, ref int number);
        }

        class Drawer : IDrawer
        {
            GUIStyle _labelStyle;

            GUIStyle _frameStyle;

            GUIStyle _textStyle;

            Rect _statRect;

            float _offset;

            Texture2D _frameTexture;

            Font _statFont;

            Font _nameFont;

            public Drawer()
            {
                _labelStyle = new GUIStyle(GUI.skin.label);

                _statFont = _labelStyle.font;
                _nameFont = LoadFont(FontCategory.SansSerif, FontStyle.Italic);

                _frameTexture = new Texture2D(1, 1);
                _frameTexture.wrapMode = TextureWrapMode.Repeat;
                _frameTexture.SetPixel(0, 0, Color.black.WithAlpha(0.2f));
                _frameTexture.Apply();

                _frameStyle = new GUIStyle();
                _frameStyle.normal.background = _frameTexture;

                _textStyle = new GUIStyle(GUI.skin.textField);

                var windowHeight = 200 * UICoeff;
                var windowWidth = 800 * UICoeff;

                _statRect = new Rect(Screen.width - windowWidth - UIMediumMargin, UIMediumMargin, windowWidth, windowHeight);
                _offset = 4 * UICoeff;
            }

            public void DrawStatFrame(int numberOfStats)
            {
                var tRect = new Rect(_statRect);

                tRect.x -= UISmallMargin;
                tRect.y -= UISmallMargin;
                tRect.height = _labelStyle.fontSize * numberOfStats + 50 * UICoeff * numberOfStats + UISmallMargin;
                tRect.width += UISmallMargin * 2;

                GUI.Box(tRect, _frameTexture, _frameStyle);
            }

            public void DrawStat(int index, string text)
            {
                var topOffset = _labelStyle.fontSize * index + 50 * UICoeff * index;

                _labelStyle.fontSize = (int)(60 * UICoeff);
                _labelStyle.alignment = TextAnchor.UpperLeft;
                _labelStyle.font = _statFont;

                // Draw shadow

                var tRect = new Rect(_statRect);
                tRect.center += Vector2.one * _offset + Vector2.up * topOffset;

                _labelStyle.normal.textColor = Color.black;

                GUI.Label(tRect, text, _labelStyle);

                // Draw text

                _labelStyle.normal.textColor = Color.white;

                tRect = new Rect(_statRect);
                tRect.center += Vector2.up * topOffset;

                GUI.Label(tRect, text, _labelStyle);
            }

            public void DrawName(string name)
            {
                var marginVertical = UIMediumMargin * 2;
                var marginHorizontal = UIMediumMargin;
                var tRect = new Rect(marginHorizontal, Screen.height - (marginHorizontal + marginVertical), 2000 * UICoeff, marginVertical);

                _labelStyle.fontSize = (int)(60 * UICoeff);
                _labelStyle.alignment = TextAnchor.LowerLeft;
                _labelStyle.font = _nameFont;

                // Draw frame

                var size = _labelStyle.CalcSize(new GUIContent(name));
                var margin = 32 * UICoeff;
                var frameRect = new Rect(tRect.x - margin, tRect.y + (marginVertical - size.y) - margin, size.x + margin * 2, size.y + margin * 2);

                GUI.Box(frameRect, _frameTexture, _frameStyle);

                // Draw shadow

                _labelStyle.normal.textColor = Color.black;

                var tRectShadow = new Rect(tRect);
                tRectShadow.center += Vector2.one * _offset;

                GUI.Label(tRectShadow, name, _labelStyle);

                // Draw text

                _labelStyle.normal.textColor = Color.white;

                GUI.Label(tRect, name, _labelStyle);
            }

            string _numberText;

            public void DrawGetNumber(string label, ref int number)
            {
                var topOffset = 800 * UICoeff;
                var margin = UISmallMargin;
                var labelSize = _labelStyle.CalcSize(new GUIContent(label));

                /// Draw Frame

                var tRect = new Rect(_statRect);

                tRect.center += Vector2.up * topOffset - Vector2.one * margin;
                tRect.size = new Vector2(
                    margin * 2 + _statRect.width,
                    margin * 3 + UISmallPadding + labelSize.y * 2
                    );

                GUI.Box(tRect, _frameTexture, _frameStyle);

                /// Label

                _labelStyle.fontSize = (int)(60 * UICoeff);
                _labelStyle.alignment = TextAnchor.UpperLeft;
                _labelStyle.font = _statFont;

                // Draw shadow

                tRect = new Rect(_statRect);

                tRect.y += topOffset;
                tRect.center += Vector2.one * _offset;

                _labelStyle.normal.textColor = Color.black;

                GUI.Label(tRect, label, _labelStyle);

                // Draw text

                _labelStyle.normal.textColor = Color.white;

                tRect = new Rect(_statRect);
                tRect.y += topOffset;

                GUI.Label(tRect, label, _labelStyle);

                /// Draw textField

                tRect = new Rect(_statRect);

                tRect.y += topOffset + labelSize.y + margin;
                tRect.height = labelSize.y + UISmallPadding;

                if (_numberText == null || int.TryParse(_numberText, out int result) && result != number) {
                    _numberText = number.ToString();
                }

                _textStyle.fontSize = (int)(60 * UICoeff);
                _textStyle.alignment = TextAnchor.MiddleLeft;
                _textStyle.font = _statFont;
                _textStyle.padding.left = _textStyle.padding.right = _textStyle.padding.top = _textStyle.padding.bottom = (int)UISmallPadding;

                var text = GUI.TextField(tRect, _numberText, 10, _textStyle);

                if (int.TryParse(text, out result))
                {
                    number = result;
                    _numberText = text;
                }
            }
        }

        void OnGUI()
        {
            if (_drawer == null)
            {
                _drawer = new Drawer();
            }

            if (Context.GUIVisibility)
            {
                PerformOnGUI(_drawer);
            }
        }

        protected virtual void PerformOnGUI(IDrawer drawer)
        {
            drawer.DrawStatFrame(4);
            drawer.DrawStat(0, "Entities: " + EntetiesCount);
            drawer.DrawStat(1, "Global Scale: " + entetiesReferenceScale);
            drawer.DrawStat(2, "Global Alpha: " + entetiesReferenceAlpha);
            drawer.DrawStat(3, "Global Speed: " + entetiesReferenceSpeed);

            drawer.DrawGetNumber("Number of Enteties [" + numberOfEntities + "]:", ref numberOfEntities);
        }
    }

}
