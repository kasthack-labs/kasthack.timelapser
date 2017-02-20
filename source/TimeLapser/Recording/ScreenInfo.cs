using System.Drawing;

namespace TimeLapser {

    public class ScreenInfo {
        public Rectangle Rect;
        public int Id;
        public string Name;
        public override string ToString() => $"{Name}({Id})";
    }
}