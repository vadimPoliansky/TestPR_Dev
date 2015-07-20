using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using ClosedXML;
using System.Web.Helpers;
using System.Threading;
using IndInv.Models;
using WebMatrix.WebData;

namespace IndInv
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801



    public class MvcApplication : System.Web.HttpApplication
    {
		private static SimpleMembershipInitializer _initializer;
		private static object _initializerLock = new object();
		private static bool _isInitialized;

		protected void Application_Start()
		{
			Database.SetInitializer<Models.InventoryDBContext>(null);

			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			AuthConfig.RegisterAuth();
			AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
			//GlobalConfiguration.Configuration.Filters.Add(new System.Web.Http.AuthorizeAttribute());

			LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
		}

		public class SimpleMembershipInitializer
		{
			public SimpleMembershipInitializer()
			{
				using (var context = new UsersContext())
					context.UserProfiles.Find(1);

				if (!WebSecurity.Initialized)
					WebSecurity.InitializeDatabaseConnection("UsersContext", "UserProfile", "UserId", "UserName", autoCreateTables: true);
			}
		}



/*
        protected void Application_BeginRequest()
        {
            if (!Context.Request.IsSecureConnection)
                Response.Redirect(Context.Request.Url.ToString().Replace
                ("http:", "https:"));
        }
*/
    }
}