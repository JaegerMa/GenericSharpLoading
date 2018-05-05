# GenericSharpLoading

GenericSharpLoading is a .NET Standard 2.0 compatible library to dynamically load external .NET libraries into the runtime.

## Usage
### Basic
If we just want to grab all classes from an assembly, we can just load the assembly and call `.GetInstances`
```csharp
var loader = new SharpLoader();
loader.LoadAssembly(@"C:\Foo\Bar\Apple.dll");

IEnumerable<IFruit> fruits = loader.GetInstances<IFruit>(/* constructor args */);
```

### Continuously load new types
If we instead want to be able to load assemblies from a folder and update our runtime if new assemblies were added, a new loader can be spawned which is connected to our "root" loader and caches which types were already initialized
```csharp

var loader = new SharpLoader();
ConfiguredSharpLoader<IFruit> fruitLoader = loader.ForType<IFruit>();

List<IFruit> fruits = new List<IFruit>();


void LoadNewFruits()
{
	//.LoadAssemblies loads all .exe and .dll files in the
	//given folder, but each file is only loaded once
	loader.LoadAssemblies(@"C:\Foo\Bar\Fruits\");

	//.GetNewInstances will initialize each type only once whilst
	//.GetInstances will create new instances every time it's called
	var newFruits = fruitLoader.GetNewInstances(/* constructor args */);
	//var allFruits = fruitLoader.GetInstances(/* constructor args */);

	fruits.AddRange(newFruits);
}
```

### Instantiate types from already loaded assemblies
If we already have plugins loaded in the runtime, because they are included in the the main assembly which is currently executed, we obviously cannot load our own assembly using `.LoadAssembly("Path/To/Our/Assembly.exe")`.
For that case, you can pass a reference to an Assembly already known to the runtime to `.LoadAssembly` and the SharpLoader adds it to its assembly cache
```csharp
class FruitManager
{
	void LoadFruits()
	{
		var loader = new SharpLoader();
		loader.LoadAssembly(typeof(FruitManager).Assembly);

		IEnumerable<IFruit> fruits = loader.GetInstances<IFruit>(/* constructor args */);
	}
}
```

## Logging
GenericSharpLoading provides a logging interface so the main application can log actions done by this library.

It is as simple as the static class `GenericSharpLoading` provides the event `OnLog` which is fired every time the library would log something.

Along with the message a log level is given, which is just a string containing the word `DEBUG`, `INFO`, `WARNING` or `ERROR`. These strings will probably always be the same, but if you want to process it, I would recommend to use the class `GenericSharpLoading.LogLevel` (nested class) and its four constants `DEBUG`, `INFO`, `WARNING` or `ERROR` which names are guaranteed to be never changed.

### Example
```csharp
class FruitManager
{
	void Init()
	{
		GenericSharpLoading.GenericSharpLoading.OnLog += this.Log;

		LoadFruits();
	}

	void LoadFruits()
	{
		var loader = new PluginLoader();
		//...
	}

	void Log(string message, string logLevel)
	{
		Console.WriteLine($"[FruitManager][{logLevel}] {message}");
	}
}

```

## License
GenericSharpLoading is licensed under the MIT License
