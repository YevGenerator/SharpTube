using System.Collections;
using VideoLibrary;

namespace SharpTube;

public class YoutubeRunner
{
    private YouTube? _youtube;
    private YouTube YouTube => _youtube ??= YouTube.Default;

    public YouTubeVideo[] GetVideos(string url)
    {
        var videos = YouTube.GetAllVideos(url);
        return videos as YouTubeVideo[] ?? videos.ToArray();
    }

    public YouTubeVideo GetVideo(string url) => YouTube.GetVideo(url);

    public static void DistributeVideos<T>(YouTubeVideo[] allVideos,
        IVideoContainer<T> container) where T: ICollection<YouTubeVideo>
    {
        YouTubeVideo? muxed = null;
        foreach (var video in allVideos)
        {
            Console.WriteLine(video.AudioFormat.ToString() + "||" + video.Format.ToString());
            if (video.AudioFormat == AudioFormat.Unknown)
            {
                container.Videos.Add(video);
                continue;
            }

            if (video.Format == VideoFormat.Unknown)
            {
                container.Audios.Add(video);
                continue;
            }

            muxed = video;
        }

        container.MuxedVideo = muxed;
    }
    private static string ReplaceInvalidChars(string fileName) => 
        string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

    public async Task<string> DownloadVideo(YouTubeVideo video, string saveDirectory, Action<double>? progress)
    {
        var resolution = video.AudioFormat == AudioFormat.Unknown ? "" : ".aac";
        var savePath = Path.Combine(saveDirectory, ReplaceInvalidChars(video.FullName)) +resolution;
        const int bufferSize = 81920;
        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        var bytesRead = 0;
        
        await using var fileStream = File.Open(savePath, FileMode.Create, FileAccess.Write);
        var videoStream = await video.StreamAsync();
        var totalSize = video.ContentLength;
        
        while ((bytesRead = await videoStream.ReadAsync(buffer.AsMemory(0, bufferSize))) != 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalBytesRead += bytesRead;
            progress?.Invoke((double)totalBytesRead / totalSize ?? 0);
        }

        return savePath;
    }
}
