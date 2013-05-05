using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ApiDashboard.Service;
using Microsoft.AspNet.SignalR;

namespace ApiDashboard.Web
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			//init signalR
			RouteTable.Routes.MapHubs();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			//start reading rabbitmq api events
			ApiEventReader.Init();
			ApiEventReader.RegisterActionToPerformOnRead(HandleApiRequest);
		}

		private void HandleApiRequest(ApiWebRequestCompletedEvent apiEvent)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<ApiRequestHub>();
			context.Clients.All.send(apiEvent.PublisherId, apiEvent.Route);
		}
	}

	public class ApiRequestHub : Hub {}
}