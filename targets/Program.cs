using GlobExpressions;
using static Bullseye.Targets;
using static SimpleExec.Command;

const string clean = "clean";
const string build = "build";
const string test = "test";
const string format = "format";
const string publish = "publish";

Target(clean,
ForEach("publish", "**/bin", "**/obj"),
dir =>
{
    foreach (var d in from d in Glob.Directories(".", dir)
                      where Directory.Exists(d)
                      select d)
    {
        Console.WriteLine($"Cleaning {d}");
        Directory.Delete(d, true);
    }
});

Target(format, () =>
{
    Run("dotnet", "tool restore");
    Run("dotnet", "format --verify-no-changes");
});

Target(build, DependsOn(format), () => Run("dotnet", "build . -c Release"));

Target(test, DependsOn(build),
    () =>
    {
        foreach (var file in Glob.Files(".", "tests/**/*.csproj"))
        {
            Run("dotnet", $"test {file} -c Release --no-restore --no-build --verbosity=normal");
        }
    });

Target(publish, DependsOn(test),
    ForEach("src/WebUI", "tools/Application.Tools"),
    project => Run("dotnet",
            $"publish {project} -c Release -f net7.0 -o ./publish --no-restore --no-build --verbosity=normal"));

Target("default", DependsOn(publish), () => Console.WriteLine("Done!"));
await RunTargetsAndExitAsync(args);

public partial class Program
{
    protected Program() { }
}