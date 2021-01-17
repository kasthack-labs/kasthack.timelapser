using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace kasthack.TimeLapser
{
    public class ScreenInfo : INotifyPropertyChanged
    {
        private string name;
        private int id;
        private Rectangle rect;

        public Rectangle Rect
        {
            get { return rect; }
            set { Update(value, ref rect); }
        }
        public int Id
        {
            get { return id; }
            set { Update(value, ref id); }
        }
        public string Name
        {
            get { return name; }
            set { Update(value, ref name); }
        }

        public override string ToString() => $"{Name}({Id})";

        private void Update<T>(T value, ref T field, [CallerMemberName] string property = null)
        {
            if (!EqualityComparer<T>.Default.Equals(value, field))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public static IList<ScreenInfo> GetScreenInfos()
        {
            var scr = Screen.AllScreens.OrderBy(a => a.Bounds.X).ToArray();
            var screens = Enumerable.Range(1, scr.Length)
                        .Select(a => new ScreenInfo { Id = a, Name = scr[a - 1].DeviceName, Rect = NormalizeRectangle(scr[a - 1].Bounds), })
                        .ToList();
            var mx = scr.Min(a => a.Bounds.X);
            var my = scr.Min(a => a.Bounds.Y);
            var w = scr.Max(a => a.Bounds.Width + a.Bounds.X) - mx;
            var h = scr.Max(a => a.Bounds.Height + a.Bounds.Y) - my;
            if (screens.Count > 1)
            {
                screens.Add(new ScreenInfo { Id = screens.Count + 1, Name = Locale.Locale.AllScreens, Rect = NormalizeRectangle(new Rectangle(mx, my, w, h)) });
            }
            return screens.ToList();
        }

        public static Rectangle NormalizeRectangle(Rectangle source)
        {
            return new Rectangle(source.Location, new Size(source.Size.Width - source.Size.Width % 2, source.Size.Height - source.Size.Height % 2));
        }
    }
}