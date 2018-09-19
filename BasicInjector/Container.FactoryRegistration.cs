using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BasicInjector
{
    public partial class Container
    {
        public void RegisterFactory<TContract>(Delegate factoryDelegate, Lifespan lifespan = Lifespan.Transient)
        {
            SetFactoryRegistration<TContract>(factoryDelegate, lifespan);
        }

        public void RegisterFactory<TContract>(Expression<Func<TContract>> factoryExpression, Lifespan lifespan = Lifespan.Transient)
        {
            SetFactoryRegistration<TContract>(factoryExpression, lifespan);
        }

        public void RegisterFactory<TContract, Dep1>(Expression<Func<Dep1, TContract>> factoryExpression, Lifespan lifespan = Lifespan.Transient)
        {
            SetFactoryRegistration<TContract>(factoryExpression, lifespan);
        }

        public void RegisterFactory<TContract, Dep1, Dep2>(Expression<Func<Dep1, Dep2, TContract>> factoryExpression, Lifespan lifespan = Lifespan.Transient)
        {
            SetFactoryRegistration<TContract>(factoryExpression, lifespan);
        }

        public void RegisterFactory<TContract, Dep1, Dep2, Dep3>(Expression<Func<Dep1, Dep2, Dep3, TContract>> factoryExpression, Lifespan lifespan = Lifespan.Transient)
        {
            SetFactoryRegistration<TContract>(factoryExpression, lifespan);
        }

        public void RegisterFactory<TContract, Dep1, Dep2, Dep3, Dep4>(Expression<Func<Dep1, Dep2, Dep3, Dep4, TContract>> factoryExpression, Lifespan lifespan = Lifespan.Transient)
        {
            SetFactoryRegistration<TContract>(factoryExpression, lifespan);
        }

        public void RegisterFactory<TContract, Dep1, Dep2, Dep3, Dep4, Dep5>(Expression<Func<Dep1, Dep2, Dep3, Dep4, Dep5, TContract>> factoryExpression, Lifespan lifespan = Lifespan.Transient)
        {
            SetFactoryRegistration<TContract>(factoryExpression, lifespan);
        }

        private void SetFactoryRegistration<TContract>(
            Delegate factoryDelegate, 
            Lifespan lifespan
        ) {
            var parametersInfo = factoryDelegate.GetMethodInfo().GetParameters();
            var parameterRegistrations = parametersInfo.GetParameterRegistrations();

            registrations[typeof(TContract)] = new Registration
            {
                Type = null,
                Lifespan = lifespan,
                ParameterCount = parameterRegistrations.Count(),
                Parameters = parameterRegistrations,
                Activator = GetFactoryActivatorViaExpressions(factoryDelegate)
            };
        }

        //// ERROR: MethodInfo must be a runtime MethodInfo object.'
        //private void SetFactoryRegistration<TContract>(
        //    LambdaExpression factoryExpression,
        //    Lifespan lifespan
        //) {
        //    var factory = factoryExpression.Compile();
        //    SetFactoryRegistration<TContract>(factory, lifespan);
        //}

        private void SetFactoryRegistration<TContract>(
            LambdaExpression factoryExpression,
            Lifespan lifespan
        ) {
            var factory = factoryExpression.Compile();
            var parametersInfo = factory.GetMethodInfo().GetParameters().Skip(1).ToArray(); // Skip the runtime closure type...
            var parameterRegistrations = parametersInfo.GetParameterRegistrations();

            registrations[typeof(TContract)] = new Registration
            {
                Type = null,
                Lifespan = lifespan,
                ParameterCount = parameterRegistrations.Count(),
                Parameters = parameterRegistrations,
                Activator = GetFactoryActivatorViaExpressions(factoryExpression)
            };
        }

        private static Func<object[], object> GetFactoryActivatorViaExpressions(LambdaExpression factoryExpression)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "args");

            var argumentExpressions = factoryExpression.Parameters
                .Select((factoryParameter, i) =>
                {
                    return (Expression)Expression.Convert(
                        Expression.ArrayIndex(parameterExpression, Expression.Constant(i)),
                        factoryParameter.Type
                    );
                });

            var invocationExpression = Expression.Invoke(factoryExpression, argumentExpressions);
            LambdaExpression lambda = Expression.Lambda(invocationExpression, parameterExpression);

            return (Func<object[], object>)lambda.Compile();
        }

        private static Func<object[], object> GetFactoryActivatorViaExpressions(Delegate factory)
        {
            var factoryInfo = factory.GetMethodInfo();
            var factoryParameters = factoryInfo.GetParameters();

            ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "args");

            var argumentExpressions = factoryParameters
                .Select((factoryParameter, i) =>
                {
                    return (Expression)Expression.Convert(
                        Expression.ArrayIndex(parameterExpression, Expression.Constant(i)),
                        factoryParameter.ParameterType
                    );
                });

            var invocationExpression = Expression.Call(factoryInfo, argumentExpressions);
            LambdaExpression lambda = Expression.Lambda(invocationExpression, parameterExpression);

            return (Func<object[], object>)lambda.Compile();
        }
    }
}
