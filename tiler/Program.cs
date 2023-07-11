// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;


string path = System.IO.Path.GetFullPath("../../../../map");
string outpath = System.IO.Path.GetFullPath("../../../../public");
void CreateReferenceLevel(int startLevel)
{
    const int Rows = 8; 
    const int Cols = 16;

    var files = Directory.EnumerateFiles(path, "fmg_tile_*.png").ToList();
    
    Regex r = new Regex("fmg_tile_(?<n>\\d+)");
    
    string levelOutDir = Path.Combine(outpath, startLevel.ToString());
    Directory.CreateDirectory(levelOutDir);
    
    var bicubicResampler = new BicubicResampler();

    foreach (var file in files)
    {
        var m = r.Match(file);
        if(!m.Success)
            continue;
        var i =  int.Parse(m.Groups["n"].Value);
        int x = i % Cols;
        int y = i / Cols;
        Console.WriteLine($"{x},{y} {i} {Path.GetFileNameWithoutExtension(file)}");
        var outFilePath = Path.Combine(levelOutDir, $"{x}{y}.png");
        File.Delete(outFilePath);
        {
            {
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
                image.Save(outFilePath);
            }
        }

        // File.Copy(file, outFilePath);
    }
}


void CreatePrevLevel(int level, int levelCols, int levelRows)
{
    int newLevel = level - 1;
    string levelOutDir = Path.Combine(outpath, newLevel.ToString());
    Directory.CreateDirectory(levelOutDir);
    for (int i = 0; i < levelCols/2; i++)
    {
        for (int j = 0; j < levelRows/2; j++)
        {
            using var img = new SixLabors.ImageSharp.Image<Rgb24>(256, 256);
            var outFilePath = Path.Combine(levelOutDir, $"{i}{j}.png");
            img.Mutate(img =>
            {
            for (int k = 0; k < 4; k++)
            {
                int dx = k % 2;
                int dy = k / 2;
                var src = Image.Load(Path.Combine(outpath, level.ToString(), $"{(i * 2 + dx)}{j*2+dy}.png"));
                src.Mutate(s => s.Resize(new Size(128,128)));
                img.DrawImage(src, new Point(128*dx, 128*dy), 1);

            }
            });
            img.Save(outFilePath);
        }
    }
}

CreateReferenceLevel(4);
int cols = 16;
int rows = 8;
for (int i = 4; i >= 1; i--)
{
    CreatePrevLevel(i, cols, rows);
    cols /= 2;
    rows /= 2;
}

