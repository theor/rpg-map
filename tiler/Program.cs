// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;


string path = System.IO.Path.GetFullPath("../../../../map");
Console.WriteLine(path);
string outpath = System.IO.Path.GetFullPath("../../../../public");
WebpEncoder GetWebpEncoder() => new WebpEncoder
{
    Quality = 100,
    // NearLosslessQuality = 100,
    FileFormat = WebpFileFormatType.Lossless,
    // NearLossless = true,
};

IEnumerable<(string file, int x, int y)> ListTiles(int levelCols)
{
    Regex r = new Regex("fmg_tile_(?<n>\\d+)");
    var files = Directory.EnumerateFiles(path, "fmg_tile_*.png");
    foreach (var file in files)
    {
        var m = r.Match(file);
        if (!m.Success)
            continue;

        var i = int.Parse(m.Groups["n"].Value);
        int x = i % levelCols;
        int y = i / levelCols;
        yield return (file, x, y);
    }
}

void CreateReferenceLevel(int startLevel, int levelCols, int levelRows)
{
    string levelOutDir = Path.Combine(outpath, startLevel.ToString());
    Directory.CreateDirectory(levelOutDir);

    var bicubicResampler = new BicubicResampler();
    var webpEncoder = GetWebpEncoder();

    foreach (var (file, x, y) in ListTiles(levelCols))
    {
        Console.WriteLine($"{x},{y} {Path.GetFileNameWithoutExtension(file)}");
        var outFilePath = Path.Combine(levelOutDir, $"{x}.{y}.webp");
        var image = Image.Load(file);
        image.Mutate(i =>
        {
            i.Resize(new ResizeOptions
            {
                Sampler = bicubicResampler,
                Size = new Size(256, 256),
                Mode = ResizeMode.Stretch,
            });
        });
        image.SaveAsPng(Path.ChangeExtension(outFilePath, "png"));
        image.SaveAsWebpAsync(outFilePath, webpEncoder);

        // File.Copy(file, outFilePath);
    }
}


void CreatePrevLevel(int level, int levelCols, int levelRows)
{
    int newLevel = level - 1;
    string levelOutDir = Path.Combine(outpath, newLevel.ToString());
    Directory.CreateDirectory(levelOutDir);
    var webpEncoder = GetWebpEncoder();
    for (int i = 0; i < levelCols / 2; i++)
    {
        for (int j = 0; j < levelRows / 2; j++)
        {
            using var img = new SixLabors.ImageSharp.Image<Rgb24>(256, 256);
            var outFilePath = Path.Combine(levelOutDir, $"{i}.{j}.webp");
            img.Mutate(img =>
            {
                for (int k = 0; k < 4; k++)
                {
                    int dx = k % 2;
                    int dy = k / 2;
                    var src = Image.Load(Path.Combine(outpath, level.ToString(), $"{(i * 2 + dx)}.{j * 2 + dy}.png"));
                    src.Mutate(s => s.Resize(new Size(128, 128)));
                    img.DrawImage(src, new Point(128 * dx, 128 * dy), 1);

                }
            });
            
            img.SaveAsPng(Path.ChangeExtension(outFilePath, "png"));
            img.SaveAsWebpAsync(outFilePath, webpEncoder);
        }
    }
}

void Split(int startLevel, int levelCols, int levelRows, int countPerTile)
{
    int ToNextNearest(int x)
    {
        if (x < 0)
        {
            return 0;
        }
        --x;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        return x + 1;
    }

    string levelOutDir = Path.Combine(outpath, startLevel.ToString());
    Directory.CreateDirectory(levelOutDir);

    var bicubicResampler = new BicubicResampler();

    var webpEncoder = GetWebpEncoder();
    foreach (var (file, x, y) in ListTiles(levelCols))
    {
        var image = Image.Load(file);
        int sourceSize = image.Size.Width / countPerTile;

        Console.WriteLine($"{x} {y} {file}");
        for (int i = 0; i < countPerTile; i++)
        {
            for (int j = 0; j < countPerTile; j++)
            {
                var xx = x*countPerTile+i;
                var yy = y*countPerTile+j;
                var outFilePath = Path.Combine(levelOutDir, $"{xx}.{yy}.webp");
                Console.WriteLine($"    {xx} {yy}");
                var i1 = i;
                var j1 = j;
                var clone = image.Clone(img =>
                {
                    img.Crop(new Rectangle(i1 * sourceSize, j1 * sourceSize, sourceSize, sourceSize))
                        .Resize(new ResizeOptions
                        {
                            // Sampler = bicubicResampler,
                            Size = new Size(256, 256),
                            Mode = ResizeMode.Stretch,
                        });
                });
                clone.SaveAsWebpAsync(outFilePath, webpEncoder);

            }
        }
    }
}

int cols = 16;
int rows = 8;
// Split(7, cols, rows, 8);
// Split(6, cols, rows, 4);
// Split(5, cols, rows, 2);
CreateReferenceLevel(4, cols, rows);
for (int i = 4; i >= 1; i--)
{
    CreatePrevLevel(i, cols, rows);
    cols /= 2;
    rows /= 2;
}