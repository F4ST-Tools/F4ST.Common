using System;
using Castle.Windsor;
using F4St.Common.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace F4St.Common.Containers
{
    public interface IIoCInstaller
    {
        void Install(WindsorContainer container, IMapper mapper);
    }
}