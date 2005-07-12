using System;
using System.Web;

using Cuyahoga.Core.Domain;

namespace Cuyahoga.Web.Util
{
	/// <summary>
	/// The UrlHelper class contains methods for url creation and manipulation.
	/// </summary>
	public class UrlHelper
	{
		private UrlHelper()
		{
		}

		/// <summary>
		/// GetApplicationPath returns the base application path and ensures that it allways ends with a "/".
		/// </summary>
		/// <returns></returns>
		public static string GetApplicationPath()
		{
			string path = HttpContext.Current.Request.ApplicationPath;
			if (path.EndsWith("/"))
			{
				return path;
			}
			else
			{
				return path + "/";
			}
		}

		/// <summary>
		/// Get the (lowercase) url of the site without any trailing slashes.
		/// </summary>
		/// <returns></returns>
		public static string GetSiteUrl()
		{
			string path = HttpContext.Current.Request.ApplicationPath;
			if (path.EndsWith("/") && path.Length == 1)
			{
				return GetHostUrl();
			}
			else
			{
				return GetHostUrl() + path.ToLower();
			}
		}
		/// <summary>
		/// Returns a formatted url for a given node (/{ApplicationPath}/{Node.ShortDescription}.aspx.
		/// </summary>
		/// <param name="nodeId"></param>
		/// <returns></returns>
		public static string GetFriendlyUrlFromNode(Node node)
		{
			return GetApplicationPath() + node.ShortDescription + ".aspx";
		}

		/// <summary>
		/// Returns a formatted url for a given node (/{ApplicationPath}/{Node.Id}/view.aspx
		/// or /{ApplicationPath}/{Node.ShortDescription}.aspx if the site has friendly urls
		/// turned on).
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static string GetUrlFromNode(Node node)
		{
			if (node.Site.UseFriendlyUrls)
			{
				return GetFriendlyUrlFromNode(node);
			}
			else
			{
				return GetApplicationPath() + node.Id.ToString() + "/view.aspx";
			}
		}

		/// <summary>
		/// Returns a formatted url for a given nodeId (/{ApplicationPath}/{nodeId}/view.aspx.
		/// </summary>
		/// <param name="nodeId"></param>
		/// <returns></returns>
		public static string GetUrlFromNodeId(int nodeId)
		{
			return GetApplicationPath() + nodeId.ToString() + "/view.aspx";
		}

		/// <summary>
		/// Returns a formatted url for a given section (/{ApplicationPath}/{Section.Id}/section.aspx).
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		public static string GetUrlFromSection(Section section)
		{
			return GetApplicationPath() + section.Id.ToString() + "/section.aspx";
		}

		/// <summary>
		/// Returns a formatted url for a given section (http://{hostname}/{ApplicationPath}/{Section.Id}/section.aspx).
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		public static string GetFullUrlFromSection(Section section)
		{
			return GetHostUrl() + GetApplicationPath() + section.Id + "/section.aspx";
		}

		/// <summary>
		/// Returns a formatted url for a rss feed for a given section 
		/// (http://{hostname}/{ApplicationPath}/{Section.Id}/rss.aspx).
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		public static string GetRssUrlFromSection(Section section)
		{
			return GetHostUrl() + GetApplicationPath() + section.Id.ToString() + "/feed.aspx";
		}

		public static string[] GetParamsFromPathInfo(string pathInfo)
		{
			if (pathInfo.Length > 0)
			{
				if (pathInfo.EndsWith("/"))
				{
					pathInfo = pathInfo.Substring(0, pathInfo.Length - 1);
				}
                pathInfo = pathInfo.Substring(1, pathInfo.Length -1);
				return pathInfo.Split(new char[] {'/'});
			}
			else
			{
				return null;
			}
		}
		
		private static string GetHostUrl()
		{
			string https = HttpContext.Current.Request.ServerVariables["HTTPS"];
			string protocol = https == "off" ? "http" : "https";
			string serverPort = HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
			string port = serverPort == "80" ? string.Empty : ":" + serverPort;
			string serverName = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
			return string.Format("{0}://{1}{2}" , protocol, serverName, port ); 
		}
	}
}
