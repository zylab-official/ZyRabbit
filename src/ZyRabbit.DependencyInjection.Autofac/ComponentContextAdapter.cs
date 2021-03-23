using System;
using System.Linq;
using Autofac;
using Autofac.Core;

namespace ZyRabbit.DependencyInjection.Autofac
{
	public class ComponentContextAdapter : IDependencyResolver
	{
		public static ComponentContextAdapter Create(IComponentContext context)
		{
			return new ComponentContextAdapter(context);
		}

		private readonly IComponentContext _context;

		public ComponentContextAdapter(IComponentContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public object GetService(Type serviceType, params object[] additional)
		{
			var parameters = additional
				.Select(a => new TypedParameter(a.GetType(), a))
				.ToList<Parameter>();
			return _context.Resolve(serviceType ?? throw new ArgumentNullException(nameof(serviceType)), parameters);
		}
	}
}
