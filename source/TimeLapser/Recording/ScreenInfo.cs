using System.Drawing;

namespace kasthack.TimeLapser
{
    public class ScreenInfo
    {
        public Rectangle Rect { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() => $"{Name}({Id})";
    }
}