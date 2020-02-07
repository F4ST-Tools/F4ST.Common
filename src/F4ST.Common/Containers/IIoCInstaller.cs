﻿using System;
using Castle.Windsor;
using F4ST.Common.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace F4ST.Common.Containers
{
    public interface IIoCInstaller
    {
        void Install(WindsorContainer container, IMapper mapper);
    }
}