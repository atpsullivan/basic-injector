using System;
using System.Collections.Generic;
using System.Text;

namespace BasicInjector
{
    public class ParameterRegistration
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public int Position { get; set; }
    }
}
