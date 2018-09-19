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
        public void Register<TConcrete>(Lifespan lifespan = Lifespan.Transient) where TConcrete : class
        {
            Register<TConcrete, TConcrete>(lifespan);
        }

        public void Register<TContract, TConcrete>(TConcrete instance) where TConcrete : class, TContract
        {
            instances[typeof(TContract)] = instance;
            Register<TContract, TConcrete>(Lifespan.Singleton);
        }

        public void Register<TConcrete>(TConcrete instance) where TConcrete : class
        {
            instances[typeof(TConcrete)] = instance;
            Register<TConcrete, TConcrete>(Lifespan.Singleton);
        }

        // TODO: Add constructor info
        public void Register<TContract, TConcrete>(Lifespan lifespan = Lifespan.Transient) where TConcrete : class, TContract
        {
            var type = typeof(TConcrete);
            var constructorInfo = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).First();
            var parametersInfo = constructorInfo.GetParameters();
            var parameterRegistrations = parametersInfo.GetParameterRegistrations();

            Func<object[], object> activator;

            switch (instantiationMethod)
            {
                case InstantiationMethod.Activator:
                    activator = (object[] args) => Activator.CreateInstance(type, args);
                    break;
                case InstantiationMethod.Invoke:
                    activator = (object[] args) => constructorInfo.Invoke(args);
                    break;
                case InstantiationMethod.Expression:
                default:
                    activator = GetClassActivatorViaExpressions(constructorInfo, parametersInfo);
                    break;
            }

            registrations[typeof(TContract)] = new Registration {
                Type = type,
                Lifespan = lifespan,
                ParameterCount = parameterRegistrations.Count(),
                Parameters = parameterRegistrations,
                Activator = activator
            };
        }

        private static Func<object[], object> GetClassActivatorViaExpressions(ConstructorInfo constructorInfo, ParameterInfo[] parametersInfo)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "args");

            var argumentExpressions = parametersInfo
                .Select((parameterInfo, i) =>
                {
                    return (Expression)Expression.Convert(
                        Expression.ArrayIndex(parameterExpression, Expression.Constant(i)),
                        parameterInfo.ParameterType
                    );
                });

            NewExpression newExpression = Expression.New(constructorInfo, argumentExpressions);
            LambdaExpression lambda = Expression.Lambda(newExpression, parameterExpression);

            return (Func<object[], object>)lambda.Compile();
        }
    }
}
