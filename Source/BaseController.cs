using PAU.Core.Filters;
using PAU.Core.Models.Context.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.WebApi;

namespace PAU.Core.Controllers.Api
{
    [RequestFilter]
    public class BaseController : UmbracoApiController
    {
        public IContext Context { get; set; }

        public BaseController(IContext context)
        {
            Context = context;
        }
    }
}