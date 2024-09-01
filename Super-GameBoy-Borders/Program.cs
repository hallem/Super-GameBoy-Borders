using System.Diagnostics;
using Super_GameBoy_Borders;

ConsoleFileOutput output = new ConsoleFileOutput();
Console.SetOut(output);

//const string basePath = "/Users/michael/Downloads/Roms/Super GameBoy Borders/espiox_SGB_Borders";
const string basePath = "/Users/michael/Downloads/Roms/Super GameBoy Borders/SGB Borders/Super Game Boy Borders";
//const string convertPath = "_THESE NEED CONVERTING TO SGB";
const string convertPath = "";

const string executableName = "superfamiconv_macos_x86-64_v0.10.0/superfamiconv";
const string outPalette = "snes.palette";
const string outTiles = "snes.tiles";
const string outMap = "snes.map";
//const string outTilesImage = "tiles.png";
//const string argumentsFormat = "-v --in-image \"{0}\" --out-palette {1} --out-tiles {2} --out-map {3} --out-tiles-image {4}";
const string argumentsFormat = "-v --in-image \"{0}\" --out-palette {1} --out-tiles {2} --out-map {3}";
//const string argumentsFormat = "--in-image \"{0}\" --out-palette {1} --out-tiles {2} --out-map {3}";

string toConvertPath = Path.Combine(basePath, convertPath);
string[] files = Directory.GetFiles(toConvertPath, "*.png", SearchOption.AllDirectories).OrderBy(f => f).ToArray();
List<string> errors = [];

foreach (string file in files)
{
    Process process = new Process();

    process.StartInfo.FileName = executableName;
    process.StartInfo.Arguments = string.Format(argumentsFormat, file, outPalette, outTiles, outMap);
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.Start();

    string processOutput = process.StandardOutput.ReadToEnd();
    string processError = process.StandardError.ReadToEnd();

    process.WaitForExit();

    if (!string.IsNullOrEmpty(processError))
    {
        Console.WriteLine(processOutput.Trim());
        Console.WriteLine(processError);
    }

    if (process.ExitCode != 0)
    {
        errors.Add(file);
        Console.WriteLine();
        continue;
    }

    byte[] sgbBytes = new byte[8192 + 1792 + 128];
    byte[] tileBytes = File.ReadAllBytes(outTiles); //new byte[8192];
    byte[] mapBytes = File.ReadAllBytes(outMap); //new byte[1792];
    byte[] paletteBytes = File.ReadAllBytes(outPalette); //new byte[128];

    if (tileBytes.Length <= 8192 && mapBytes.Length <= 1792 && paletteBytes.Length <= 128)
    {
        tileBytes.CopyTo(sgbBytes, 0);
        mapBytes.CopyTo(sgbBytes, 8192);
        paletteBytes.CopyTo(sgbBytes, 8192 + 1792);

        string fileName = Path.GetFileNameWithoutExtension(file) + ".sgb";
        string outputFile;

        if (!string.IsNullOrEmpty(convertPath))
        {
            string splitPath = file.Split([convertPath], StringSplitOptions.RemoveEmptyEntries).Last();
            string subdirectory = Path.GetDirectoryName(splitPath)!;
            string outputDirectory = Path.Join(basePath, subdirectory);

            outputFile = Path.Combine(outputDirectory, fileName);
        }
        else
        {
            outputFile = Path.Combine(basePath, fileName);
        }

        //Console.WriteLine($"Writing \"{outputFile}\"...");
        File.WriteAllBytes(outputFile, sgbBytes);
        //Console.WriteLine("Complete!");
    }
    else
    {
        errors.Add(file);
        Console.WriteLine();
        Console.WriteLine($"Error Converting \"{file}\"...");
        Console.WriteLine($"  Tile Bytes:    {tileBytes.Length,5} / 8192 bytes {(tileBytes.Length > 8192 ? "**" : "" )}");
        Console.WriteLine($"  Map Bytes:     {mapBytes.Length,5} / 1792 bytes {(mapBytes.Length > 1792 ? "**" : "" )}");
        Console.WriteLine($"  Palette Bytes: {paletteBytes.Length,5} /  128 bytes {(paletteBytes.Length > 128 ? "**" : "" )}");
        Console.WriteLine("");
    }

    //Console.WriteLine("");

    File.Delete(outPalette);
    File.Delete(outTiles);
    File.Delete(outMap);
    //File.Delete(outTilesImage);
}

Console.WriteLine("Unable to convert files:");

foreach (string file in errors)
{
    Console.WriteLine($"  {file}");
}

output.Close();
