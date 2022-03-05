using GlobExpressions;
using static Bullseye.Targets;
using static SimpleExec.Command;

const string Clean = "clean";
const string Build = "build";
const string Test = "test";
const string Format = "format";
const string Publish = "publish";

Target(Clean,
ForEach("publish", "**/bin", "**/obj"),
dir =>
{
    foreach (var d in Glob.Directories(".", dir))
    {
        if (Directory.Exists(d))
        {
            Console.WriteLine($"Cleaning {d}");
            Directory.Delete(d, true);
        }
    }
});

Target(Format, () =>
{
    Run("dotnet", "tool restore");
    Run("dotnet", "dotnet-format --verify-no-changes");
});

Target(Build, DependsOn(Format), () => Run("dotnet", "build . -c Release"));

Target(Test, DependsOn(Build),
    () =>
    {
        foreach (var file in Glob.Files(".", "tests/**/*.csproj"))
        {
            Run("dotnet", $"test {file} -c Release --no-restore --no-build --verbosity=normal");
        }
    });

Target(Publish, DependsOn(Test),
    ForEach("src/WebUI", "tools/Application.Tools"),
    project =>
    {
        Run("dotnet",
            $"publish {project} -c Release -f net6.0 -o ./publish --no-restore --no-build --verbosity=normal");
    });

Target("default", DependsOn(Publish), () => Console.WriteLine("Done!"));
await RunTargetsAndExitAsync(args);