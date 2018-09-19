using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BasicInjector
{
    public static class Extensions
    {
        public static ParameterRegistration[] GetParameterRegistrations(this ParameterInfo[] parameters)
        {
            return parameters.Select(parameter => 
                new ParameterRegistration() {
                    Name = parameter.Name,
                    Type = parameter.ParameterType,
                    Position = parameter.Position
                }).ToArray();
        }
    }
}
