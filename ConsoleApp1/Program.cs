using System.Collections;
using System.CommandLine;
using System.Diagnostics;
using static Program;
internal class Program
{
    public enum SortOptions
    {
        type,
        name
    }
    private static void Main(string[] args)
    {
        var outputOption = new Option<FileInfo>("--output", "File path");
        outputOption.AddAlias("--o");
        var languageOption = new Option<List<string>>("--language",
        "language of list.")
        .FromAmong("C#", "Java", "Python", "C++", "C", "all");
        languageOption.IsRequired = true;
        languageOption.AllowMultipleArgumentsPerToken = true;
        languageOption.AddAlias("--l");
        var noteOption = new Option<bool>("--note", "bundle note");
        noteOption.AddAlias("--n");
        var sortOption = new Option<string>(
        name: "--sort",
        description: "sort the code files by file-name or code-kind",
        getDefaultValue: () => "name");
        sortOption.AddAlias("--srt");
        sortOption.Arity = ArgumentArity.ZeroOrOne;
        var removeEmptyLinesOption = new Option<bool>("--remove", "Remove empty Lines ");
        removeEmptyLinesOption.AddAlias("--rel");
        var authorOption = new Option<string>("--author", "Author's name ");
        authorOption.AddAlias("--aut");
        var bundleCommand = new Command("bundle", "Bundle code ...")
{
    languageOption,
    outputOption,
    noteOption,
    sortOption,
    removeEmptyLinesOption,
    authorOption,
};
        var createRspCommand = new Command("create-rsp", "Create a response file ...");
        var rootCommand = new RootCommand("Root command for File Bundler CLI");
        bundleCommand.SetHandler((output, languages, note, sort, removeEL, author) =>
        {
            try
            {
                // נסה לבצע את כל הלוגיקה המרכזית בלחיצה אחת
                BundleFiles(output, languages, note, sort, removeEL, author);
                Console.WriteLine("Bundle process completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }, outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);


        createRspCommand.SetHandler(() =>
        {
            Console.WriteLine("Welcome to the create-rsp command!");
            Console.WriteLine("Please provide the desired values for each command option:");

            Console.Write("Output file path and name: ");
            string outputFile = Console.ReadLine();

            Console.Write("File path and name for --output option: ");
            string outputFilePath = Console.ReadLine();

            Console.Write("Languages for --language option (separated by space): ");
            string languagesInput = Console.ReadLine();
            string[] languages = languagesInput.Split(' ');

            Console.Write("Add code source note (--note option)? (Y/N): ");
            bool addNote = Console.ReadLine().Equals("Y", StringComparison.OrdinalIgnoreCase);

            Console.Write("Sort the files (--sort option)?(name/type): ");
            string sortOptionInput = Console.ReadLine();
            SortOptions sortOptions = sortOptionInput.Equals("type", StringComparison.OrdinalIgnoreCase)
            ? SortOptions.type :
            SortOptions.name;
            Console.Write("Remove empty lines (--remove-empty-lines option)? (Y/N): ");
            bool removeEmptyLines = Console.ReadLine().Equals("Y", StringComparison.OrdinalIgnoreCase);
            Console.Write("Author name (--author option): ");
            string authorName = Console.ReadLine();

            string rspContent = $"--output {outputFilePath}{Environment.NewLine}" +
                                $"--language {string.Join(" ", languages)}{Environment.NewLine}" +
                                $"--note {addNote}{Environment.NewLine}" +
                                $"--sort {sortOptions}{Environment.NewLine}" +
                                $"--remove {removeEmptyLines}{Environment.NewLine}" +
                                $"--author {authorName}";

            string rspFilePath = $@"{Environment.CurrentDirectory}\{outputFile}.rsp";
            File.WriteAllText(rspFilePath, rspContent);

            Console.WriteLine($"Response file '{rspFilePath}' has been created successfully!");
        });
        rootCommand.AddCommand(bundleCommand);
        rootCommand.AddCommand(createRspCommand);
        rootCommand.Invoke(args);
    }
    // הפונקציה המבצעת את הלוגיקה הראשית של ה-Bundle
    public static void BundleFiles(FileInfo output, List<string> languages, bool note, string sort, bool removeEL, string author)
    {
        var languageExtensions = new Dictionary<string, List<string>>()
        {
        { "java", new List<string>() { ".java" } },
        { "c#", new List<string>() { ".cs" } },
        { "python", new List<string>() { ".pt" } },
        { "c++", new List<string>() { ".cpp", ".h" } },
        { "c", new List<string>() { ".c", ".h" } }
        };

        using (var bundleFile = new StreamWriter(output.FullName, false))
        {
            // רישום שם היוצר בתוך קובץ ה-bundle
            if (!string.IsNullOrEmpty(author))
            {
                bundleFile.WriteLine($"// Author: {author}");
            }
            List<string> allowedExtensionsForLanguage = GetAllowedExtensionsForLanguage(languages.FirstOrDefault(), languageExtensions);

            string path = Directory.GetCurrentDirectory();

            List<string> filesList = GetFilteredFiles(path, allowedExtensionsForLanguage);
            string[] files = filesList.ToArray();
            if (note)
            {
                bundleFile.WriteLine($"// Note: {path}");
            }
            foreach (var file in files)
            {
                if (note)
                {
                    bundleFile.WriteLine($"// Note: {path}");
                }
                var sourceCode = File.ReadAllLines(file);
               //מיון
                if (sort == "type")
                {
                    Array.Sort(files, CompareByType);
                }
                else
                {
                    Array.Sort(files, CompareByName);
                }
                if (removeEL)
                {
                    sourceCode = sourceCode.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                }
                foreach (var line in sourceCode)
                {
                    bundleFile.WriteLine(line);
                }
            }
        }
    }
    public static int CompareByName(string x, string y)
    {
        return string.Compare(Path.GetFileName(x), Path.GetFileName(y));
    }
    public static int CompareByType(string x, string y)
    {
        string xExtension = Path.GetExtension(x).ToLower();
        string yExtension = Path.GetExtension(y).ToLower();
        return string.Compare(xExtension, yExtension);
    }
    // פונקציה לסינון והשגת הקבצים המתאימים
    public static List<string> GetFilteredFiles(string path, List<string> allowedExtensionsForLanguage)
    {
        return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
            .Where(file => !file.Contains("bin") &&
                            !file.Contains("obj") &&
                            !file.EndsWith(".dll") &&
                            !file.EndsWith(".exe") &&
                            !file.EndsWith(".vs") &&
                            allowedExtensionsForLanguage.Contains(Path.GetExtension(file)))
            .ToList();
    }
    public static List<string> GetAllowedExtensionsForLanguage(string selectedLanguage, Dictionary<string, List<string>> languageExtensions)
    {
        List<string> allowedExtensionsForLanguage;

        switch (selectedLanguage?.ToLower())
        {
            case "java":
                allowedExtensionsForLanguage = languageExtensions["java"];
                break;
            case "c#":
                allowedExtensionsForLanguage = languageExtensions["c#"];
                break;
            case "python":
                allowedExtensionsForLanguage = languageExtensions["python"];
                break;
            case "c++":
                allowedExtensionsForLanguage = languageExtensions["c++"];
                break;
            case "c":
                allowedExtensionsForLanguage = languageExtensions["c"];
                break;
            case "all":
                allowedExtensionsForLanguage = languageExtensions.SelectMany(kv => kv.Value).ToList();
                break;
            default:
                allowedExtensionsForLanguage = new List<string>();
                break;
        }
        return allowedExtensionsForLanguage;
    }
}