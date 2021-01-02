using System;
using System.IO;
using System.Threading.Tasks;
using NDesk.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace imgResizer
{
    class Program
    {
        static bool _showHelp = false;
        static bool _useWatchMode = false;
        static string _source = string.Empty;
        static string _output = string.Empty;
        static int _width = 100;
        static int _height = 0;
        static int _exitCode = 0;
        static string _watchDir = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "in";
        static string _watchOutDir = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "out";

        static FileSystemWatcher _fileSystemWatcher;

        static async Task Main(string[] args)
        {
            var p = new OptionSet() {
                { "s|imageSource=", "image file path to transform.", v => _source = v },
                { "o|imageOut=", "new resized image path", v => _output = v },
                { "w|imageWidth=", "width for new image, must be an integer", (int v) => _width = v },
                { "h|imageHeight=", "height for new image, must be an integer", (int v) => _height = v },
                { "wm|watchMode=", "use watch mode, only valid option is true", (bool v) => _useWatchMode = v},
                { "wid|watchInDir=", "directory to watch for changes", v => _watchDir = v },
                { "wod|watchOutDir=", "directory where files being watched output to", v=> _watchOutDir = v},
                { "help",  "show this message and exit", v => _showHelp = v != null }
            };

            try
            {
                p.Parse(args);

                if (_showHelp)
                {
                    ShowHelp(p);
                    Environment.Exit(_exitCode);
                }


                if (_useWatchMode)
                {
                    if (!Directory.Exists(_watchDir))
                        Directory.CreateDirectory(_watchDir);

                    if (!Directory.Exists(_watchOutDir))
                        Directory.CreateDirectory(_watchOutDir);

                    Console.WriteLine(string.Format("Watching {0} and output will go to {1}", _watchDir, _watchOutDir));
                    using (_fileSystemWatcher = new FileSystemWatcher())
                    {
                        _fileSystemWatcher.Path = _watchDir;
                        _fileSystemWatcher.Created += OnAdded;
                        _fileSystemWatcher.InternalBufferSize = 65536;
                        _fileSystemWatcher.NotifyFilter = NotifyFilters.FileName;
                        _fileSystemWatcher.EnableRaisingEvents = true;
                        await Task.Delay(-1);
                    }
                }
                else
                {
                    await ResizeImage(_source, _width, _height, _output);
                }
            }
            catch (OptionException ex)
            {
                Console.Write("imgResizer: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try 'imgResizer --help' for more information");
                _exitCode = 1;
            }
            Environment.Exit(_exitCode);
        }

        private static void OnAdded(object source, FileSystemEventArgs e)
        {
            Task.Run(async () =>
            {
                Console.WriteLine($"{e.ChangeType} - File: {e.FullPath} => {_watchOutDir + Path.DirectorySeparatorChar + e.Name}");
                try
                {
                    await ResizeImage(e.FullPath, _width, _height, _watchOutDir + Path.DirectorySeparatorChar + e.Name);
                }
                catch (Exception ex)
                {
                    Console.Write("imgResizer: ");
                    Console.WriteLine(ex.Message);
                    _exitCode = 1;
                }
            });
        }

        static async Task ResizeImage(string source, int width, int height, string output)
        {
            try
            {
                using (Image image = Image.Load(source))
                {
                    image.Mutate(x => x.Resize(width, height));
                    await image.SaveAsync(output);
                    image.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.Write("imgResizer: ");
                Console.WriteLine(ex.Message);
                _exitCode = 1;
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage Examples:");
            Console.WriteLine(" ");
            Console.WriteLine("Single Image:");
            Console.WriteLine("imgResizer -s {PATH_TO_SOURCE_IMG} -o {PATH_FOR_RESIZED_IMG} -w {INT_WIDTH} -h {INT_HEIGHT}");
            Console.WriteLine(" ");
            Console.WriteLine("Watch a directory:");
            Console.WriteLine("imgResizer -wm true -wid {DIR_TO_WATCH} -wod {OUTPUT_DIR} -w {INT_WIDTH} -h {INT_HEIGHT}");
            Console.WriteLine(" ");
            Console.WriteLine("Notes:");
            Console.WriteLine("If no height or width is specified, 100 width by 0 height is used for the resize. Pass 0 in for either width or height to maintain aspect ratio.");
            Console.WriteLine(" ");
            Console.WriteLine("If no watch directory is defined, it will create 'in' and 'out' directories where the imgResizer executable lives.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
