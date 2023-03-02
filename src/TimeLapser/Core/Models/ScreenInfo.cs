namespace kasthack.TimeLapser.Core.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Screen info merged with viewmodel for simplicity.
    /// </summary>
    public class ScreenInfo : INotifyPropertyChanged
    {
        private string name;
        private int id;
        private Rectangle rect;

        public event PropertyChangedEventHandler PropertyChanged;

        public Rectangle Rectangle
        {
            get => this.rect;
            set => this.Update(NormalizeRectangle(value), ref this.rect);
        }

        public int Id
        {
            get => this.id;
            set => this.Update(value, ref this.id);
        }

        public string Name
        {
            get => this.name;
            set => this.Update(value, ref this.name);
        }

        public override string ToString() => $"{this.Name}({this.Id}) ({this.Rectangle.Width}x{this.Rectangle.Height})";

        /// <summary>
        /// width and height must be even for simpler capturing.
        /// </summary>
        /// <param name="source">Rectangle to normalize.</param>
        /// <returns>Normalized rectangle.</returns>
        private static Rectangle NormalizeRectangle(Rectangle source) => new(source.Location, new Size(source.Size.Width & ~1, source.Size.Height & ~1));

        private void Update<T>(T value, ref T field, [CallerMemberName] string property = null)
        {
            if (!EqualityComparer<T>.Default.Equals(value, field))
            {
                field = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
