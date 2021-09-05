using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GlobExpressions;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace targets
{
    class Program
    {
        const string Clean = "clean";
        const string Build = "build";
        const string Test = "test";
        const string Format = "format";
        const string Publish = "publish";

        static async Task Main(string[] args)
        {
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
                Run("dotnet", "format --check");
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

            Target(Publish, DependsOn(Build),
                ForEach("src/WebUI"),
                project =>
                {
                    Run("dotnet",
                        $"publish {project} -c Release -f net5.0 -o ./publish --no-restore --no-build --verbosity=normal");
                });

            Target("default", DependsOn(Publish), () => Console.WriteLine("Done!"));
            await RunTargetsAndExitAsync(args);
        }
    }
}
