using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BasicInjector
{
    public partial class Container
    {
        private readonly IDictionary<Type, Registration> registrations = new Dictionary<Type, Registration>();
        private readonly IDictionary<Type, object> instances = new Dictionary<Type, object>();

        private InstantiationMethod instantiationMethod;

        public Container() : this(InstantiationMethod.Expression) { }

        public Container(InstantiationMethod instantiationMethod)
        {
            this.instantiationMethod = instantiationMethod;
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type requestedType)
        {
            if (registrations.TryGetValue(requestedType, out Registration registration))
            {
                if(registration.Lifespan == Lifespan.Singleton)
                {
                    if(!instances.TryGetValue(requestedType, out object instance))
                    {
                        instance = CreateInstance(registration);
                        instances[requestedType] = instance;
                    }
                    return instance;
                }
                else
                {
                    return CreateInstance(registration);
                }
            }
            else
            {
                throw new InvalidOperationException("A dependency of type " + requestedType.Name + " has not been registered.");
            }
        }

        private object CreateInstance(Registration registration)
        {
            //var parameterInstances = registration.Parameters.Select(parameter => Resolve(parameter.Type)).ToArray();
            var parameterInstances = new Object[registration.ParameterCount];
            for (int i = 0; i < registration.ParameterCount; i++)
            {
                parameterInstances[i] = Resolve(registration.Parameters[i].Type);
            }
            return registration.Activator(parameterInstances);
        }

        // TODO: Check for circular dependencies
        public void Verify()
        {
            var errors = new List<string>();
            foreach (var pair in registrations)
            {
                try
                {
                    Resolve(pair.Key);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            if(errors.Count() > 0)
                throw new InvalidOperationException(string.Join(". ", errors));
        }

        private class Registration
        {
            public Type Type { get; set; }
            public Lifespan Lifespan { get; set; }
            public ParameterRegistration[] Parameters { get; set; }
            public int ParameterCount { get; set; }
            public Func<object[], object> Activator { get; set; }
        }
    }
}
