#region license

//  Copyright (C) 2018 ClassicUO Development Community on Github
//
//	This project is an alternative client for the game Ultima Online.
//	The goal of this is to develop a lightweight client considering 
//	new technologies.  
//  (Copyright (c) 2018 ClassicUO Development Team)
//    
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Gumps
{
    public class HtmlGump : GumpControl
    {
        private readonly RenderedText _gameText;
        private IScrollBar _scrollBar;

        public HtmlGump(string[] parts, string[] lines) : this()
        {
            X = int.Parse(parts[1]);
            Y = int.Parse(parts[2]);
            Width = int.Parse(parts[3]);
            Height = int.Parse(parts[4]);
            int textIndex = int.Parse(parts[5]);
            HasBackground = parts[6] == "1";
            HasScrollbar = parts[7] != "0";
            UseFlagScrollbar = HasScrollbar && parts[7] == "2";

            _gameText.IsHTML = true;

            InternalBuild(lines[textIndex], 0);
        }

        public HtmlGump(int x, int y, int w, int h, string text, int hasbackground, int hasscrollbar, int hue,
            bool ishtml, byte font = 1, bool isunicode = true, FontStyle style = FontStyle.None,
            TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT) : this()
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            HasBackground = hasbackground == 1;
            HasScrollbar = hasscrollbar != 0;
            UseFlagScrollbar = hasscrollbar != 0 && hasscrollbar == 2;

            _gameText.IsHTML = ishtml;
            _gameText.FontStyle = style;
            _gameText.Align = align;
            _gameText.Font = font;
            _gameText.IsUnicode = isunicode;

            InternalBuild(text, hue);
        }

        public HtmlGump()
        {
            _gameText = new RenderedText
            {
                IsUnicode = true,
                Align = TEXT_ALIGN_TYPE.TS_LEFT,
                Font = 1
            };
            CanMove = true;
        }


        public bool HasScrollbar { get; }
        public bool HasBackground { get; }
        public bool UseFlagScrollbar { get; }
        public int ScrollX { get; set; }
        public int ScrollY { get; set; }

        public string Text
        {
            get => _gameText.Text;
            set => _gameText.Text = value;
        }

        private void InternalBuild(string text, int hue)
        {
            if (_gameText.IsHTML)
            {
                uint htmlColor = 0xFFFFFFFF;
                ushort color = 0;

                if (hue > 0)
                {
                    if (hue == 0x00FFFFFF)
                        htmlColor = 0xFFFFFFFE;
                    else
                        htmlColor = (Hues.Color16To32((ushort) hue) << 8) | 0xFF;
                }
                else if (!HasBackground)
                {
                    color = 0xFFFF;
                    if (!HasScrollbar)
                        htmlColor = 0x010101FF;
                }
                else
                    htmlColor = 0x010101FF;

                _gameText.HTMLColor = htmlColor;
                _gameText.Hue = color;
            }
            else
                _gameText.Hue = (ushort) hue;

            _gameText.ColorBackground = !HasBackground;
            _gameText.MaxWidth = Width - (HasScrollbar ? 15 : 0) - (HasBackground ? 8 : 0);
            _gameText.Text = text;

            if (HasBackground)
            {
                AddChildren(new ResizePic(0x2486)
                {
                    Width = Width - (HasScrollbar ? 15 : 0),
                    Height = Height,
                    AcceptMouseInput = false
                });
            }

            if (HasScrollbar)
            {
                if (UseFlagScrollbar)
                {
                    _scrollBar = new ScrollFlag(this)
                    {
                        Location = new Point(Width - 14, 0)
                    };
                }
                else
                    _scrollBar = new ScrollBar(this, Width - 14, 0, Height);

                _scrollBar.Height = Height;
                _scrollBar.MinValue = 0;
                _scrollBar.MaxValue = _gameText.Height - Height + (HasBackground ? 8 : 0);
                ScrollY = _scrollBar.Value;
            }

            if (Width != _gameText.Width)
                Width = _gameText.Width;
        }

        public override void Update(double totalMS, double frameMS)
        {
            if (HasScrollbar)
            {
                _scrollBar.Height = Height;
                _scrollBar.MinValue = 0;
                _scrollBar.MaxValue = _gameText.Height - Height + (HasBackground ? 8 : 0);
                ScrollY = _scrollBar.Value;
            }

            base.Update(totalMS, frameMS);
        }

        public override bool Draw(SpriteBatchUI spriteBatch, Vector3 position, Vector3? hue = null)
        {
            base.Draw(spriteBatch, position);

            _gameText.Draw(spriteBatch,
                new Rectangle((int) position.X + (HasBackground ? 4 : 0), (int) position.Y + (HasBackground ? 4 : 0),
                    Width - (HasBackground ? 8 : 0), Height - (HasBackground ? 8 : 0)), ScrollX, ScrollY);

            return true;
        }

        protected override void OnMouseClick(int x, int y, MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                for (int i = 0; i < _gameText.Links.Count; i++)
                {
                    WebLinkRect link = _gameText.Links[i];
                    Rectangle rect = new Rectangle(link.StartX, link.StartY, link.EndX, link.EndY);
                    bool inbounds = rect.Contains(x, y);
                    if (inbounds && Fonts.GetWebLink(link.LinkID, out WebLink result))
                    {
                        Service.Get<Log>().Message(LogTypes.Info, "LINK CLICKED: " + result.Link);
                        BrowserHelper.OpenBrowser(result.Link);
                        break;
                    }
                }
            }
        }
    }
}