using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Nop.Core.Infrastructure;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public class WebConfigMangerHelper : IWebConfigMangerHelper
{
	private readonly INopFileProvider _fileProvider;

	public WebConfigMangerHelper(INopFileProvider fileProvider)
	{
		_fileProvider = fileProvider;
	}

	public void AddMaxJsonSetting()
	{
		bool hasChanged = false;
		XDocument xDocument;
		using (FileStream stream = File.OpenRead(_fileProvider.MapPath("~/Web.config")))
		{
			xDocument = XDocument.Load((Stream)stream);
		}
		if (xDocument == null)
		{
			return;
		}
		xDocument.Changed += delegate
		{
			hasChanged = true;
		};
		XElement xElement = xDocument.XPathSelectElement("configuration//appSettings");
		XElement xElement2 = xDocument.XPathSelectElement("configuration//appSettings//add[@key='aspnet:MaxJsonDeserializerMembers']");
		if (xElement != null && xElement2 == null)
		{
			XElement xElement3 = new XElement("add");
			xElement3.SetAttributeValue("key", "aspnet:MaxJsonDeserializerMembers");
			xElement3.SetAttributeValue("value", "150000");
			xElement.Add(xElement3);
		}
		if (hasChanged)
		{
			try
			{
				xDocument.Save(_fileProvider.MapPath("~/Web.config"));
			}
			catch
			{
			}
		}
	}
}
