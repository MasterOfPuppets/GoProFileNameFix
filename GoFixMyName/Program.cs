// Paulo Oliveira
// 21/10/2023

string[] prefixes = { "GH", "GX", "GP", "GL" };

Console.WriteLine("=============================");
Console.WriteLine("= Go fix my pro file names. =");
Console.WriteLine("=============================");
Console.WriteLine("");
Console.WriteLine("\tIf there are GoPro files to process, I will swap the 'zz' chapter number, with the 'xxxxx' file number.");
Console.WriteLine("\tAfter I'm done, you'll have perfectly ordered GoPro files in your directory,");
Console.WriteLine("\tinstead of the mess of filenames that the GoPro generates.");
Console.WriteLine("\t(btw, if you run this tool three times in the same directory,");
Console.WriteLine("\tit will reset the filenames to their original state.)");
Console.WriteLine("");

string path = AppDomain.CurrentDomain.BaseDirectory;
List<FileInfo> goProFiles = new();
if (args.Length == 1)
{
    path = args[0];
    if (!Directory.Exists(path))
    {
        Console.WriteLine($"Couldn't get to: '{path}'. I'm leaving.");
        Console.WriteLine("Bye");
        Environment.Exit(2);
    }
} 
else
{
    Console.WriteLine("\tYou can pass a path as an argument, for me to process.");
}


DirectoryInfo folder = new(path);
foreach(FileInfo file in folder.GetFiles())
{
    if (IsGoProFile(file.Name))
    {
        goProFiles.Add(file);
    }
}

if (goProFiles.Count == 0)
{
    Console.WriteLine($"The path: '{path}'. Has no GoPro files in it. I'm leaving.");
    Console.WriteLine("Bye");
    Environment.Exit(0);
}

Console.WriteLine($"The path: '{path}', has {goProFiles.Count} GoPro files in it.\nIf you really want to rename these files answer 'yes', otherwise I'll leave.");
string? answer = Console.ReadLine();
if (answer != "yes")
{
    Console.WriteLine("Bye");
    Environment.Exit(0);
}

foreach(FileInfo file in goProFiles)
{
    string newFileName = ChangeFileName(file.Name);
    Console.WriteLine($"Renaming '{file.Name}' to '{newFileName}'");
    file.MoveTo(Path.Combine(file.DirectoryName ?? "/", newFileName));
}

Console.WriteLine($"I'm done here, bye.");
Environment.Exit(0);

// Helpers
bool IsGoProFile(string fileName)
{
    return fileName.Length == 12 && prefixes.Where(p => fileName.StartsWith(p)).Any();
}
string ChangeFileName(string oldFileName)
{
    string ext = oldFileName.Substring(oldFileName.LastIndexOf('.') + 1);
    string ret = oldFileName.Substring(0, 2) + oldFileName.Substring(4, 4) + oldFileName.Substring(2, 2) + "." + ext;
    return ret;
}