using CelHost.Database;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;

namespace CelHost.ServicesImpl
{
    public class NodeServiceImpl
    {
        private readonly IUnitOfWork<HostContext> unitOfWork;
        public NodeServiceImpl(IUnitOfWork<HostContext> unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

    }
}
