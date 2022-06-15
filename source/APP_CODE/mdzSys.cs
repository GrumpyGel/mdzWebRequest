using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Web;
using System.Web.Mail;
using System.Net;
using System.Runtime.InteropServices;


/*
mdzSys : Class for helper functions
===================================

The mdzSys class provides helper functions to reduce coding and provide consistency for repetetive tasks.  It
also provides a repository for system wide fixed properties as configuration for the app.

The class is implemented as a Singleton class allowing for construction and destruction code to be implemented
if required for future use.  The singleton is published as the 'i' (instance) property.  All functions should
therefore be envoked as 'mdzSys.i.{FunctionName}({Arguments});'.

Most functions are generic in purpose.  Others are specific to the way MyDocz builds applications.

The following specific functions:

Name                    Function
----------------------  ----------------------------------------------------------------------------
SendEmail               Function used to send emails.
WriteLog                Function for writing to logging files for debug/error tracing purposes.
ErrorLog                Function for logging application errors.
HttpHit                 Function to make a http hit and return the response as a string.
PerformanceCheck        Generic routine to monitor performance of processes.
AddError                Function for adding error details into XML for XSLT response.

The following types of functions are provided:

Name                    Function
----------------------  ----------------------------------------------------------------------------
XML Helpers             Functions to perform common xml related activity.
Post/URL Field Helpers  Functions to simplify extracting posted form / URL fields to correct data type.
DataType Helpers        Common data type conversions and processing.
*/


public class mdzSys
{

/*
Static Singleton instance declaration published as 'i' property.
*/

	private static readonly mdzSys s_Instance = new mdzSys();
	
	public static mdzSys i { get { return s_Instance; } }


/*
Property declarations.

These are read only properties used by MyDocz classes as a central configuration.

Do not modify defaults here, or else the same changes will be needed if the file is updated.
Set changes in the mdzSys.config file.

Name                    Function
----------------------  ----------------------------------------------------------------------------
DateNull                A DateTime value indicates null.
DateDefault             A DateTime value used as a default value where DateTime.Now/Today is not appropriate.
UrlPrivateServer				Used to locate secure information, for example database connection strings.
*/

	private DateTime		c_DateNull							= new DateTime(1, 1, 1, 0, 0, 0);
	private DateTime		c_DateDefault						= new DateTime(2020, 1, 1, 0, 0, 0);
	private string			c_UrlPrivateServer			= "http://private.dev.mydocz.com";

	public DateTime			DateNull							{ get { return c_DateNull;						} }
	public DateTime			DateDefault						{ get { return c_DateDefault;					} }
	public string				UrlPrivateServer			{ get { return c_UrlPrivateServer;		} }


/*
Constants used as Configuration of functions in mdzSys.

Do not modify defaults here, or else the same changes will be needed if the file is updated.
Set changes in the mdzSys.config file.
*/

	private	string			mdzSys_Smtp_Server									= "localhost";
	private	bool				mdzSys_Smtp_Authenticate						= false;
	private	string			mdzSys_Smtp_Authenticate_Port				= "587";
	private	string			mdzSys_Smtp_Authenticate_UserName		= "";
	private	string			mdzSys_Smtp_Authenticate_Password		= "";
	private	bool				mdzSys_Smtp_LogErrors								= false;
	private	bool				mdzSys_Smtp_ThrowErrors							= false;

	private string			mdzSys_WriteLog_File								= "~mdzLog.txt";

	private string			mdzSys_ErrorLog_File								= "~ErrorLog.txt";
	private string			mdzSys_ErrorLog_From								= "admin@mydocz.com";
	private string			mdzSys_ErrorLog_To									= "admin@mydocz.com";
	private string			mdzSys_ErrorLog_CC									= "";
	private	bool				mdzSys_ErrorLog_ThrowErrors					= false;

	private bool				mdzSys_PerformanceCheck_Enable			= true;
	private string			mdzSys_PerformanceCheck_File				= "~mdzPerformance.txt";

	private Object			c_WriteLogLock											= new Object();		// Used to Lock Log stream.

/*
Construction and destruction code
=================================

The initialisation code loads configuration from the mdzSys.config file.
This code is only ran on initialisation, to reload configuration the website must be restarted.
The mdzSys.config file is xml format, it is not named .xml so that the web server does not serve the file.

A sample file is asd follows:

<mdzSys @DateNull="1 1 1 0 0 0"
        @DateDefault="2020 1 1 0 0 0"
        @UrlPrivateServer="http://private.dev.mydocz.com"
  <Smtp @Server="localhost"
        @Authenticate="False"
        @Authenticate_Port="587"
        @Authenticate_UserName=""
        @Authenticate_Password=""
        @LogErrors="False"
        @ThrowErrors="False"/>
  <WriteLog @File="~mdzLog.txt"/>
  <ErrorLog @File="~ErrorLog.txt"
            @From="admin@mydocz.com"
            @To="admin@mydocz.com"
            @CC=""
            @ThrowErrors="False"/>
  <PerformanceCheck @Enable="True"
                    @File="~mdzPerformance.txt"/>
</mdzSys>
*/

	static mdzSys() { }


	private mdzSys()
	{
		XmlDocument		i_Config;
		XmlElement		i_Root;
		XmlElement		i_Section;
		XmlNodeList 	i_Element;

		try {
			i_Config							= new XmlDocument();
			i_Config.Load(System.Web.HttpContext.Current.Server.MapPath("mdzSys.config"));
			i_Root								= i_Config.DocumentElement;
			if (i_Root.HasAttribute("DateNull"))										{ c_DateNull													= ToDate(i_Root.GetAttribute("DateNull")); }
			if (i_Root.HasAttribute("DateDefault"))									{ c_DateDefault												= ToDate(i_Root.GetAttribute("DateDefault")); }
			if (i_Root.HasAttribute("UrlPrivateServer"))						{ c_UrlPrivateServer									= i_Root.GetAttribute("UrlPrivateServer"); }

			i_Element							= i_Root.GetElementsByTagName("Smtp");
			if (i_Element.Count == 1) {
				i_Section						= (XmlElement) i_Element[0];
				if (i_Section.HasAttribute("Server"))									{ mdzSys_Smtp_Server									= i_Section.GetAttribute("Server"); }
				if (i_Section.HasAttribute("Authenticate"))						{ mdzSys_Smtp_Authenticate						= ToBool(i_Section.GetAttribute("Authenticate")); }
				if (i_Section.HasAttribute("Authenticate_Port"))			{ mdzSys_Smtp_Authenticate_Port				= i_Section.GetAttribute("Authenticate_Port"); }
				if (i_Section.HasAttribute("Authenticate_UserName"))	{ mdzSys_Smtp_Authenticate_UserName		= i_Section.GetAttribute("Authenticate_UserName"); }
				if (i_Section.HasAttribute("Authenticate_Password"))	{ mdzSys_Smtp_Authenticate_Password		= i_Section.GetAttribute("Authenticate_Password"); }
				if (i_Section.HasAttribute("LogErrors"))							{ mdzSys_Smtp_LogErrors								= ToBool(i_Section.GetAttribute("LogErrors")); }
				if (i_Section.HasAttribute("ThrowErrors"))						{ mdzSys_Smtp_ThrowErrors							= ToBool(i_Section.GetAttribute("ThrowErrors")); } }

			i_Element							= i_Root.GetElementsByTagName("WriteLog");
			if (i_Element.Count == 1) {
				i_Section						= (XmlElement) i_Element[0];
				if (i_Section.HasAttribute("File"))										{ mdzSys_WriteLog_File								= i_Section.GetAttribute("File"); } }

			i_Element							= i_Root.GetElementsByTagName("ErrorLog");
			if (i_Element.Count == 1) {
				i_Section						= (XmlElement) i_Element[0];
				if (i_Section.HasAttribute("File"))										{ mdzSys_ErrorLog_File								= i_Section.GetAttribute("File"); }
				if (i_Section.HasAttribute("From"))										{ mdzSys_ErrorLog_From								= i_Section.GetAttribute("From"); }
				if (i_Section.HasAttribute("To"))											{ mdzSys_ErrorLog_To									= i_Section.GetAttribute("To"); }
				if (i_Section.HasAttribute("CC"))											{ mdzSys_ErrorLog_CC									= i_Section.GetAttribute("CC"); }
				if (i_Section.HasAttribute("ThrowErrors"))						{ mdzSys_ErrorLog_ThrowErrors					= ToBool(i_Section.GetAttribute("ThrowErrors")); } }

			i_Element							= i_Root.GetElementsByTagName("PerformanceCheck");
			if (i_Element.Count == 1) {
				i_Section						= (XmlElement) i_Element[0];
				if (i_Section.HasAttribute("Enable"))									{ mdzSys_PerformanceCheck_Enable			= ToBool(i_Section.GetAttribute("Enable")); }
				if (i_Section.HasAttribute("File"))										{ mdzSys_PerformanceCheck_File				= i_Section.GetAttribute("File"); } } }
		catch {}
	}


	~mdzSys() { }


/*
SendEmail Function
==================

Simple function used to send emails.

Body parameter is expected to be HTML format email message content.
Optional Attachments parameter should be list of files to attach separated by ';' characters.

The mdzSys_Smtp_* static constants should be set to configure email for the app.
The LogErrors constant can be overridden as a parameter to the function.
The 'System.Web.Mail.MailMessage' is used for the email.  This is deprecated, but still implemented.
*/


	public bool SendEmail(string p_From, string p_To, string p_CC, string p_BCC, string p_Subject, string p_Body)
	{
		return(SendEmail(p_From, p_To, p_CC, p_BCC, p_Subject, p_Body, "", mdzSys_Smtp_LogErrors));
	}


	public bool SendEmail(string p_From, string p_To, string p_CC, string p_BCC, string p_Subject, string p_Body, bool p_LogErrors)
	{
		return(SendEmail(p_From, p_To, p_CC, p_BCC, p_Subject, p_Body, "", p_LogErrors));
	}


	public bool SendEmail(string p_From, string p_To, string p_CC, string p_BCC, string p_Subject, string p_Body, string p_Attachments)
	{
		return(SendEmail(p_From, p_To, p_CC, p_BCC, p_Subject, p_Body, p_Attachments, mdzSys_Smtp_LogErrors));
	}


	public bool SendEmail(string p_From, string p_To, string p_CC, string p_BCC, string p_Subject, string p_Body, string p_Attachments, bool p_LogErrors)
	{
		System.Web.Mail.MailMessage		i_Message;
		string												i_Attachments		= "";
		string												i_Attachment		= "";
		int														i_Sub						= 0;
		bool													i_Sent					= false;
		
		try {
			i_Message							= new System.Web.Mail.MailMessage();
			i_Message.From				= p_From;
			i_Message.To					= p_To;

			if (p_CC != "")		{ i_Message.Cc	= p_CC; }
			if (p_BCC != "")	{ i_Message.Bcc	= p_BCC; }

			i_Message.Subject			= p_Subject;
			i_Message.BodyFormat	= System.Web.Mail.MailFormat.Html;
			i_Message.Body				= p_Body;

			i_Attachments					= p_Attachments.Trim();
			while (i_Attachments != "") {
				i_Sub								= i_Attachments.IndexOf(";");
				if (i_Sub == -1) {
					i_Attachment			= i_Attachments;
					i_Attachments			= ""; }
				else {
					i_Attachment			= i_Attachments.Substring(0, i_Sub);
					i_Attachments			= i_Attachments.Substring(i_Sub + 1); }
				i_Message.Attachments.Add(new System.Web.Mail.MailAttachment(i_Attachment)); }

			System.Web.Mail.SmtpMail.SmtpServer		= mdzSys_Smtp_Server;
			if (mdzSys_Smtp_Authenticate == true) {
				i_Message.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport",			mdzSys_Smtp_Authenticate_Port);
				i_Message.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate",		"1");    
				i_Message.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername",				mdzSys_Smtp_Authenticate_UserName);
				i_Message.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword",				mdzSys_Smtp_Authenticate_Password); }

			System.Web.Mail.SmtpMail.Send(i_Message);
			i_Sent								= true; }
		catch(Exception ex) {
			if (p_LogErrors == true) {
				ErrorLog("mdzSys.cs", "SendEmail('" + p_From + "', '" + p_To + "', '" + p_CC + "', '" + p_BCC + "', '" + p_Subject + "', '" + p_Body + "')", "", ex); }
			if (mdzSys_Smtp_ThrowErrors == true) {
				throw; } }
		return i_Sent;
	}


/*
WriteLog Function
=================

Common function for writing to logging files for degug/error tracing purposes.
The Entry should be a text value.

Format of is as follows:

{Date & Time} {Function}-{MessageCode} : Entry
20/6/2020 18:30:00 Login-1 : Database error

The function and MessageCode are optional and can be "" or 0 respectively to not include.
The entry will be written to the filenamed specified.
If the filename is not specified, the constant mdzSys_WriteLog_File is used.
If the filename is prefixed with a "~" character, it will be relative to the app directory.
*/


	public bool WriteLog(string p_Filename, string p_Entry)
	{
		return(WriteLog(p_Filename, p_Entry, "", 0));
	}


	public bool WriteLog(string p_Entry)
	{
		return(WriteLog(mdzSys_WriteLog_File, p_Entry, "", 0));
	}


	public bool WriteLog(string p_Entry, string p_Function, int p_MessageCode)
	{
		return(WriteLog(mdzSys_WriteLog_File, p_Entry, p_Function, p_MessageCode));
	}


	public bool WriteLog(string p_Filename, string p_Entry, string p_Function, int p_MessageCode)
	{
		StreamWriter			i_Log;
		string						i_Filename		= "";
		string						i_Function		= "";
		bool							i_Written			= false;

		try {
			i_Filename						= p_Filename;
			if (i_Filename.StartsWith("~") == true) {
				i_Filename					= System.Web.HttpContext.Current.Server.MapPath(i_Filename.Substring(1)); }
			if (p_Function != "") {
				if (p_MessageCode != 0) {
					i_Function				= " " + p_Function + "-" + p_MessageCode.ToString(); }
				else {
					i_Function				= " " + p_Function; } }
			else {
				if (p_MessageCode != 0) {
					i_Function				= " " + p_MessageCode.ToString(); } }

			lock (c_WriteLogLock) {
				i_Log								= File.AppendText(i_Filename);
				i_Log.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + i_Function + " : " + p_Entry);
				i_Log.Dispose(); }
			i_Written							= true; }
		catch {
			i_Written							= false; }
		return i_Written;
	}


/*
ErrorLog Function
=================

Common function for logging application errors.

Depending on mdzSys_ErrorLog_* configuration constants, will write to an error log file and/or send en email.

Returns the <body> element of the email content so that it can be used in an error page if required.
*/


	public XmlElement ErrorLog (string p_Program, string p_Routine, string p_Action, Exception p_Exception)
	{
		return(ErrorLog(p_Program, p_Routine, p_Action, p_Exception.Source, p_Exception.Message));
	}


	public XmlElement ErrorLog(string p_Program, string p_Routine, string p_Action, string p_Message)
	{
		return(ErrorLog(p_Program, p_Routine, p_Action, "", p_Message));
	}


	public XmlElement ErrorLog(string p_Program, string p_Routine, string p_Action, string p_Source, string p_Message)
	{
		StreamWriter			i_Log;
		XmlDocument				i_Xml;
		XmlElement				i_Body				= null;
		XmlElement				i_Table;
		XmlElement				i_TBody;
		string						i_IP;
		string						i_Url;
		string						i_Referrer;
		string						i_Framework;

		try {
			if (p_Message == "Thread was being aborted.") {
				return null; }

			try { i_IP				= System.Web.HttpUtility.HtmlEncode(System.Web.HttpContext.Current.Request.UserHostAddress);				} catch(Exception ex) { i_IP				= "ERR:" + ex.Message; }
			try { i_Url				= System.Web.HttpUtility.HtmlEncode(System.Web.HttpContext.Current.Request.Url.ToString());					} catch(Exception ex) { i_Url				= "ERR:" + ex.Message; }
			try { i_Referrer	= System.Web.HttpUtility.HtmlEncode(System.Web.HttpContext.Current.Request.UrlReferrer.ToString());	} catch(Exception ex) { i_Referrer	= "ERR:" + ex.Message; }
			try { i_Framework	= RuntimeInformation.FrameworkDescription;																													} catch(Exception ex) { i_Framework	= "ERR:" + ex.Message; }

			i_Xml									= new XmlDocument();
			i_Body								= i_Xml.CreateElement("body");
			i_Xml.AppendChild(i_Body);
			XmlAddElement(i_Body, "p", "mdz Error Log :");
			i_Table								= i_Xml.CreateElement("table");
			i_Body.AppendChild(i_Table);
			i_TBody								= i_Xml.CreateElement("table");
			i_Table.AppendChild(i_TBody);
			ErrorLog_AddItem(i_TBody, "Program",		p_Program);
			ErrorLog_AddItem(i_TBody, "Routine",		p_Routine);
			ErrorLog_AddItem(i_TBody, "Action",			p_Action);
			ErrorLog_AddItem(i_TBody, "Source",			p_Source);
			ErrorLog_AddItem(i_TBody, "Message",		p_Message);
			ErrorLog_AddItem(i_TBody, "IP Address",	i_IP);
			ErrorLog_AddItem(i_TBody, "URL",				i_Url);
			ErrorLog_AddItem(i_TBody, "Referer",		i_Referrer);
			ErrorLog_AddItem(i_TBody, "Framework",	i_Framework); }
		catch {
			if (mdzSys_ErrorLog_ThrowErrors == true) {
				throw; }
			return null; }

		try {
			if (mdzSys_ErrorLog_File != "") {
				i_Log							= File.AppendText(System.Web.HttpContext.Current.Server.MapPath(mdzSys_ErrorLog_File));
				i_Log.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ", Prg:" + p_Program + ", Rtn:" + p_Routine + ", Act:" + p_Action + ", Src:" + p_Source + ", Msg:" + p_Message);
				i_Log.Dispose(); } }
		catch {
			if ((mdzSys_ErrorLog_From == "" || mdzSys_ErrorLog_To == "") && mdzSys_ErrorLog_ThrowErrors == true) {
				throw; } }

		try {
			if (mdzSys_ErrorLog_From != "" && mdzSys_ErrorLog_To != "") {
				SendEmail(mdzSys_ErrorLog_From, mdzSys_ErrorLog_To, mdzSys_ErrorLog_CC, "", "mdz Error Log - " + p_Program, i_Body.OuterXml); } }
		catch {
			if (mdzSys_ErrorLog_ThrowErrors == true) {
				throw; } }
		return i_Body;
	}


	private void ErrorLog_AddItem(XmlElement p_Table, string p_Description, string p_Value)
	{
		XmlElement				i_Tr;

		i_Tr									= XmlAddElement(p_Table, "tr");
		XmlAddElement(i_Tr, "td", p_Description);
		XmlAddElement(i_Tr, "td", p_Value);
	}


/*
HttpHit Function
================

Simple function used to make a http hit and return the response as a string.

The function is expected to complete and return the response as a string.
If the submission is not completed for any reason, including a response code other than 200,
an error is thrown.
*/


	public string HttpHit(string p_URL)
	{
		return (HttpHit("GET", p_URL, "", "", "", ""));
	}


	public string HttpHit(string p_Method, string p_URL, string p_Content, string p_ContentType)
	{
		return(HttpHit(p_Method, p_URL, p_Content, p_ContentType, "", ""));
	}


	public string HttpHit(string p_Method, string p_URL, string p_Content, string p_ContentType, string p_UserName, string p_Password)
	{
		mdzWebRequest			i_Request;

		try
		{
			i_Request								= new mdzWebRequest();
			i_Request.URL						= p_URL;
			i_Request.Method				= p_Method;
			i_Request.Content				= p_Content;
			i_Request.ContentType		= p_ContentType;
			i_Request.UserName			= p_UserName;
			i_Request.Password			= p_Password;
			i_Request.Submit();
			if (i_Request.ResponseCode != HttpStatusCode.OK) {
				throw new Exception("HTTP Status Code = " + i_Request.ResponseCode); }
			if (i_Request.IsBinary == true) {
				throw new Exception("Response in unexpected binary format"); } }
		catch(Exception ex) {
			ErrorLog("mdzSys.cs", "HttpHit('" + p_Method + "', '" + p_URL + "', '" + p_Content + "', '" + p_ContentType + "', '" + p_UserName + "', '" + p_Password + "')", "", ex);
			throw; }
		return i_Request.Response;
	}


/*
PerformanceCheck Function
=========================

A generic routine to monitor performance of processes.
A calling routine should record the Datetime.Now at the start of the process.
It can then call PerformaceCheck once the process is complete passing the recorded start time and how many seconds are allowable.
If the process completed outside the allowed time, an ErrorLog is made passing the Program, Routine and Action parameters together
with a text decription of the timings followed by the Options parameter.
If the mdzSys_PerformanceCheck_Enable configuration is true, an entry will be written in the mdzSys_PerformanceCheck_File log.
*/


	public void PerformanceCheck(DateTime p_TimeIn, int p_AllowSeconds, string p_Program, string p_Routine, string p_Action, string p_Options)
	{
		DateTime		i_TimeOut;
		TimeSpan		i_TimeSpan;
		string			i_Message			= "";

		try {
			if (mdzSys_PerformanceCheck_Enable == false) {
				return; }
			i_TimeOut							= DateTime.Now;
			i_TimeSpan						= i_TimeOut - p_TimeIn;
			if (i_TimeSpan.TotalSeconds > p_AllowSeconds) {
				i_Message						= "Performance Log - " + p_Action + " Time Exceeded " + p_AllowSeconds.ToString() + " Seconds, took " + i_TimeSpan.TotalSeconds.ToString() + ((p_Options != "") ? " : Options = " + p_Options : "");
				ErrorLog(p_Program, p_Routine, p_Action, i_Message);
				if (mdzSys_PerformanceCheck_File != "") {
					WriteLog(mdzSys_PerformanceCheck_File, i_Message, p_Program + ((p_Routine != "") ? "-" + p_Routine : "") + ((p_Action != "") ? "-" + p_Action : ""), 0); } } }
		catch { }
	}


/*
AddError Function
=================

MyDocz .Net apps uise XSLT stylesheets for page and Ajax responses.
The code creates an XML document that all data required for the page is added to.
Within this XML document is a <Errors> element containing <Error> elements for standard error handling.
Each error consist of the field to which it applies and a message.
The AddError routine writes an error in to the XML document.
It should be passed the XML Document, the field and message.
*/

	public void AddError(XmlElement p_Element, string p_Field, string p_Message)
	{
		XmlElement			i_Root;
		XmlNodeList			i_Nodes;
		XmlElement			i_Errors;
		XmlElement			i_Error;

		try {
			i_Root					= p_Element.OwnerDocument.DocumentElement;
			i_Nodes					= i_Root.SelectNodes("Errors");
			switch (i_Nodes.Count) {
				case 0:		i_Errors		= i_Root.OwnerDocument.CreateElement("Errors");
							i_Root.AppendChild(i_Errors);
							break;
				case 1:		i_Errors		= i_Nodes[0] as XmlElement;
							break;
				default:	throw new Exception("Multiple Errors nodes encountered"); }
			i_Error					= i_Errors.OwnerDocument.CreateElement("Error");
			i_Error.SetAttribute("Field",	p_Field);
			i_Error.SetAttribute("Message",	p_Message);
			i_Errors.AppendChild(i_Error); }
		catch(Exception ex) {
			ErrorLog("mdzSys.cs", "AddError(Object, '" + p_Field + "', '" + p_Message + "')", "", ex);
			throw; }
	}


/*
XML Helper functions
====================

These functions are designed to simplify general XML processing, but also for use in creating XML for 
transformation into web page response.

XmlAddElement				: Passed the Parent Element, will append a child element and optionally set its value (text).

xmlGet{DataType}		: Passed the Parent and child's tag, will return the child's value in that data type.
										  If the DefaultValue parameter is passed this is returned if the element does not exist.
											If the DefaultValue parameter is not passed an Exception is thrown if no element exists.
											If multiple elements exist, an Exception is thrown.
*/


	public XmlElement XmlAddElement(XmlElement p_Parent, string p_Name)
	{
		XmlElement			i_Item;

		try {
			i_Item					= p_Parent.OwnerDocument.CreateElement(p_Name);
			p_Parent.AppendChild(i_Item); }
		catch(Exception ex) {
			ErrorLog("mdzSys.cs", "XmlAddElement(Object, '" + p_Name + "'') ", "", ex);
			throw; }
		return i_Item;
	}


	public XmlElement XmlAddElement(XmlElement p_Parent, string p_Name, string p_Value)
	{
		XmlElement			i_Item;

		try {
			i_Item					= p_Parent.OwnerDocument.CreateElement(p_Name);
			i_Item.InnerText		= p_Value;
			p_Parent.AppendChild(i_Item); }
		catch(Exception ex) {
			ErrorLog("mdzSys.cs", "AddElement(Object, '" + p_Name + "', '" + p_Value + "') ", "", ex);
			throw; }
		return i_Item;
	}


	public XmlElement XmlAddElement(XmlElement p_Parent, string p_Name, DateTime p_Date)
	{
		XmlElement			i_Item;

		try {
			i_Item					= p_Parent.OwnerDocument.CreateElement(p_Name);
			if (p_Date.CompareTo(c_DateNull) == 0) {
				i_Item.SetAttribute("Null",						"True"); }
			else {
				i_Item.SetAttribute("Null",						"False");
				
				i_Item.SetAttribute("DateTime",				p_Date.ToString("dd/MM/yyyy HH:mm:ss"));
			
				i_Item.SetAttribute("Date",						p_Date.ToString("dd/MM/yyyy"));
				i_Item.SetAttribute("DateFull",				p_Date.ToString("dddd d MMMM yyyy"));
				i_Item.SetAttribute("DateFullNoYear",	p_Date.ToString("dddd d MMMM"));
				i_Item.SetAttribute("DateLong",				p_Date.ToString("d MMMM yyyy"));
				i_Item.SetAttribute("DateShort",			p_Date.ToString("dd MMM yy"));
				i_Item.SetAttribute("DateMini",				p_Date.ToString("dd MMM"));
				i_Item.SetAttribute("DateMicro",			p_Date.ToString("dd/MM"));

				i_Item.SetAttribute("DayOfWeek",			p_Date.ToString("dddd"));
				i_Item.SetAttribute("DayOfWeekShort",	p_Date.ToString("ddd"));
			
				i_Item.SetAttribute("Time",						p_Date.ToString("HH:mm:ss"));
				i_Item.SetAttribute("TimeAM",					p_Date.ToString("h:mm:ss tt"));
				i_Item.SetAttribute("TimeShort",			p_Date.ToString("HH:mm"));

				i_Item.SetAttribute("ddmmyy",					p_Date.ToString("dd/MM/yy"));
				i_Item.SetAttribute("dmyyyy",					p_Date.ToString("d/M/yyyy"));
				i_Item.SetAttribute("dmyy",						p_Date.ToString("d/M/yy"));
				i_Item.SetAttribute("mmyy",						p_Date.ToString("MM/yy"));
				i_Item.SetAttribute("yyyymmdd",				p_Date.ToString("yyyyMMdd")); }
			p_Parent.AppendChild(i_Item); }
		catch(Exception ex) {
			ErrorLog("mdzSys.cs", "AddElement(Object, '" + p_Name + "', '" + p_Date.ToString() + "') ", "", ex);
			throw; }
		return i_Item;
	}


	public string XmlGetString(XmlElement p_Parent, string p_TagName, string p_DefaultValue)
	{
		XmlNodeList 			i_Element;
		string						i_Value			= "";

		try {
			i_Element							= p_Parent.GetElementsByTagName(p_TagName);
			if (i_Element.Count == 0) {
				return p_DefaultValue; }
			if (i_Element.Count != 1) {
				throw new Exception("Multiple " + p_TagName + " XML Elements found"); }
			i_Value								= i_Element[0].InnerText; }
		catch(Exception ex) {
			ErrorLog("mdzSys.cs", "XmlGetElementValue(Object, '" + p_TagName + "')", "", ex);
			throw; }
		return i_Value;
	}


	public string XmlGetString(XmlElement p_Parent, string p_TagName)
	{
		XmlNodeList 			i_Element;
		string						i_Value			= "";

		try {
			i_Element							= p_Parent.GetElementsByTagName(p_TagName);
			if (i_Element.Count != 1) {
				throw new Exception("Not 1 " + p_TagName + " XML Element"); }
			i_Value								= i_Element[0].InnerText; }
		catch(Exception ex) {
			ErrorLog("mdzSys.cs", "XmlGetElementValue(Object, '" + p_TagName + "')", "", ex);
			throw; }
		return i_Value;
	}


	public bool XmlGetBool(XmlElement p_Parent, string p_TagName, bool p_DefaultValue)
	{
		return ToBool(XmlGetString(p_Parent, p_TagName, p_DefaultValue.ToString()));
	}


	public bool XmlGetBool(XmlElement p_Parent, string p_TagName)
	{
		return ToBool(XmlGetString(p_Parent, p_TagName));
	}


	public int XmlGetInt(XmlElement p_Parent, string p_TagName, int p_DefaultValue)
	{
		return ToInt(XmlGetString(p_Parent, p_TagName, p_DefaultValue.ToString()));
	}


	public int XmlGetInt(XmlElement p_Parent, string p_TagName)
	{
		return ToInt(XmlGetString(p_Parent, p_TagName));
	}


	public long XmlGetLong(XmlElement p_Parent, string p_TagName, long p_DefaultValue)
	{
		return ToLong(XmlGetString(p_Parent, p_TagName, p_DefaultValue.ToString()));
	}


	public long XmlGetLong(XmlElement p_Parent, string p_TagName)
	{
		return ToLong(XmlGetString(p_Parent, p_TagName));
	}


	public decimal XmlGetDecimal(XmlElement p_Parent, string p_TagName, decimal p_DefaultValue)
	{
		return ToDecimal(XmlGetString(p_Parent, p_TagName, p_DefaultValue.ToString()));
	}


	public decimal XmlGetDecimal(XmlElement p_Parent, string p_TagName)
	{
		return ToDecimal(XmlGetString(p_Parent, p_TagName));
	}


	public DateTime XmlGetDate(XmlElement p_Parent, string p_TagName, DateTime p_DefaultValue)
	{
		return ToDate(XmlGetString(p_Parent, p_TagName, p_DefaultValue.ToString()));
	}


	public DateTime XmlGetDate(XmlElement p_Parent, string p_TagName)
	{
		return ToDate(XmlGetString(p_Parent, p_TagName));
	}



/*
Posted Form / URL Encoded Fields Helper functions
=================================================

These functions are designed to simplify extracting of posted form fields and fields encoded on the URL.

Get{DataType}				: Given the field's name, returns its value in that data type.

*/

	public string GetString(string p_Name)
	{
		HttpRequest		i_Request;
		string				i_Param		= "";
		string				i_Test		= "";
		string[]			i_Banned	= { "<script", "<iframe" };
		int						i_Sub		= 0;
		bool					i_Dirty		= false;

		try {
			i_Request							= System.Web.HttpContext.Current.Request;
			if (i_Request[p_Name] == null) {
				i_Param							= ""; }
			else {
				i_Param							= i_Request[p_Name].ToString(); }

			if (i_Param != "") {
				i_Dirty							= false;
				i_Test							= i_Param.ToLower();
				i_Test							= Regex.Replace(i_Test, "\\s+", "");
				for (i_Sub = 0; i_Sub < i_Banned.Length; i_Sub++) {
					if (i_Test.IndexOf(i_Banned[i_Sub]) != -1)	{
						i_Dirty					= true; } }
				if (i_Dirty) {
					i_Param						= ""; } } }
		catch {
			i_Param								= ""; }
		return i_Param;
	}


	public bool GetBool(string p_Name)
	{
		return ToBool(GetString(p_Name));
	}


	public int GetInt(string p_Name)
	{
		return ToInt(GetString(p_Name));
	}


	public long GetLong(string p_Name)
	{
		return ToLong(GetString(p_Name));
	}


	public decimal GetDecimal(string p_Name)
	{
		return ToDecimal(GetString(p_Name));
	}


	public DateTime GetDate(string p_Name)
	{
		return ToDate(GetString(p_Name));
	}


//	Requires 2 fields.  2nd field named p_Name + '_Time'.

	public DateTime GetDateAndTime(string p_Name)
	{
		DateTime	i_Param		= DateTime.Now;
		string		i_Text		= "";
		string		i_Time		= "";

		try {
			i_Param								= mdzSys.i.DateNull;
			i_Text								= GetString(p_Name);
			i_Time								= GetString(p_Name + "_Time");

			if (i_Text != "") {
				if (i_Time != "") {
					i_Text						= i_Text + " " + i_Time; }
				i_Param							= DateTime.Parse(i_Text); } }
		catch {
			i_Param								= mdzSys.i.DateNull; }
		return i_Param;
	}


/*
DataType Helper functions
=========================

These provide repetetively used functions in an app.

To{DataType}				: Passed a string value and will return the value in that data type.

IsNumeric						: Passed a string will return true if all characters are numeric digits, otherwise
                	    (or if string is blank) false.

NoNull							: Passed a string, will return the string or "" if the string is null or 0 length.

*/


	public bool ToBool(string p_Value)
	{
		bool		i_Param	= false;
		string		i_Text;

		try {
			i_Param			= false;
			i_Text			= p_Value.ToLower();
			if (i_Text == "true" || i_Text == "yes" || i_Text == "1") {
				i_Param		= true; } }
		catch {
			i_Param		= false; }
		return i_Param;
	}


	public int ToInt(string p_Value)
	{
		int		i_Param	= 0;

		try {
			i_Param		= 0;
			i_Param		= Int32.Parse(p_Value); }
		catch {
			i_Param		= 0; }
		return i_Param;
	}


	public long ToLong(string p_Value)
	{
		long		i_Param	= 0;

		try {
			i_Param		= 0;
			i_Param		= Int64.Parse(p_Value); }
		catch {
			i_Param		= 0; }
		return i_Param;
	}


	public decimal ToDecimal(string p_Value)
	{
		decimal		i_Param	= 0;

		try {
			i_Param		= 0;
			i_Param		= Decimal.Parse(p_Value); }
		catch {
			i_Param		= 0; }
		return i_Param;
	}


	public DateTime ToDate(string p_Value)
	{
		DateTime		i_Param	= mdzSys.i.DateNull;

		try {
			i_Param		= mdzSys.i.DateNull;
			i_Param		= DateTime.Parse(p_Value); }
		catch {
			i_Param		= mdzSys.i.DateNull; }
		return i_Param;
	}


	public bool IsNumeric(string p_Value)
	{
		bool	i_ReturnVal;
		int		i_Len;
		int		i_Pos;

		try {
			if (p_Value == "") {
				return false; }

			i_Len					= p_Value.Length;
			i_ReturnVal		= true;
			for (i_Pos = 0; i_Pos < i_Len; i_Pos++) {
				if (String.Compare(p_Value, i_Pos, "0", 0, 1) < 0)	{ i_ReturnVal = false; }
				if (String.Compare(p_Value, i_Pos, "9", 0, 1) > 0)	{ i_ReturnVal = false; } } }
		catch(Exception ex) {
			ErrorLog("mdzSys.cs", "IsNumeric('" + p_Value + "')", "", ex);
			throw; }
		return i_ReturnVal;
	}


	public string NoNull(string p_Value)
	{
		string		i_Value;

		try {
			if (p_Value == null || p_Value.Length == 0) {
				i_Value		= ""; }
			else {
				i_Value		= p_Value; } }
		catch(Exception ex) { 
			ErrorLog("mdzSys.cs", "NoNull(Str)", "", ex);
			throw; }
		return i_Value;
	}


}

