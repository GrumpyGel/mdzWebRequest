<%@ Page language="C#" Debug="true" ValidateRequest="false"%>
<%@ Import Namespace="System"%>
<%@ Import Namespace="System.Data"%>
<%@ Import Namespace="System.IO"%>
<%@ Import Namespace="System.Text"%>
<%@ Import Namespace="System.Net"%>
<%@ Import Namespace="System.Xml"%>
<%@ Import Namespace="System.Xml.Xsl"%>
<%@ Import Namespace="System.Web"%>


<script language="C#" runat="server">

//const	string			i_FixedProxyURL			= "";
	const	string			i_FixedProxyURL			= "http://mdzwr.dev.mydocz.com/mdzWebRequestProxy.php";

	void Page_Load(object sender, System.EventArgs e)
	{
		mdzWebRequest		i_Request						= new mdzWebRequest();
		XmlDocument			i_Xml								= new XmlDocument();
		XmlElement			i_Root							= null;
		XslTransform		i_Xslt							= new XslTransform();
		StringWriter		i_XsltOutput				= new StringWriter();
		string					i_Action						= "";
		string					i_ProcessAs					= "auto";
		byte[]					i_ResponseBytes			= null;
		string					i_ResponseBase64		= "";
		string					i_Html							= "";

		try {
			i_Root												= i_Xml.CreateElement("root");
			i_Xml.AppendChild(i_Root);
			i_Action											= mdzSys.i.GetString("Action");

			if (i_Action == "Submit") {
				i_Request.URL								= mdzSys.i.GetString("URL");
				i_Request.Method						= mdzSys.i.GetString("Method");
				if (i_Request.Method == mdzWebRequest.mdzwr_Method_Post || i_Request.Method == mdzWebRequest.mdzwr_Method_Put) {
					i_Request.Content					= mdzSys.i.GetString("Content");
					i_Request.ContentType			= mdzSys.i.GetString("ContentType"); }
				i_Request.UserName					= mdzSys.i.GetString("UserName");
				i_Request.Password					= mdzSys.i.GetString("Password");
				i_Request.ExpectedFormat		= mdzSys.i.GetString("ExpectedFormat");
				i_Request.MaxBinarySize			= mdzSys.i.GetInt("MaxBinarySize");
				if (i_FixedProxyURL == "") {
					i_Request.UseProxy				= mdzSys.i.GetBool("UseProxy");
					if (i_Request.UseProxy == true) {
						i_Request.ProxyURL			= mdzSys.i.GetString("ProxyURL");
						i_Request.ProxyUserName	= mdzSys.i.GetString("ProxyUserName");
						i_Request.ProxyPassword	= mdzSys.i.GetString("ProxyPassword"); } }
				else {
					i_Request.UseProxy				= true;
					i_Request.ProxyURL				= i_FixedProxyURL; }
				i_Request.Submit();
				if (i_Request.IsBinary == true) {
					i_ResponseBase64					=  Convert.ToBase64String(i_Request.ResponseBinary); }
				else {
					i_ResponseBytes						= System.Text.Encoding.UTF8.GetBytes(i_Request.Response);
					i_ResponseBase64					=  Convert.ToBase64String(i_ResponseBytes); }
				i_ProcessAs									= mdzSys.i.GetString("ProcessAs"); }

			mdzSys.i.XmlAddElement(i_Root, "Action",					i_Action);
			mdzSys.i.XmlAddElement(i_Root, "ResponseBase64",	i_ResponseBase64);
			mdzSys.i.XmlAddElement(i_Root, "ProcessAs",				i_ProcessAs);
			mdzSys.i.XmlAddElement(i_Root, "SelectProxy",			((i_FixedProxyURL == "") ? "True" : "False"));
			i_Request.ToXml(i_Root);
			i_Xslt.Load(System.Web.HttpContext.Current.Server.MapPath("mdzWebRequest_Test.xslt"));
			i_Xslt.Transform(i_Xml, null, i_XsltOutput);
			System.Web.HttpContext.Current.Response.Write("<!DOCTYPE Root PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" + i_XsltOutput.ToString()); }
		catch(Exception ex) { Response.Write("Error : " + ex.Message); }
	}

</script>

