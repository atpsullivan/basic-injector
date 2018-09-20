using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BasicInjector;
using SimpleInjector;

namespace ContainerTest
{
    static class Program
    {
        static Program() { }

        static void Main(string[] args)
        {
            FeatureTest();
            Console.WriteLine();

            PerformanceTest();
            Console.WriteLine();
            PerformanceTest();
            Console.WriteLine();
            PerformanceTest();
            Console.WriteLine();
            PerformanceTest();
            Console.WriteLine();

            InstanceCreationTest();
            Console.WriteLine();
            InstanceCreationTest();
            Console.WriteLine();
            InstanceCreationTest();
            Console.WriteLine();
            InstanceCreationTest();
            Console.WriteLine();

            Console.ReadLine();
        }

        static void FeatureTest()
        {
            var container = new BasicInjector.Container();
            FeatureBootstrapper(container);

            var processInstance = container.Resolve<IProcess>();
            processInstance.Execute();
        }

        static void InstanceCreationTest(int instancesToCreate = 1000000)
        {
            var config = new Config() { AppName = "Container Test" };
            var logger = new ConsoleLogger(config);
            var repo = new TestRepo();
            var type = typeof(TestProcess);
            var constructorInfo = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).First();
            var parametersInfo = constructorInfo.GetParameters();

            var activator = GetClassActivatorViaExpressions(constructorInfo, parametersInfo);
            var args = new object[] { logger, repo };

            var watch1 = Stopwatch.StartNew();
            for (int i = 0; i < instancesToCreate; i++)
            {
                activator(args);
            }
            watch1.Stop();
            Console.WriteLine("{0}: {1}ms", "Expression Create", watch1.ElapsedMilliseconds);

            //var watch2 = Stopwatch.StartNew();
            //for (int i = 0; i < instancesToCreate; i++)
            //{
            //    constructorInfo.Invoke(args);
            //}
            //watch2.Stop();
            //Console.WriteLine("{0}: {1}ms", "Invoke", watch2.ElapsedMilliseconds);

            //var watch3 = Stopwatch.StartNew();
            //for (int i = 0; i < instancesToCreate; i++)
            //{
            //    Activator.CreateInstance(type, args);
            //}
            //watch3.Stop();
            //Console.WriteLine("{0}: {1}ms", "Activator", watch3.ElapsedMilliseconds);

            var watch4 = Stopwatch.StartNew();
            Func<object[], object> func = (object[] a) => new TestProcess((ConsoleLogger)a[0], (TestRepo)a[1]);
            for (int i = 0; i < instancesToCreate; i++)
            {
                func(args);
            }
            watch4.Stop();
            Console.WriteLine("{0}: {1}ms", "New", watch4.ElapsedMilliseconds);
        }

        static void PerformanceTest(int instancesToCreate = 10000000)
        {
            //****************
            // Basic Injector
            //****************
            var expressionContainer = new BasicInjector.Container(InstantiationMethod.Expression);
            BasicPerformanceBootstrapper(expressionContainer);
            TestContainer("Expression", () => expressionContainer.Resolve<IProcess>(), instancesToCreate);

            //var activatorContainer = new BasicInjector.Container(InstantiationMethod.Activator);
            //BasicPerformanceBootstrapper(activatorContainer);
            //TestContainer("Activator", () => activatorContainer.Resolve<IProcess>(), instancesToCreate);

            //var invokeContainer = new BasicInjector.Container(InstantiationMethod.Invoke);
            //BasicPerformanceBootstrapper(invokeContainer);
            //TestContainer("Invoke", () => invokeContainer.Resolve<IProcess>(), instancesToCreate);


            //*****************
            // Simple Injector
            //*****************
            var simpleContainer = new SimpleInjector.Container();
            SimplePerformanceBootstrapper(simpleContainer);
            TestContainer("SimpleInjector", () => simpleContainer.GetInstance<IProcess>(), instancesToCreate);
        }

        static void TestContainer(string description, Func<object> resolver, int instancesToCreate = 1000000)
        {
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < instancesToCreate; i++)
            {
                resolver();
            }
            watch.Stop();
            Console.WriteLine("{0}: {1}ms", description, watch.ElapsedMilliseconds);
        }

        static void FeatureBootstrapper(BasicInjector.Container container)
        {
            container.Register(new Config() { AppName = "Container Test" });
            container.RegisterFactory<ILogger, Config>((config) => new ConsoleLogger(config), Lifespan.Singleton);
            //Func<Config, ILogger> loggerFactory = (config) => new ConsoleLogger(config);
            //container.RegisterFactory<ILogger>(loggerFactory, Lifespan.Singleton);
            container.Register<IRepo<TestEntity>, TestRepo>();
            container.Register<IProcess, TestProcess>();

            container.Verify();
        }

        static void BasicPerformanceBootstrapper(BasicInjector.Container container)
        {
            container.Register(new Config() { AppName = "Container Test" });
            container.Register<ILogger, ConsoleLogger>(Lifespan.Singleton);
            container.Register<IRepo<TestEntity>, TestRepo>();
            container.Register<IProcess, TestProcess>();

            container.Verify();
        }

        static void SimplePerformanceBootstrapper(SimpleInjector.Container container)
        {
            container.RegisterInstance(new Config() { AppName = "Container Test" });
            container.Register<ILogger, ConsoleLogger>(Lifestyle.Singleton);
            container.Register<IRepo<TestEntity>, TestRepo>();
            container.Register<IProcess, TestProcess>();

            container.Verify();
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

        // TODO: Test scope - i.e. pass a func as a factory that has references to a class
    }
}
