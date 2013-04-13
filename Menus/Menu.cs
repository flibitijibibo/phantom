﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Phantom.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Phantom.Menus
{
    public enum MenuOrientation { TopToBottom, LeftToRight, TopToBottomLeftToRight, LeftToRightTopToBottom, TwoDimensional };

    public class Menu : GameState
    {
        //TODO this should be a font included in the library
        public static SpriteFont Font;
        public static Color ColorText = Color.Black;
        public static Color ColorTextHighLight = Color.Blue;
        public static Color ColorFace = Color.Gray;
        public static Color ColorFaceHighLight = Color.Yellow;
        public static Color ColorShadow = Color.Black;


        public MenuOrientation Orientation;

        private List<MenuControl> controls;
        private MenuControl selected;
        private Renderer renderer;
        public MenuControl Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                    return;
                if (selected != null)
                {
                    selected.CancelPress();
                    selected.Selected = false;
                }
                selected = value;
                if (selected != null)
                    selected.Selected = true;
            }
        }

        public Menu(Renderer renderer, MenuOrientation orientation)
        {
            this.Orientation = orientation;
            AddComponent(renderer);
            OnlyOnTop = true;
            controls = new List<MenuControl>();
            this.renderer = renderer;
        }

        public override void ClearComponents()
        {
            base.ClearComponents();
            controls.Clear();
        }

        protected override void OnComponentAdded(Component child)
        {
            base.OnComponentAdded(child);
            if (child is MenuControl)
                controls.Add((MenuControl)child);
        }

        protected override void OnComponentRemoved(Component child)
        {
            base.OnComponentRemoved(child);
            if (child is MenuControl)
                controls.Remove((MenuControl)child);
        }

        public override void BackOnTop()
        {
            base.BackOnTop();
            if (Selected == null && controls.Count > 0)
                Selected = controls[0];
        }

        public override void Render(RenderInfo info)
        {
            if (info != null)
                base.Render(info);
            else
                renderer.Render(null);
        }

        public virtual void Back()
        {
            //TODO 
        }

        public void ConnectControls()
        {
            ConnectControls(float.MaxValue);
        }

        public void ConnectControls(float maxDistance)
        {
            for (int i = 0; i < controls.Count; i++)
            {
                switch (Orientation)
                {
                    case MenuOrientation.LeftToRight:
                        FindConnectionsLeftRight(controls[i], maxDistance);
                        break;
                    case MenuOrientation.TopToBottom:
                        FindConnectionsTopBottom(controls[i], maxDistance);
                        break;
                    case MenuOrientation.LeftToRightTopToBottom:
                    case MenuOrientation.TopToBottomLeftToRight:
                    case MenuOrientation.TwoDimensional:
                        FindConnectionsTopBottom(controls[i], maxDistance);
                        FindConnectionsLeftRight(controls[i], maxDistance);
                        break;
                }
            }
            switch (Orientation)
            {
                case MenuOrientation.LeftToRightTopToBottom:
                    RemoveConnectionsTopToBottom();
                    break;
                case MenuOrientation.TopToBottomLeftToRight:
                    RemoveConnectionsLeftToRight();
                    break;
            }
        }

        private void FindConnectionsTopBottom(MenuControl menuControl, float maxDistance)
        {
            menuControl.Above = null;
            menuControl.Below = null;
            float distanceAbove = maxDistance * maxDistance;
            float distanceBelow = maxDistance * maxDistance;
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i] != menuControl)
                {
                    Vector2 d = controls[i].Position - menuControl.Position;
                    if (Math.Abs(d.X)<Math.Abs(d.Y)) 
                    {
                        float distance = d.LengthSquared();
                        if (d.Y > 0 && distance < distanceBelow)
                        {
                            distanceBelow = distance;
                            menuControl.Below = controls[i];
                        } 
                        else if (d.Y < 0 && distance < distanceAbove)
                        {
                            distanceAbove = distance;
                            menuControl.Above = controls[i];
                        }
                    }
                }
            }
        }

        private void FindConnectionsLeftRight(MenuControl menuControl, float maxDistance)
        {
            menuControl.Left = null;
            menuControl.Right = null;
            float distanceLeft = maxDistance * maxDistance;
            float distanceRight = maxDistance * maxDistance;
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i] != menuControl)
                {
                    Vector2 d = controls[i].Position - menuControl.Position;
                    if (Math.Abs(d.X) > Math.Abs(d.Y))
                    {
                        float distance = d.LengthSquared();
                        if (d.X > 0 && distance < distanceRight)
                        {
                            distanceRight = distance;
                            menuControl.Right = controls[i];
                        }
                        else if (d.X < 0 && distance < distanceLeft)
                        {
                            distanceLeft = distance;
                            menuControl.Left = controls[i];
                        }
                    }
                }
            }
        }

        private void RemoveConnectionsTopToBottom()
        {
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i].Right == null && controls[i].Left != null)
                {
                    MenuControl mc = controls[i].Left;
                    int max = 100;
                    while (mc.Left != null && max > 0 && mc.Left.Below != null)
                    {
                        mc = mc.Left;
                        max--;
                    }

                    controls[i].Right = mc.Below;
                }
            }

            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i].Right != null && controls[i].Right.Left == null)
                    controls[i].Right.Left = controls[i];
            }

            for (int i = 0; i < controls.Count; i++)
            {
                controls[i].Above = null;
                controls[i].Below = null;
            }
        }

        private void RemoveConnectionsLeftToRight()
        {
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i].Below == null && controls[i].Above != null)
                {
                    MenuControl mc = controls[i].Above;
                    int max = 100;
                    while (mc.Above != null && max > 0 && mc.Above.Right != null)
                    {
                        mc = mc.Above;
                        max--;
                    }

                    controls[i].Below = mc.Right;
                }
            }

            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i].Below != null && controls[i].Below.Above == null)
                    controls[i].Below.Above = controls[i];
            } 
            
            for (int i = 0; i < controls.Count; i++)
            {
                controls[i].Left = null;
                controls[i].Right = null;
            }
        }

        public void WrapControls()
        {
            for (int i = 0; i < controls.Count; i++)
            {
                switch (Orientation)
                {
                    case MenuOrientation.LeftToRight:
                    case MenuOrientation.LeftToRightTopToBottom:
                        WrapLeftRight(controls[i]);
                        break;
                    case MenuOrientation.TopToBottom:
                    case MenuOrientation.TopToBottomLeftToRight:
                        WrapTopBottom(controls[i]);
                        break;
                    case MenuOrientation.TwoDimensional:
                        WrapTopBottom(controls[i]);
                        WrapLeftRight(controls[i]);
                        break;
                }
            }
        }

        private void WrapTopBottom(MenuControl menuControl)
        {
            if (menuControl.Above == null)
            {
                MenuControl mc = menuControl.Below;
                if (mc != null)
                {
                    int max = 100;
                    while (mc.Below != null && max > 0)
                    {
                        mc = mc.Below;
                        max--;
                    }
                }
                menuControl.Above = mc;
                mc.Below = menuControl;
            }
            if (menuControl.Below == null)
            {
                MenuControl mc = menuControl.Above;
                if (mc != null)
                {
                    int max = 100;
                    while (mc.Above != null && max > 0)
                    {
                        mc = mc.Above;
                        max--;
                    }
                }
                menuControl.Below = mc;
                mc.Above = menuControl;
            }
        }

        private void WrapLeftRight(MenuControl menuControl)
        {
            if (menuControl.Left == null)
            {
                MenuControl mc = menuControl.Right;
                if (mc != null)
                {
                    int max = 100;
                    while (mc.Right != null && max > 0)
                    {
                        mc = mc.Right;
                        max--;
                    }
                }
                menuControl.Left = mc;
                mc.Right = menuControl;
            }
            if (menuControl.Right == null)
            {
                MenuControl mc = menuControl.Left;
                if (mc != null)
                {
                    int max = 100;
                    while (mc.Left != null && max > 0)
                    {
                        mc = mc.Left;
                        max--;
                    }
                }
                menuControl.Right = mc;
                mc.Left = menuControl;
            }
        }

    }
}