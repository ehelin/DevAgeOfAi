using Shared.Interfaces;
using System.Text;

namespace BLL.Ai.Agents
{
    public class BaseAgent : IAgent
    {
        protected System.Threading.Timer _timer;
        protected bool isRunning = false;
        protected IDataFileService dataFileService;
        protected StringBuilder sb;

        public virtual void Start()
        {
            throw new NotImplementedException();
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }

        public virtual void AgentLoop()
        {
            throw new NotImplementedException();
        }

        protected bool ShouldContinueRunning()
        {
            if (isRunning)
            {
                isRunning = false;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
