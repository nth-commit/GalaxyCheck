#r "nuget: NebulaCheck,0.0.0-626278508" 
#r "nuget: System.Text.Json,5.0.1" 

using System;
using System.IO;
using System.Text.Json;
using System.Runtime.CompilerServices;
using NebulaCheck;

public static string GetScriptFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

var gen =
    from min in Gen.Int32().Between(-10, 10).WithBias(Gen.Bias.None)
    from origin in Gen.Int32().Between(min, min + 10).WithBias(Gen.Bias.None)
    from max in Gen.Int32().Between(origin, origin + 10).WithBias(Gen.Bias.None)
    from value in Gen.Int32().Between(min, max).WithBias(Gen.Bias.None)
    select new
    {
        Min = min,
        Max = max,
        Origin = origin,
        Value = value
    };

var sample = gen.Sample(seed: 0, iterations: 25);

var json = JsonSerializer.Serialize(sample, new JsonSerializerOptions { WriteIndented = true });

File.WriteAllText(Path.Combine(GetScriptFolder(), "int32-example-space-scenarios.json"), json);