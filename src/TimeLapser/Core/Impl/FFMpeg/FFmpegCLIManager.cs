// Imported from: https://github.com/ShareX/ShareX/blob/e91fd94d2bfcecf276893fee1dd7d115939b6c0c/ShareX.MediaLib/FFmpegCLIManager.cs
// changes: async
#pragma warning disable IDE0065,SA1200,SA1516,SA1201,SA1101,SA1401,SA1413,IDE0009,IDE0011,SA1503,SA1202,IDE0031,SA1208,SA1303,SA1513,RCS1059,SA1310,SA1005,SA1117,SA1515,SA1124,IDE0007,IDEO0022,RCS1170

#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2023 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShareX.MediaLib
{
    public class FFmpegCLIManager : ExternalCLIManager
    {
        public const string SourceNone = "None";
        public const string SourceGDIGrab = "GDI grab";
        public const string SourceVideoDevice = "screen-capture-recorder";
        public const string SourceAudioDevice = "virtual-audio-capturer";

        public const int x264_min = 0;
        public const int x264_max = 51;
        public const int x265_min = 0;
        public const int x265_max = 51;
        public const int vp8_min = 4;
        public const int vp8_max = 63;
        public const int vp9_min = 0;
        public const int vp9_max = 63;
        public const int xvid_min = 1;
        public const int xvid_max = 31;
        public const int mp3_min = 0;
        public const int mp3_max = 9;

        public delegate void EncodeStartedEventHandler();
        public event EncodeStartedEventHandler EncodeStarted;

        public delegate void EncodeProgressChangedEventHandler(float percentage);
        public event EncodeProgressChangedEventHandler EncodeProgressChanged;

        public string FFmpegPath { get; private set; }
        public StringBuilder Output { get; private set; }
        public bool IsEncoding { get; set; }
        public bool ShowError { get; set; }
        public bool StopRequested { get; set; }
        public bool TrackEncodeProgress { get; set; }
        public TimeSpan VideoDuration { get; set; }
        public TimeSpan EncodeTime { get; set; }
        public float EncodePercentage { get; set; }

        private int closeTryCount = 0;

        public FFmpegCLIManager(string ffmpegPath)
        {
            FFmpegPath = ffmpegPath;
            Output = new StringBuilder();
            OutputDataReceived += FFmpeg_DataReceived;
            ErrorDataReceived += FFmpeg_DataReceived;
        }

        public async Task<bool> Run(string args)
        {
            return await Run(FFmpegPath, args).ConfigureAwait(false);
        }

        protected async Task<bool> Run(string path, string args)
        {
            StopRequested = false;
            int errorCode = await Open(path, args).ConfigureAwait(false);
            IsEncoding = false;
            bool result = errorCode == 0;
            if (!result && ShowError)
            {
                throw new Exception($"FFMpeg failed with code {errorCode}");
            }
            return result;
        }

        public override async Task Close()
        {
            StopRequested = true;

            if (IsProcessRunning && process != null)
            {
                if (closeTryCount >= 2)
                {
                    process.Kill();
                }
                else
                {
                    await WriteInput("q").ConfigureAwait(false);

                    closeTryCount++;
                }
            }
        }

        private void FFmpeg_DataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (this)
            {
                string data = e.Data;

                if (!string.IsNullOrEmpty(data))
                {
                    Output.AppendLine(data);

                    if (!IsEncoding && data.Contains("Press [q] to stop", StringComparison.OrdinalIgnoreCase))
                    {
                        IsEncoding = true;

                        OnEncodeStarted();
                    }

                    if (TrackEncodeProgress)
                    {
                        UpdateEncodeProgress(data);
                    }
                }
            }
        }

        private void UpdateEncodeProgress(string data)
        {
            if (VideoDuration.Ticks == 0)
            {
                //  Duration: 00:00:15.32, start: 0.000000, bitrate: 1095 kb/s
                Match match = Regex.Match(data, @"Duration:\s*(\d+:\d+:\d+\.\d+),\s*start:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                if (match.Success && TimeSpan.TryParse(match.Groups[1].Value, out TimeSpan duration))
                {
                    VideoDuration = duration;
                }
            }
            else
            {
                //frame=  942 fps=187 q=35.0 size=    3072kB time=00:00:38.10 bitrate= 660.5kbits/s speed=7.55x
                Match match = Regex.Match(data, @"time=\s*(\d+:\d+:\d+\.\d+)\s*bitrate=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                if (match.Success && TimeSpan.TryParse(match.Groups[1].Value, out TimeSpan time))
                {
                    EncodeTime = time;
                    EncodePercentage = ((float)EncodeTime.Ticks / VideoDuration.Ticks) * 100;

                    OnEncodeProgressChanged(EncodePercentage);
                }
            }
        }

        protected void OnEncodeStarted()
        {
            EncodeStarted?.Invoke();
        }

        protected void OnEncodeProgressChanged(float percentage)
        {
            EncodeProgressChanged?.Invoke(percentage);
        }

        public async Task<VideoInfo> GetVideoInfo(string videoPath)
        {
            VideoInfo videoInfo = new VideoInfo();
            videoInfo.FilePath = videoPath;

            await Run($"-hide_banner -i \"{videoPath}\"").ConfigureAwait(false);
            string output = Output.ToString();

            Match matchInput = Regex.Match(output, @"Duration: (?<Duration>\d{2}:\d{2}:\d{2}\.\d{2}),.+?start: (?<Start>\d+\.\d+),.+?bitrate: (?<Bitrate>\d+) kb/s",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (matchInput.Success)
            {
                videoInfo.Duration = TimeSpan.Parse(matchInput.Groups["Duration"].Value);
                //videoInfo.Start = TimeSpan.Parse(match.Groups["Start"].Value);
                videoInfo.Bitrate = int.Parse(matchInput.Groups["Bitrate"].Value);
            }
            else
            {
                return null;
            }

            Match matchVideoStream = Regex.Match(output, @"Stream #\d+:\d+(?:\(.+?\))?: Video: (?<Codec>.+?) \(.+?,.+?, (?<Width>\d+)x(?<Height>\d+).+?, (?<FPS>\d+(?:\.\d+)?) fps",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (matchVideoStream.Success)
            {
                videoInfo.VideoCodec = matchVideoStream.Groups["Codec"].Value;
                videoInfo.VideoResolution = new Size(int.Parse(matchVideoStream.Groups["Width"].Value), int.Parse(matchVideoStream.Groups["Height"].Value));
                videoInfo.VideoFPS = double.Parse(matchVideoStream.Groups["FPS"].Value, CultureInfo.InvariantCulture);
            }

            Match matchAudioStream = Regex.Match(output, @"Stream #\d+:\d+(?:\(.+?\))?: Audio: (?<Codec>.+?)(?: \(|,)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (matchAudioStream.Success)
            {
                videoInfo.AudioCodec = matchAudioStream.Groups["Codec"].Value;
            }

            return videoInfo;
        }

        public async Task<DirectShowDevices> GetDirectShowDevices()
        {
            DirectShowDevices devices = new DirectShowDevices();

            await Run("-hide_banner -list_devices true -f dshow -i dummy").ConfigureAwait(false);

            string output = Output.ToString();
            string[] lines = output.Lines();
            bool isAudio = false;
            Regex regex = new Regex(@"\[dshow @ \w+\] +""(.+)""", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            foreach (string line in lines)
            {
                if (line.Contains("] DirectShow video devices"))
                {
                    isAudio = false;
                    continue;
                }
                else if (line.Contains("] DirectShow audio devices"))
                {
                    isAudio = true;
                    continue;
                }

                Match match = regex.Match(line);

                if (match.Success)
                {
                    if (line.EndsWith("\" (video)"))
                    {
                        isAudio = false;
                    }
                    else if (line.EndsWith("\" (audio)"))
                    {
                        isAudio = true;
                    }

                    string deviceName = match.Groups[1].Value;

                    if (isAudio)
                    {
                        devices.AudioDevices.Add(deviceName);
                    }
                    else
                    {
                        devices.VideoDevices.Add(deviceName);
                    }
                }
            }

            return devices;
        }

        public async Task ConcatenateVideos(string[] inputFiles, string outputFile, bool autoDeleteInputFiles = false)
        {
            string listFile = outputFile + ".txt";
            string contents = string.Join(Environment.NewLine, inputFiles.Select(inputFile => $"file '{inputFile}'"));
            File.WriteAllText(listFile, contents);

            try
            {
                bool result = await Run($"-hide_banner -f concat -safe 0 -i \"{listFile}\" -c copy \"{outputFile}\"").ConfigureAwait(false);

                if (result && autoDeleteInputFiles)
                {
                    foreach (string inputFile in inputFiles)
                    {
                        if (File.Exists(inputFile))
                        {
                            File.Delete(inputFile);
                        }
                    }
                }
            }
            finally
            {
                if (File.Exists(listFile))
                {
                    File.Delete(listFile);
                }
            }
        }
    }
}
#pragma warning restore IDE0065, SA1200, SA1516, SA1201, SA1101, SA1401, SA1413, IDE0009, IDE0011, SA1503, SA1202, IDE0031, SA1208, SA1303, SA1513, RCS1059, SA1310, SA1005, SA1117, SA1515, SA1124, IDE0007, IDEO0022, RCS1170
