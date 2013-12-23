﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;
using Phantom.Core;
using Phantom.Misc;
using Microsoft.Xna.Framework.Input;

namespace Phantom.GameUI
{
    /// <summary>
    /// A simple menu button that can be clicked to throw MenuClicked messages in the menu.
    /// It renders a simple button if the menu's renderer has a canvas.
    /// </summary>
    public class EditBox : UIElement
    {
        /// <summary>
        /// The buttons visible caption
        /// </summary>
        public string Text;
        public int TextLength;
        private float w;
        private float h;
        private float timer = 0;
        private int cursor;
        private Konsoul.KeyMap keyMap;


        private KeyboardState previous;


        /// <summary>
        /// Creates the button and sets the button's caption
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="shape"></param>
        public EditBox(string name, string text, Vector2 position, OABB shape, int textLength)
            : base(name, position, shape)
        {
            this.Text= text;
            this.w = shape.HalfSize.X - 4;
            this.h = UILayer.Font.LineSpacing * 0.5f;
            this.TextLength = textLength;
            this.keyMap = new Konsoul.KeyMap();
        }

        /// <summary>
        /// A simple visualization rendered to the menu's renderer's canvas. But only when the menu's static font has been set
        /// </summary>
        /// <param name="info"></param>
        public override void Render(Graphics.RenderInfo info)
        {
            if (UILayer.Font != null && Visible)
            {
                Color face = Color.Lerp(UILayer.ColorTextBox, UILayer.ColorTextBoxHighLight, this.currentSelected);
                Color text = Color.Lerp(UILayer.ColorText, UILayer.ColorTextHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = UILayer.ColorFaceDisabled;
                    text = UILayer.ColorTextDisabled;
                }

				PhantomUtils.DrawShape(info, this.Position, this.Shape, face, UILayer.ColorShadow, 2);
				//PhantomUtils.DrawShape(info, this.Position - Vector2.One * down, this.Shape, face, UILayer.ColorShadow, 2);
                Vector2 p = Position;
                p.Y -= h;
                p.X -= w;
                p.X = (float)Math.Round(p.X);
                p.Y = (float)Math.Round(p.Y);
                string s = Text;
                if (timer % 0.5f > 0.2f)
                    s += "_";
                info.Batch.DrawString(UILayer.Font, s, p, text);
            }
        }

        public override void Update(float elapsed)
        {
            if (this.Focus)
            {
                KeyboardState current = Keyboard.GetState();
                timer += elapsed;

                bool shift = current.IsKeyDown(Keys.LeftShift) || current.IsKeyDown(Keys.RightShift);
                //bool ctrl = current.IsKeyDown(Keys.LeftControl) || current.IsKeyDown(Keys.RightControl);

                Keys[] pressedKeys = current.GetPressedKeys();
                for (int i = 0; i < pressedKeys.Length; i++)
                {
                    Keys k = pressedKeys[i];
                    if (previous.IsKeyDown(k) || Text.Length >= TextLength)
                        continue;
                    char c = this.keyMap.getChar(k, shift ? Konsoul.KeyMap.Modifier.Shift : Konsoul.KeyMap.Modifier.None);
                    if (c != '\0')
                    {
                        this.Text = this.Text.Insert(this.cursor++, c.ToString());
                        this.GetAncestor<GameState>().HandleMessage(Messages.UIElementValueChanged, this);
                    }
                }
                if (current.IsKeyDown(Keys.Back) && !previous.IsKeyDown(Keys.Back) && this.cursor > 0)
                {
                    this.Text = this.Text.Remove(this.cursor - 1, 1);
                    this.cursor = (int)MathHelper.Clamp(this.cursor - 1, 0, this.Text.Length);
                    this.GetAncestor<GameState>().HandleMessage(Messages.UIElementValueChanged, this);
                }
                if (current.IsKeyDown(Keys.Delete) && !previous.IsKeyDown(Keys.Delete) && this.cursor < this.Text.Length)
                {
                    this.Text = this.Text.Remove(this.cursor, 1);
                    this.GetAncestor<GameState>().HandleMessage(Messages.UIElementValueChanged, this);
                    //lastCursor = -1; // force reblink
                }

                if (current.IsKeyDown(Keys.Enter) && !previous.IsKeyDown(Keys.Enter))
                {
                    this.GetAncestor<GameState>().HandleMessage(Messages.UIElementEnter, this);
                    this.layer.FocusOnNext();
                }

                if (current.IsKeyDown(Keys.Tab) && !previous.IsKeyDown(Keys.Tab))
                {
                    if (shift)
                        this.layer.FocusOnPrevious();
                    else
                        this.layer.FocusOnNext();
                }

                previous = current;
            }
            else
            {
                timer = 0.0f;
            }
            base.Update(elapsed);
        }
    }
}
