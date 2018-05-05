using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static GenericSharpLoading.GenericSharpLoading;

namespace GenericSharpLoader
{
	public class ConfiguredSharpLoader : SharpLoader
	{
		protected SharpLoader parent;
		public Type BaseType { get; protected set; }
		public HashSet<Type> InstantiatedTypes { get; set; }

		public SharpLoader Parent
		{
			get => this.parent ?? this;
			set => this.parent = value;
		}


		public override Dictionary<string, Assembly> LoadedAssembliesDic
		{
			get => this.Parent.LoadedAssembliesDic;
			set => this.Parent.LoadedAssembliesDic = value;
		}
		public override IEnumerable<Assembly> LoadedAssemblies => this.LoadedAssembliesDic.Values;

		public ConfiguredSharpLoader(SharpLoader parent, Type baseType)
		{
			this.parent = parent ?? new SharpLoader();
			this.BaseType = baseType;

			this.InstantiatedTypes = new HashSet<Type>();
		}

		public override void LoadAssemblies(string directory) => this.Parent.LoadAssembly(directory);
		public override void LoadAssembly(Assembly assembly) => this.Parent.LoadAssembly(assembly);
		public override void LoadAssembly(string assemblyPath) => this.Parent.LoadAssembly(assemblyPath);


		public virtual IEnumerable<object> GetInstances(params object[] constructorArgs)
		{
			return GetInstances(constructorArgs, ToTypes(constructorArgs));
		}
		public virtual IEnumerable<object> GetInstances(object[] constructorArgs, Type[] constructorArgTypes, bool markTypesAsInstantiated = true, bool onlyNewTypes = false)
		{
			var types = GetTypes(onlyNewTypes);

			Log($"Creating instances for {(onlyNewTypes ? "new " : "")} plugins with type '{this.BaseType?.FullName}'. Types are {(markTypesAsInstantiated ? "" : "not ")} marked as instantiated", LogLevel.DEBUG);

			if(markTypesAsInstantiated)
				this.InstantiatedTypes.UnionWith(types);

			return SharpLoader.GetInstances(types, constructorArgs, constructorArgTypes);
		}
		
		public virtual IEnumerable<object> GetNewInstances(params object[] constructorArgs)
		{
			return GetNewInstances(constructorArgs, ToTypes(constructorArgs));
		}
		public virtual IEnumerable<object> GetNewInstances(object[] constructorArgs, Type[] constructorArgTypes, bool markTypesAsInstantiated = true)
		{
			return GetInstances(constructorArgs, constructorArgTypes, markTypesAsInstantiated: markTypesAsInstantiated, onlyNewTypes: true);
		}

		public override IEnumerable<PluginType> GetInstances<PluginType>(params object[] constructorArgs) => this.Parent.GetInstances<PluginType>(constructorArgs);
		public override IEnumerable<PluginType> GetInstances<PluginType>(object[] constructorArgs, Type[] constructorArgTypes) => this.Parent.GetInstances<PluginType>(constructorArgs, constructorArgTypes);
		public override IEnumerable<object> GetInstances(Type baseType, params object[] constructorArgs) => this.Parent.GetInstances(baseType, constructorArgs);
		public override IEnumerable<object> GetInstances(Type baseType, object[] constructorArgs, Type[] constructorArgTypes) => this.Parent.GetInstances(baseType, constructorArgs, constructorArgTypes);


		public override ConfiguredSharpLoader ForType(Type baseType) => this.Parent.ForType(baseType);
		public override ConfiguredSharpLoader<BaseType> ForType<BaseType>() => this.Parent.ForType<BaseType>();

		public override IEnumerable<Type> GetTypes(Type pluginType) => this.Parent.GetTypes(pluginType);

		public virtual IEnumerable<Type> GetTypes(bool onlyNewTypes = false)
		{
			var types = GetTypes(this.BaseType);

			if(onlyNewTypes)
				types = types.Where((type) => !this.InstantiatedTypes.Contains(type));

			//Resolve enumerable to prevent problems with InstantiatedTypes
			return types.ToList();
		}
		public virtual IEnumerable<Type> GetNewTypes()
		{
			return GetTypes(onlyNewTypes: true);
		}

		public virtual void ClearInstantiatedTypes()
		{
			this.InstantiatedTypes.Clear();
		}
	}
}
