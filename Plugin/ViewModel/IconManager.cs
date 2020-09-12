using static StreamDeck.OscBridge.Constants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BarRaider.SdTools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

namespace StreamDeck.OscBridge.ViewModel
{
    internal class IconManager
    {
        private readonly ResizeOptions _buttonImageResize = new ResizeOptions()
        {
            Compand = true,
            Mode = ResizeMode.Stretch,
            Sampler = KnownResamplers.Welch,
            Size = new Size(ButtonImageWidth, ButtonImageHeight)
        };

        private readonly IImageFormat _buttonImageFormat = PngFormat.Instance;

        private class CachedImage
        {
            public string _data;
            public DateTime _modifiedDate;
        }
        internal readonly static IconManager s_instance = new IconManager();
        private readonly Model.Config _config;

        internal void NoOperation() { }

        private IconManager()
        {
            _iconsNormalSize = new ConcurrentDictionary<Tuple<string, string>, string>();
            _cachedImages = new ConcurrentDictionary<string, CachedImage>(StringComparer.InvariantCultureIgnoreCase);
            _config = Model.Sys.s_instance.Config;
            _config.BackgroundDirectoryChanged += LoadIconFiles;
            _config.IconDirectoryChanged += LoadIconFiles;
            LoadIconResouces();
            LoadIconFiles();
        }

        private void LoadIconResouces()
        {
            var icons = new Dictionary<string, string>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resources = assembly.GetManifestResourceNames();
            foreach (string icon in new string[] { "keyDefault.png", "keyMedia.png", "keyMediaMissing.png" })
            {
                string resource = resources.Single(str => str.EndsWith(icon));
                using Stream stream = assembly.GetManifestResourceStream(resource);
                icons.Add(icon, PngBytesToString(stream));
            }
            _defaultIcon = icons;
        }

        static string PngBytesToString(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            stream.Position = 0;
            byte[] bytes = reader.ReadBytes((int)stream.Length);
            if (bytes.Length < 1)
            {
                return null;
            }
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }

        private void LoadIconFiles()
        {
            string backgroundDir = Environment.ExpandEnvironmentVariables(_config.BackgroundDirectory);
            if (!Directory.Exists(backgroundDir))
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Background directory {backgroundDir} does not exist, so cannot load icons.");
                return;
            }
            string iconDir = Environment.ExpandEnvironmentVariables(_config.IconDirectory);
            if (!Directory.Exists(iconDir))
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Icon directory {iconDir} does not exist, so cannot load icons.");
                return;
            }
            List<Tuple<string, Image<Rgba32>>> backgrounds = Directory
                .EnumerateFiles(backgroundDir)
                .Select(f => new Tuple<string, Image<Rgba32>>(Path.GetFileNameWithoutExtension(f), Image.Load<Rgba32>(f)))
                .ToList();
            GraphicsOptions options = new GraphicsOptions() { AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver, ColorBlendingMode = PixelColorBlendingMode.Normal, Antialias = false, BlendPercentage = 1 };
            IImageFormat format = PngFormat.Instance;
            foreach (string name in Directory.EnumerateFiles(iconDir))
            {
                int bgIndex = 0;
                using Image<Rgba32> icon = Image.Load<Rgba32>(name);
                foreach (Tuple<string, Image<Rgba32>> background in backgrounds)
                {
                    using Image<Rgba32> composited = background.Item2.Clone();
                    composited.Mutate(ctx => ctx.DrawImage(icon, options));
                    string base64String = composited.ToBase64String(format);
                    _iconsNormalSize.AddOrUpdate(BuildKey(Path.GetFileNameWithoutExtension(name), background.Item1), base64String, (k, v) => base64String);
                    bgIndex++;
                }
            }
            backgrounds.ForEach(b => b.Item2.Dispose());
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Loaded {_iconsNormalSize.Count} images");
        }

        private static Tuple<string, string> BuildKey(string icon, string background) => new Tuple<string, string>(icon, background);

        private readonly ConcurrentDictionary<Tuple<string, string>, string> _iconsNormalSize;
        private IReadOnlyDictionary<string, string> _defaultIcon;
        private readonly ConcurrentDictionary<string, CachedImage> _cachedImages;

        /// <summary>
        /// Get and image with the given icon and background, falling back to the default for the given requestor type.
        /// </summary>
        public string GetImage(string icon, string background)
        {
            if (_iconsNormalSize.TryGetValue(BuildKey(icon, background), out string image))
            {
                return image;
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Could not find image for icon {icon} with background {background}");
            return _defaultIcon[IconKeyDefault];
        }

        /// <summary>
        /// Get an image from a media path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetImage(string path, out bool usingFallback)
        {
            bool exists = !string.IsNullOrWhiteSpace(path) && File.Exists(path);

            if (_cachedImages.TryGetValue(path, out CachedImage image))
            {
                if (!exists)
                {
                    _cachedImages.TryRemove(path, out _);
                    image = null;
                }
            }
            if (exists)
            {
                DateTime modified = File.GetLastWriteTimeUtc(path);
                if (image?._modifiedDate != modified)
                {
                    string newImageText = GetThumbnailFromDisk(path);
                    if (newImageText != null)
                    {
                        image = new CachedImage() { _data = newImageText, _modifiedDate = modified };
                        _cachedImages.AddOrUpdate(path, image, (s, i) => image);
                    }
                }
            }

            if (image == null)
            {
                usingFallback = true;
                return _defaultIcon[string.IsNullOrWhiteSpace(path) ? IconKeyMediaMissing : IconKeyMedia];
            }
            else
            {
                usingFallback = false;
                return image._data;
            }
        }

        private string GetThumbnailFromDisk(string path)
        {
            string expanded = Environment.ExpandEnvironmentVariables(path);
            switch (Path.GetExtension(expanded).ToLower())
            {
                case ".png":
                case ".jpeg":
                case ".jpg":
                case ".gif":
                case ".bmp":
                case ".tif":
                case ".tiff":
                case ".tga":
                    return GetImageThumbnailFromDisk(expanded);
                case ".mov":
                case ".mp4":
                case ".avi":
                case ".m4v":
                case ".mkv":
                case ".mpg":
                case ".mpeg":
                case ".wmv":
                    return GetMovieThumbnailFromDisk(expanded);
            }
            return null;
        }

        private string GetImageThumbnailFromDisk(string path)
        {
            try
            {
                using Image<Rgba32> image = Image.Load<Rgba32>(path);
                image.Mutate(ctx => ctx.Resize(_buttonImageResize));
                return image.ToBase64String(_buttonImageFormat);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Exception while loading image {path}: {ex.Message}");
                return null;
            }
        }

        private double GetMovieDurationFromDisk(string path)
        {
            string ffProbe = Path.Combine(Model.Sys.s_instance.Config.FfMpegDirectory, "ffprobe.exe");
            if (File.Exists(ffProbe))
            {
                var inputArgs = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"";

                using var process = new Process
                {
                    StartInfo =
                    {
                        FileName = ffProbe,
                        Arguments = inputArgs,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
                using var stream = new MemoryStream();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if (double.TryParse(result, out double seconds))
                {
                    return seconds;
                }
                return 0;
            }
            return 0;
        }

        private string GetMovieThumbnailFromDisk(string path)
        {
            try
            {
                string ffMpeg = Path.Combine(Model.Sys.s_instance.Config.FfMpegDirectory, "ffmpeg.exe");
                if (File.Exists(ffMpeg))
                {
                    double duration = GetMovieDurationFromDisk(path);
                    if (duration <= 0)
                    {
                        return null;
                    }
                    double half = duration / 2;
                    var inputArgs = $"-ss {half} -i \"{path}\"";
                    var outputArgs = $"-vframes 1 -s {ButtonImageWidth}x{ButtonImageHeight} pipe:1.png";

                    using var process = new Process
                    {
                        StartInfo =
                    {
                        FileName = ffMpeg,
                        Arguments = $"{inputArgs} {outputArgs}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                    };
                    process.Start();
                    using var stream = new MemoryStream();
                    process.StandardOutput.BaseStream.CopyTo(stream);
                    process.WaitForExit();
                    return PngBytesToString(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"Exception while getting thumbnail from movie {path}: {ex.Message}");
            }
            return null;
        }
    }
}
