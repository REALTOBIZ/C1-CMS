﻿using System;
using System.Web;
using System.Web.UI;

namespace Composite.Ajax
{
    internal class AjaxResponseHttpModule : IHttpModule
	{
        public void Init(HttpApplication context)
        {
            context.PostMapRequestHandler += AttachFilter;
        }

        private static void AttachFilter(object sender, EventArgs e)
	    {
            var httpContext = HttpContext.Current;

            if (httpContext.Handler != null 
                && httpContext.Handler is Page
                && httpContext.Request.RequestType == "GET")
            {
                var response = httpContext.Response;
                response.Filter = new AjaxStream(response.Filter);
            }
	    }

	    public void Dispose()
        {
        }
    }
}
