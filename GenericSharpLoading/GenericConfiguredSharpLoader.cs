using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericSharpLoading
{
	public class ConfiguredSharpLoader<BaseType> : ConfiguredSharpLoader
	{
		public ConfiguredSharpLoader(SharpLoader parent) : base(parent, typeof(BaseType))
		{ }

		public new IEnumerable<BaseType> GetInstances(params object[] constructorArgs)
		{
			return base.GetInstances(constructorArgs)
						.Cast<BaseType>();
		}
		public new IEnumerable<BaseType> GetInstances(object[] constructorArgs, Type[] constructorArgTypes, bool markTypesAsInstantiated = true, bool onlyNewTypes = false)
		{
			return base.GetInstances(constructorArgs, constructorArgTypes, markTypesAsInstantiated: markTypesAsInstantiated, onlyNewTypes: onlyNewTypes)
						.Cast<BaseType>();
		}
		public new IEnumerable<BaseType> GetNewInstances(params object[] constructorArgs)
		{
			return base.GetNewInstances(constructorArgs)
						.Cast<BaseType>();
		}
		public new IEnumerable<BaseType> GetNewInstances(object[] constructorArgs, Type[] constructorArgTypes, bool markTypesAsInstantiated = true)
		{
			return base.GetNewInstances(constructorArgs, constructorArgTypes, markTypesAsInstantiated: markTypesAsInstantiated)
						.Cast<BaseType>();
		}
	}
}
