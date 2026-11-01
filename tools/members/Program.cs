using System.Reflection;

// Lists a game type's properties and methods with their types, read from the interop assemblies
// without loading them. Use scripts/members.sh, which supplies the interop directory.
if (args.Length != 3)
{
    Console.Error.WriteLine("usage: members <interopDir> <type|*> <memberRegex>");
    return 1;
}

var dir = args[0];
var paths = Directory.GetFiles(dir, "*.dll")
    .Concat(Directory.GetFiles(Path.Combine(dir, "..", "core"), "*.dll"))
    .Concat(Directory.GetFiles(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "*.dll"))
    .ToList();
var mlc = new MetadataLoadContext(new PathAssemblyResolver(paths), typeof(object).Assembly.GetName().Name);
var asm = mlc.LoadFromAssemblyPath(Path.Combine(dir, "Assembly-CSharp.dll"));
var re = new System.Text.RegularExpressions.Regex(args[2]);
var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

foreach (var t in asm.GetTypes().Where(t => args[1] == "*" || t.Name == args[1]))
{
    var lines = new List<string>();
    if (t.IsEnum && (args[1] == t.Name || re.IsMatch(t.Name)))
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
