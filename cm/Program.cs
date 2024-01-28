
using cm;
using Microsoft.VisualBasic.FileIO;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

//the commands
#region הגדרת הפקודות
var bundleCommand = new Command("bundle", "Bundle code to a single file");
var createRspCommand = new Command("create-rsp", "Create a response file with a ready command");

#endregion

#region הגדרת אופציות  של מיקום שפה עורך הערות שורות ריקות וכו
var outputOption = new Option<FileInfo>("--output", "File path and name")
{
    IsRequired = true,  // Make it optional
};
var languageOption = new Option<string>("--language", "Programming language to bundle files")
{
    IsRequired = true,
};
var author = new Option<string>("--author", "Name of the author");
var noteOfDescription = new Option<bool>("--note", "Add name and path of the bundle");
var removeEmptyLines = new Option<bool>("--remove", "remove empty lines from the file");
var sort = new Option<bool>("--sort", "sort the file according the code type (default: alpha bettic order)");

#endregion

#region הוספת קיצור לכל אופציה
outputOption.AddAlias("-o");
languageOption.AddAlias("-l");
author.AddAlias("-a");
noteOfDescription.AddAlias("-n");
removeEmptyLines.AddAlias("-r");
sort.AddAlias("-s");
#endregion

#region   bundleCommand הוספת אופציות לפקודה 
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOfDescription);
bundleCommand.AddOption(removeEmptyLines);
bundleCommand.AddOption(author);
bundleCommand.AddOption(sort);
#endregion

#region createRspCommand הוספת אופציות לפקודה 
createRspCommand.AddOption(outputOption);
createRspCommand.AddOption(sort);
createRspCommand.AddOption(author);
createRspCommand.AddOption(languageOption);
createRspCommand.AddOption(noteOfDescription);
createRspCommand.AddOption(removeEmptyLines);
#endregion

#region create-rsp הגדרת הפקודה 
createRspCommand.SetHandler((output, language, author, note, removeEmptyLines, sort) =>
{
    CreateRspCommand(output.FullName, author, note, removeEmptyLines, sort, language);
    // Bundle files for a specific language
}, outputOption, languageOption, author, noteOfDescription, removeEmptyLines, sort);
#endregion

#region create-rsp    הגדרת הפונקציונאליות של
static void CreateRspCommand(string outputOption, string author, bool noteOfDescription, bool removeEmptyLines, bool sort, string languageOption)
{
    if (string.IsNullOrEmpty(outputOption))
    {
        Console.WriteLine("file path is required");
        outputOption = Console.ReadLine();
    }

    // Build the command based on user input
    var commandBuilder = new System.Text.StringBuilder("bundle");

    if (outputOption != null)
        commandBuilder.Append($" --output {Path.GetFileName(outputOption)}");

    if (!string.IsNullOrEmpty(author))
        commandBuilder.Append($" --author {author}");

    if (languageOption != null)
        commandBuilder.Append($" --language {languageOption}");

    if (noteOfDescription)
        commandBuilder.Append(" --note");

    if (removeEmptyLines)
        commandBuilder.Append(" --remove");

    if (sort)
        commandBuilder.Append(" --sort");

    // Save the command to the response file
    var rspFile = Path.Combine(Environment.CurrentDirectory, "response.txt");
    File.WriteAllText(rspFile, commandBuilder.ToString());
    Console.WriteLine($"Response file created successfully at '{rspFile}'.");
}
#endregion

#region   bundleCommand הגדרת הפקודה 
bundleCommand.SetHandler((output, languageOption, note, removeEmptyLines, author, sort) =>
{/// Validate and sanitize user inputs

    if (!ValidateOutputPath(output.FullName))
    {
        Console.WriteLine("Invalid output path. Please provide a valid file path.");
        return;
    }
    // Bundle files from all languages
    string[] files = Directory.GetFiles(Environment.CurrentDirectory);
    BundleFiles(output.FullName, files, note, removeEmptyLines, author, sort, languageOption);

}, outputOption, languageOption, noteOfDescription, removeEmptyLines, author, sort);
#region בדיקה האם הניתוב שהכניסו תקין 
bool ValidateOutputPath(string outputPath)
{
    try
    {
        var fileInfo = new FileInfo(outputPath);
        return fileInfo.Directory.Exists;
    }
    catch (Exception)
    {
        return false;
    }
}

#endregion
#endregion
#region הגדרות פקודה בסיסיות
var rootCommand = new RootCommand("Root command for File Bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
return rootCommand.Invoke(args);
#endregion
#region סגירה כללית לקובץ
static void BundleFiles(string outputPath, string[] files, bool note, bool removeEmptyLines, string author, bool sort, string Slanguage)
{
    Lan[] currentLen = new Lan[Slanguage.Length];
    //bool exist=false;
    Lan[] supported = new Lan[]
    {
        new Lan("c#", ".cs"),
        new Lan("Java", ".java"),
        new Lan("Python", ".py"),
        new Lan("JavaScript", ".js"),
        new Lan("Html", ".html"),
        new Lan("c++", ".cpp"),
        new Lan("c", ".c")
    };

    // Exclude files from specific directories (e.g., bin, debug, etc.)
    files = ExcludeFilesFromDirectories(files, new string[] { "bin", "obj", "debug" });

    if (sort)
    {
        SortTheFileAccCode(files);
    }
    else
    {
        SortTheFileAccAlph(files);
    }
    string outputFilePath = Path.Combine(outputPath);
    List<string> allTextFiles = new List<string>();
    if (!string.IsNullOrEmpty(author))
    {
        allTextFiles.Add($"//---------author: {author} -----------");
    }
    if (Slanguage != "all")
    {
        string[] Stringlanguage = Slanguage.Split(',');
        currentLen = new Lan[Stringlanguage.Length];
        for (int i = 0; i < Stringlanguage.Length; i++)
        {
            string option = Stringlanguage[i];
            var supportedLanguage = supported.FirstOrDefault(lang => lang.FileName.Equals(option, StringComparison.OrdinalIgnoreCase));
            if (supportedLanguage != null)
                currentLen[i++] = supportedLanguage;
        }
    }
    foreach (string file in files)
    {
        bool fileAdded = false;
        string text="";
        if (Slanguage != "all")
        {
            
            foreach (Lan item in currentLen)
            {
                if (file.EndsWith(item.FileExtension))
                {
                    if (note)
                    {
                        allTextFiles.Add($"//filePath:--------------{file}----------------");
                    }

                     text = File.ReadAllText(file);
                    if (removeEmptyLines)
                    {
                        text = string.Join(Environment.NewLine, text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
                    }
                    break; // לאחר שהקובץ נוסף, יש לצאת מלולאה הפנימית
                }
            }
        }
        if (/*!fileAdded &&*/ Slanguage == "all")
        {
            if (note)
            {
                allTextFiles.Add($"//filePath:--------------{file}----------------");
            }

             text = File.ReadAllText(file);

            if (removeEmptyLines)
            {
                text = string.Join(Environment.NewLine, text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            }
        }
        allTextFiles.Add(text);
    }
    File.WriteAllText(outputFilePath, string.Join(Environment.NewLine, allTextFiles));
    Console.WriteLine($"the files bundled successfully into {outputFilePath}");
}
#endregion

#region מיון הקבצים לפי סדר ה א-ב
static string[] SortTheFileAccAlph(string[] files)
{
    return files = files.OrderBy(file => file).ToArray();
}
#endregion

#region מיון הקבצים לפי קוד
static string[] SortTheFileAccCode(string[] files)
{
    Array.Sort(files, (a, b) => Path.GetExtension(a).CompareTo(Path.GetExtension(b)));
    return files;
}
#endregion

#region הכנסה לקובץ התגובה רק קבצים שהם קבצי קוד
static string[] ExcludeFilesFromDirectories(string[] files, string[] excludedDirectories)
{
    List<string> filteredFiles = new List<string>();

    foreach (string file in files)
    {
        // Check if the file is not in any of the excluded directories
        string directory = Path.GetDirectoryName(file).ToLower();
        bool excludeFile = false;

        foreach (string excludedDirectory in excludedDirectories)
        {
            if (directory.Contains(excludedDirectory.ToLower()))
            {
                excludeFile = true;
                break;
            }
        }

        if (!excludeFile)
        {
            filteredFiles.Add(file);
        }
    }
    return filteredFiles.ToArray();
}
#endregion
// Add validation functions
#region בדיקה האם בחרו  בשפה  תקינה
static bool ValidateLanguage(string language)
{
    // Add more sophisticated validation if needed
    return !string.IsNullOrEmpty(language);
}
#endregion

