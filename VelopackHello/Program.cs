using Velopack;

VelopackApp.Build().Run();

var baseDirectory = AppContext.BaseDirectory;
var isDevMode = baseDirectory.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
var appVersion = isDevMode ? "Development build (dev mode)" : VelopackRuntimeInfo.VelopackProductVersion.ToString();
var velopackVersion = VelopackRuntimeInfo.VelopackNugetVersion.ToString();

Console.CursorVisible = false;
Console.Clear();
Console.WriteLine("VelopackHello");
Console.WriteLine($"App version: {appVersion}");
Console.WriteLine($"Velopack version: {velopackVersion}");
Console.WriteLine();
Console.WriteLine("Hora actual:");
Console.WriteLine();
Console.WriteLine("Pulsa cualquier tecla para cerrar.");

var clockRow = Console.CursorTop - 3;

while (!Console.KeyAvailable) {
    Console.SetCursorPosition(0, clockRow);
    Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadRight(Console.WindowWidth - 1));
    Thread.Sleep(200);
}

Console.ReadKey(intercept: true);
