# Iciclecreek.Json.Net.DependencyInjection
This library implements a JSON.NET converter which will use IServiceProvider to perform dependency injection as needed while deserializing.

# Add NUGET package
To add to your project add
```dotnet add package Iciclecreek.Json.Net.DependencyInjection```

# Description
There are 2 classes in this library
* **ServiceProviderConverter** - a universal converter which will use the IServiceProvider to instantiate objects that do not have a parameterless converter
* **ServiceProviderConvert<T>** - A typed converter which will use IServiceProvider only for the given type T.

# Usage
To use these converters you add them to the converters collection on **JsonSerializerSettings** object.

Example for universal types needing dependency injection:
```C#
IServiceProvider serviceProvider = new ServiceCollection()
    .AddSingleton<JsonSerializerSettings>((sp) => new JsonSerializerSettings() 
    { 
        Converters = new List<JsonConverter>() 
        { 
            new ServiceProviderConverter(sp) 
        } 
    })
    // .., add other dependencies as needed ...
    .BuildServiceProvider();
```

Example for explicit resgistration just for types that need dependency injection:
```c#
IServiceProvider serviceProvider = new ServiceCollection()
    .AddSingleton<JsonSerializerSettings>((sp) => new JsonSerializerSettings() 
    { 
        Converters = new List<JsonConverter>() 
        { 
            new ServiceProviderConverter<Obj1>(sp),
            new ServiceProviderConverter<Obj2>(sp),
            new ServiceProviderConverter<Obj3>(sp) 
        } 
    })
    // .., add other dependencies as needed ...
    .BuildServiceProvider();
```

> NOTE: Registering for objects explicitely is slighty faster on first deserialization because the universal converter has to detect when an object can't be created.

Then simply use your json serializer settings when deserializing:
```c#
var result = JsonConvert.DeserializeObject<MyClass>(json, serviceProvider.GetRequiredService<JsonSerializerSettings>());
```

