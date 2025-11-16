using System.CommandLine;
using Wtrfll.Translations.Exporting;
using Wtrfll.Translations.Parsing;

var rootCommand = new RootCommand("wtrfll translation tooling");

var importCommand = new Command("import", "Normalize a translation file into wtrfll format");
var formatOption = new Option<string>(
    name: "--format",
    description: "Input format (legacy-json, zefania-xml)",
    getDefaultValue: () => "legacy-json");
var codeOption = new Option<string>("--code", "Translation code (e.g., RVR1960)") { IsRequired = true };
var nameOption = new Option<string>("--name", "Human friendly name") { IsRequired = true };
var languageOption = new Option<string>("--language", "ISO language code (e.g., es, en)") { IsRequired = true };
var inputOption = new Option<FileInfo>("--input", "Input file path") { IsRequired = true };
var outputOption = new Option<FileInfo>("--output", "Normalized JSON output file") { IsRequired = true };

importCommand.AddOption(formatOption);
importCommand.AddOption(codeOption);
importCommand.AddOption(nameOption);
importCommand.AddOption(languageOption);
importCommand.AddOption(inputOption);
importCommand.AddOption(outputOption);

importCommand.SetHandler(async (format, code, name, language, input, output) =>
{
    IBibleParser parser = format switch
    {
        "legacy-json" => new LegacyJsonBibleParser(),
        "zefania-xml" => new ZefaniaXmlBibleParser(),
        _ => throw new ArgumentException($"Unknown format '{format}'."),
    };

    var exporter = new NormalizedJsonExporter();
    var document = await parser.ParseAsync(new ParseRequest(input.FullName, code, name, language));
    await exporter.ExportAsync(document, output.FullName);

    Console.WriteLine($"✅ Generated normalized translation at {output.FullName}");
}, formatOption, codeOption, nameOption, languageOption, inputOption, outputOption);

rootCommand.AddCommand(importCommand);

return await rootCommand.InvokeAsync(args);
