//using DokanLab;
using DokanNet;
using DokanNet.Logging;
using UserFileSystem;

char driveLetter = 'Y';

using (ConsoleLogger consoleLogger = new("[Dokan]"))
using (Dokan dokan = new(consoleLogger))
{
    string mountPoint = $"{driveLetter}:\\";
    VirtualFileSystem myFs = new();
    DokanInstanceBuilder dokanInstanceBuilder = new DokanInstanceBuilder(dokan)
        .ConfigureLogger(() => consoleLogger)
        .ConfigureOptions(options =>
        {
            options.Options = DokanOptions.DebugMode;
            options.MountPoint = mountPoint;
        });
    using DokanInstance dokanInstance = dokanInstanceBuilder.Build(myFs);
    Console.ReadLine();
}
