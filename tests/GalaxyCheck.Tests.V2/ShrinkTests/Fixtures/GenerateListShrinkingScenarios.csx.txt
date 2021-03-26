#r "nuget: NebulaCheck,0.0.0-626278508" 
#r "nuget: System.Text.Json,5.0.1" 

using System;
using System.IO;
using System.Text.Json;
using System.Runtime.CompilerServices;
using NebulaCheck;

public static string GetScriptFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

var gen =
    from minLength in Gen.Int32().Between(0, 3).WithBias(Gen.Bias.None)
    from list in Gen.Int32().Between(-100, 100).ListOf().BetweenLengths(minLength, minLength + 3).WithLengthBias(Gen.Bias.None)
    select new { List = list, MinLength = minLength };

var sample = gen.Sample(seed: 0);

var json = JsonSerializer.Serialize(sample, new JsonSerializerOptions { WriteIndented = true });

File.WriteAllText(Path.Combine(GetScriptFolder(), "list-shrinking-scenarios.json"), json);