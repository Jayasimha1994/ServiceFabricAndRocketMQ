﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiService
{
    using Owin;

    public interface IOwinAppBuilder
    {
        void Configuration(IAppBuilder appBuilder);
    }
}
