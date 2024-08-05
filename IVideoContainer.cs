using System.Collections.ObjectModel;
using VideoLibrary;

namespace SharpTube;

public interface IVideoContainer<T> where T: ICollection<YouTubeVideo>
{ 
    YouTubeVideo? MuxedVideo { get; set; }
    T Videos { get; set; }
    T Audios { get; set; }
}