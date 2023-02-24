namespace kasthack.TimeLapser.Recording.Snappers.Factory;

using kasthack.TimeLapser.Recording.Models;

public interface ISnapperFactory
{
    ISnapper GetSnapper(SnapperType type);
}
