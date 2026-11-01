using System.Reflection;

// Lists a game type's properties and methods with their types, read from the interop assemblies
// without loading them.
//   dotnet run --project tools/members -- MoonWorld 'Belt|Conveyor'
//   dotnet run --project tools/members -- Glyph .           an enum lists its values
//   dotnet run --project tools/members -- '*' 'Splitter'    search every type
// The game is MS_GAME_DIR, else the default Steam library (same defaults as Game.props).
if (args.Length != 2)
{
    Console.Error.WriteLine("usage: members <type|*> <memberRegex>");
    return 1;
}

var game = Environment.GetEnvironmentVariable("MS_GAME_DIR");
if (string.IsNullOrEmpty(game))
    game = Path.Combine(
        OperatingSystem.IsWindows()
            ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share"),
        "Steam", "steamapps", "common", "Moon Sweepers");
var dir = Path.Combine(game, "BepInEx", "interop");
if (!Directory.Exists(dir))
{
    Console.Error.WriteLine($"BepInEx interop not found in: {game} (run the game once with BepInEx and the patcher). Set MS_GAME_DIR to override.");
    return 1;
}

var paths = Directory.GetFiles(dir, "*.dll")
    .Concat(Directory.GetFiles(Path.Combine(dir, "..", "core"), "*.dll"))
    .Concat(Directory.GetFiles(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "*.dll"))
    .ToList();
var mlc = new MetadataLoadContext(new PathAssemblyResolver(paths), typeof(object).Assembly.GetName().Name);
var asm = mlc.LoadFromAssemblyPath(Path.Combine(dir, "Assembly-CSharp.dll"));
var re = new System.Text.RegularExpressions.Regex(args[1]);
var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

foreach (var t in asm.GetTypes().Where(t => args[0] == "*" || t.Name == args[0]))
{
    var lines = new List<string>();
    if (t.IsEnum && (args[0] == t.Name || re.IsMatch(t.Name)))
        lines.Add("  E " + string.Join(", ", t.GetFields(BindingFlags.Static | BindingFlags.Public).Select(f => f.Name + "=" + f.GetRawConstantValue())));
    foreach (var p in t.GetProperties(flags).Where(p => re.IsMatch(p.Name)))
        lines.Add($"  P {(p.GetMethod?.IsStatic == true ? "static " : "")}{Pretty(p.PropertyType)} {p.Name} {(p.CanWrite ? "rw" : "ro")}");
    foreach (var m in t.GetMethods(flags).Where(m => re.IsMatch(m.Name) && !m.IsSpecialName))
        lines.Add($"  M {(m.IsStatic ? "static " : "")}{Pretty(m.ReturnType)} {m.Name}({string.Join(", ", m.GetParameters().Select(x => Pretty(x.ParameterType) + " " + x.Name))})");

    if (lines.Count == 0) continue;
    Console.WriteLine($"== {t.FullName} : {t.BaseType?.Name}");
    lines.ForEach(Console.WriteLine);
}
return 0;

static string Pretty(Type t) => t.IsGenericType ? t.Name.Split('`')[0] + "<" + string.Join(",", t.GetGenericArguments().Select(Pretty)) + ">" : t.Name;
