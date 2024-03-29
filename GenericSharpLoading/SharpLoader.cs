﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static GenericSharpLoading.GenericSharpLoading;

namespace GenericSharpLoading
{
	public class SharpLoader
	{
		public virtual Dictionary<string, Assembly> LoadedAssembliesDic { get; set; } = new Dictionary<string, Assembly>();

		public virtual IEnumerable<Assembly> LoadedAssemblies => this.LoadedAssembliesDic.Values;

		public virtual void LoadAssemblies(string directory)
		{
			if(!Directory.Exists(directory))
				return;

			Log($"Loading assemblies from directory {directory}", LogLevel.DEBUG);

			foreach(string file in Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories))
				this.LoadAssembly(file);
			foreach(string file in Directory.GetFiles(directory, "*.exe", SearchOption.AllDirectories))
				this.LoadAssembly(file);
		}
		public virtual void LoadAssembly(Assembly assembly)
		{
			if(assembly == null)
				return;

			var name = assembly.FullName;
			Log($"Loading internal assembly {name}", LogLevel.DEBUG);

			if(this.LoadedAssembliesDic.ContainsKey(name))
			{
				Log($"Loading internal assembly {name} already in pool", LogLevel.DEBUG);
				return;
			}

			this.LoadedAssembliesDic.Add(name, assembly);
			Log($"Internal assembly {name} loaded", LogLevel.DEBUG);
		}
		public virtual void LoadAssembly(string assemblyPath)
		{
			try
			{
				if(this.LoadedAssembliesDic.ContainsKey(assemblyPath))
					return;

				Log($"Loading external assembly '{assemblyPath}'");
				this.LoadedAssembliesDic.Add(assemblyPath, Assembly.LoadFrom(assemblyPath));
			}
			catch(Exception x)
			{
				Log($"Error while loading assembly: {x.ToString()}", LogLevel.ERROR);
			}
		}


		public virtual IEnumerable<PluginType> GetInstances<PluginType>(params object[] constructorArgs)
		{
			return this.GetInstances<PluginType>(constructorArgs, SharpLoader.ToTypes(constructorArgs));
		}
		public virtual IEnumerable<PluginType> GetInstances<PluginType>(object[] constructorArgs, Type[] constructorArgTypes)
		{
			var baseType = typeof(PluginType);

			return this.GetInstances(baseType, constructorArgs, constructorArgTypes)
					.Cast<PluginType>();
		}
		public virtual IEnumerable<object> GetInstances(Type type, params object[] constructorArgs)
		{
			return this.GetInstances(type, constructorArgs, SharpLoader.ToTypes(constructorArgs));
		}
		public virtual IEnumerable<object> GetInstances(Type baseType, object[] constructorArgs, Type[] constructorArgTypes)
		{
			Log($"Creating instances for plugins with type '{baseType?.FullName}'", LogLevel.DEBUG);

			return SharpLoader.GetInstances(this.GetTypes(baseType), constructorArgs, constructorArgTypes);
		}

	
		public virtual ConfiguredSharpLoader ForType(Type baseType)
		{
			return new ConfiguredSharpLoader(this, baseType);
		}
		public virtual ConfiguredSharpLoader<BaseType> ForType<BaseType>()
		{
			return new ConfiguredSharpLoader<BaseType>(this);
		}

		public virtual IEnumerable<Type> GetTypes(Type pluginType)
		{
			Log($"Searching plugins with type '{pluginType?.FullName}'", LogLevel.DEBUG);

			return this.LoadedAssemblies
						.SelectMany((assembly) => GetTypesOfAssembly(assembly, pluginType))
						.ToList();
		}



		public static object GetInstance(Type type, params object[] constructorArgs)
		{
			return SharpLoader.GetInstance(type, constructorArgs, ToTypes(constructorArgs));
		}
		public static object GetInstance(Type type, object[] constructorArgs, Type[] constructorArgTypes)
		{
			Log($"Creating instance for type '{type?.FullName}'", LogLevel.DEBUG);

			ConstructorInfo constructor = type.GetConstructor(constructorArgTypes);
			if(constructor == null)
			{
				Log($"Type {type?.FullName} doesn't have an appropriate constructor", LogLevel.DEBUG);
				return null;
			}

			object instance;
			try
			{
				instance = constructor.Invoke(constructorArgs);
			}
			catch(Exception x)
			{
				Log($"Error while creating instance of type '{type?.FullName}': ${x.ToString()}", LogLevel.ERROR);
				return null;
			}

			return instance;
		}
		public static IEnumerable<object> GetInstances(IEnumerable<Type> types, object[] constructorArgs, Type[] constructorArgTypes)
		{
			Log($"Trying to create instances for following types: '{string.Join("', '", types.Select((type) => type?.FullName))}'");

			//Resolve enumerable to avoid creating instances multiple times
			return types
					.Select((type) => GetInstance(type, constructorArgs, constructorArgTypes))
					.Where((instance) => instance != null)
					.ToList();
		}
		public static IEnumerable<Type> GetTypesOfAssembly(Assembly assembly, Type baseType)
		{
			foreach(Type type in assembly.GetTypes())
			{
				if(type.IsAbstract || type.IsInterface)
					continue;

				if(baseType.IsAssignableFrom(type))
				{
					Log($"Found class of type '{type?.FullName}' in assembly {assembly?.FullName}", LogLevel.DEBUG);
					yield return type;
				}
			}
		}
		
		public static Type[] ToTypes(object[] constructorArgs)
		{
			return constructorArgs.Select((obj) => obj?.GetType()).ToArray();
		}
	}
}
