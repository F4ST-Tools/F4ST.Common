using Castle.Windsor;
using F4ST.Common.Containers;
using F4ST.Common.Mappers;

namespace F4ST.Common.Tools
{
    public class CommonInstaller : IIoCInstaller
    {
        public int Priority => -99;

        public void Install(WindsorContainer container, IMapper mapper)
        {
            
        }
    }
}