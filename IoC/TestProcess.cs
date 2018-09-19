using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContainerTest
{
    public class TestProcess : IProcess
    {
        private ILogger logger;
        private IRepo<TestEntity> repo;

        public TestProcess(ILogger logger, IRepo<TestEntity> repo)
        {
            this.logger = logger;
            this.repo = repo;
        }

        public void Execute()
        {
            var result = repo.GetAll();
            this.logger.Log("Records queried during execution of the test process: " + result.Count().ToString());
            // ...
        }
    }
}
