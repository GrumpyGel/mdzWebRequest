using System;
using System.ComponentModel;
using System.Collections;
using System.Threading;
using System.Data;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Net;
using System.Net.Sockets;
using System.Web;

/*

mdzWebRequest
=============

Alternative to httpWebRequest allowing modern ciphers on versions of Windows Server prior to 2022.

For documentation see : https://github.com/GrumpyGel/mdzWebRequest

Classes:

mdzWebRequest          Class for making web requests.
mdzWebRequestConfig    Class used by mdzWebRequest for configuration, implemented as a static Singleton instance declaration published as 'i' property.

*/

public class mdzWebRequestConfig
{

  private static readonly mdzWebRequestConfig s_Instance = new mdzWebRequestConfig();
  
  public static mdzWebRequestConfig i { get { return s_Instance; } }

  bool          c_IP_AutoAllow          = true;
  int           c_IP_Qty                = 0;
  IPAddress[]   c_IP_Exceptions         = new IPAddress[20];
  int[]         c_IP_MaskLength         = new int[20];
  uint[]        c_IP_AsUint             = new uint[20];
  BitArray[]    c_IP_AsBitArray         = new BitArray[20];
  int[]         c_IP_Idx                = new int[20];
  
  bool          c_Host_AutoAllow        = true;
  int           c_Host_Qty              = 0;
  string[]      c_Host_Exceptions       = new string[20];
  int[]         c_Host_Idx              = new int[20];

  string        c_ValidationLog         = "";


  static mdzWebRequestConfig() { }


  private mdzWebRequestConfig()
  {
    XmlDocument   i_Config;
    XmlElement    i_Root;
    XmlElement    i_Section;
    XmlNodeList   i_Element;
    IPAddress     i_IPAddress;
    string        i_Filename          = "";
    string        i_Value             = "";
    int           i_MaskLength        = 0;
    byte[]        i_IP_AsBytes;
    string        i_Pos               = "Init";
    string        i_Errors            = "";
    int           i_Sub               = 0;
    int           i_Idx               = 0;

    try {
      i_Filename            = System.Web.HttpContext.Current.Server.MapPath("mdzWebRequest.config");
      if (File.Exists(i_Filename) == false) {
        return; }
      i_Config              = new XmlDocument();
      i_Config.Load(i_Filename);
      i_Pos                 = "Root";
      i_Root                = i_Config.DocumentElement;
      try {  if (i_Root.HasAttribute("IP_AutoAllow"))    { c_IP_AutoAllow    = mdzSys.i.ToBool(i_Root.GetAttribute("IP_AutoAllow"));    } } catch { i_Errors = i_Errors + ", IP_AutoAllow"; }
      try {  if (i_Root.HasAttribute("Host_AutoAllow"))  { c_Host_AutoAllow  = mdzSys.i.ToBool(i_Root.GetAttribute("Host_AutoAllow"));  } } catch { i_Errors = i_Errors + ", Host_AutoAllow"; }
      try {  if (i_Root.HasAttribute("ValidationLog"))   { c_ValidationLog   = i_Root.GetAttribute("ValidationLog");                    } } catch { i_Errors = i_Errors + ", ValidationLog"; }

      i_Pos                  = "Exception";
      i_Element              = i_Root.GetElementsByTagName("Exception");
      for (i_Sub = 0; i_Sub < i_Element.Count; i_Sub++) {
        i_Section            = (XmlElement) i_Element[i_Sub];
        if (i_Section.HasAttribute("Type") == false || i_Section.HasAttribute("Value") == false) {
          i_Errors          = i_Errors + ", Exception[" + i_Sub.ToString() + "]";
          continue; }
        i_Value             = i_Section.GetAttribute("Value").ToLower();
        if (i_Value == "") {
          i_Errors          = i_Errors + ", Exception[" + i_Sub.ToString() + "]/@Value";
          continue; }
        switch (i_Section.GetAttribute("Type")) {
          case "IP":    i_MaskLength                  = 0;
                        i_Idx                         = i_Value.IndexOf("/");
                        if (i_Idx != -1) {
                          i_MaskLength                = int.Parse(i_Value.Substring(i_Idx + 1));
                          i_Value                     = i_Value.Substring(0, i_Idx); }
                        i_IPAddress                   = IPAddress.Parse(i_Value);
                        if (c_IP_Qty == c_IP_Exceptions.Length) {
                          Array.Resize(ref c_IP_Exceptions,   c_IP_Exceptions.Length + 20);
                          Array.Resize(ref c_IP_MaskLength,   c_IP_Exceptions.Length);
                          Array.Resize(ref c_IP_AsUint,       c_IP_Exceptions.Length);
                          Array.Resize(ref c_IP_AsBitArray,   c_IP_Exceptions.Length);
                          Array.Resize(ref c_IP_Idx,          c_IP_Exceptions.Length); }
                        c_IP_Exceptions[c_IP_Qty]     = i_IPAddress;
                        c_IP_MaskLength[c_IP_Qty]     = i_MaskLength;
                        if (i_IPAddress.AddressFamily == AddressFamily.InterNetwork) {
                          i_IP_AsBytes                = i_IPAddress.GetAddressBytes();
                          Array.Reverse(i_IP_AsBytes);
                          c_IP_AsUint[c_IP_Qty]       = BitConverter.ToUInt32(i_IP_AsBytes, 0); }
                        if (i_IPAddress.AddressFamily == AddressFamily.InterNetworkV6) {
                          i_IP_AsBytes                = i_IPAddress.GetAddressBytes();
                          Array.Reverse(i_IP_AsBytes);
                          c_IP_AsBitArray[c_IP_Qty]   = new BitArray(i_IP_AsBytes); }
                        c_IP_Idx[c_IP_Qty]            = i_Sub + 1;
                        c_IP_Qty++;
                        break;
          case "Host":  if (c_Host_Qty == c_Host_Exceptions.Length) {
                          Array.Resize(ref c_Host_Exceptions, c_Host_Exceptions.Length + 20);
                          Array.Resize(ref c_Host_Idx,        c_Host_Exceptions.Length); }
                        c_Host_Exceptions[c_Host_Qty] = i_Value;
                        c_Host_Idx[c_Host_Qty]        = i_Sub + 1;
                        c_Host_Qty++;
                        break;
          default:      i_Errors                      = i_Errors + ", Exception[" + i_Sub.ToString() + "]/@Type";
                        break; } }
      if (i_Errors != "") {
        mdzSys.i.ErrorLog("mdzWebRequest.cs", "mdzWebRequestConfig()", "", "Errors were encountered setting the following properties : " + i_Errors.Substring(2)); } }
    catch(Exception ex) {
        mdzSys.i.ErrorLog("mdzWebRequest.cs", "mdzWebRequestConfig()", "Initialisation at " + i_Pos, ex); }
  }


  ~mdzWebRequestConfig() { }


  public bool Validate(string p_IP, string p_URL)
  {
    IPAddress   i_IP                  = null;
    Uri         i_Uri                 = null;
    bool        i_Allow               = false;
    bool        i_Match               = false;
    byte[]      i_IP_AsBytes;
    uint        i_IP_AsUint           = 0;
    BitArray    i_IP_AsBitArray;
    int         i_IP_Match            = 0;
    int         i_Host_Match          = 0;
    uint        i_Mask                = 0;
    string      i_Host                = "";
    string      i_Msg                 = "";
    int         i_Len                 = 0;
    int         i_Sub                 = 0;
    int         i_Sub2                = 0;
  
    try {
      i_Allow                         = c_IP_AutoAllow;
      if (c_IP_Qty != 0) {
        i_IP                          = IPAddress.Parse(p_IP);
        i_Sub                         = 0;
        while (i_Sub < c_IP_Qty) {
          i_Match                     = false;
          if (i_IP.AddressFamily != c_IP_Exceptions[i_Sub].AddressFamily) {
            i_Sub++;
            continue; }
          if (c_IP_MaskLength[i_Sub] == 0) {
            if (i_IP.Equals(c_IP_Exceptions[i_Sub]) == true) {
              i_Match                 = true; } }
          else if (i_IP.AddressFamily == AddressFamily.InterNetwork) {
            i_IP_AsBytes              = i_IP.GetAddressBytes();
            Array.Reverse(i_IP_AsBytes);
            i_IP_AsUint               = BitConverter.ToUInt32(i_IP_AsBytes, 0);
            i_Mask = uint.MaxValue << (32 - c_IP_MaskLength[i_Sub]);
            if ((c_IP_AsUint[i_Sub] & i_Mask) == (i_IP_AsUint & i_Mask)) {
              i_Match                 = true; } }
          else if (i_IP.AddressFamily == AddressFamily.InterNetworkV6) {
            i_IP_AsBytes              = i_IP.GetAddressBytes();
            Array.Reverse(i_IP_AsBytes);
            i_IP_AsBitArray           = new BitArray(i_IP_AsBytes);
            if (c_IP_AsBitArray[i_Sub].Length != i_IP_AsBitArray.Length) {
              throw new Exception("Length of IP Address and Subnet Mask do not match."); }
            i_Len                     = i_IP_AsBitArray.Length;
            i_Match                   = true;
            for (i_Sub2 = i_Len - 1; i_Sub2 >= i_Len - c_IP_MaskLength[i_Sub]; i_Sub2--) {
              if (i_IP_AsBitArray[i_Sub2] != c_IP_AsBitArray[i_Sub][i_Sub2]) {
                i_Match               = false;
                break; } } }
          if (i_Match == true) {
            i_IP_Match                = c_IP_Idx[i_Sub];
            if (c_IP_AutoAllow == true) {
              i_Allow                 = false; }
            else {
              i_Allow                 = true; }
            break; }
          i_Sub++; } }
      if (i_Allow == false) {
        if (c_ValidationLog != "") {
          if (c_IP_Qty == 0) {
            i_IP                      = IPAddress.Parse(p_IP); }
          i_Msg                       = "mdzWebRequest : IP Validation Failed" +
                                          ((i_IP_Match == 0) ? ", No Exception Match" : ", Exception Match " + i_IP_Match.ToString()) +
                                          ", IP=[" + p_IP + "], ParsedAs=[" + i_IP.ToString() + "]";
          mdzSys.i.WriteLog(c_ValidationLog, i_Msg); }
        return false; }
      i_Allow                         = c_Host_AutoAllow;
      if (c_Host_Qty != 0) {
        i_Uri                         = new Uri(p_URL);
        i_Host                        = i_Uri.Host.ToLower();
        i_Sub                         = 0;
        while (i_Sub < c_Host_Qty) {
          if (i_Host == c_Host_Exceptions[i_Sub]) {
            i_Host_Match              = c_Host_Idx[i_Sub];
            if (c_Host_AutoAllow == true) {
              i_Allow                 = false; }
            else {
              i_Allow                 = true; }
            break; }
          i_Sub++; } }
      if (i_Allow == false) {
        if (c_ValidationLog != "") {
          if (c_Host_Qty == 0) {
            i_Uri                     = new Uri(p_URL);
            i_Host                    = i_Uri.Host.ToLower(); }
          i_Msg                       = "mdzWebRequest : Host Validation Failed" +
                                          ((i_Host_Match == 0) ? ", No Exception Match" : ", Exception Match " + i_Host_Match.ToString()) +
                                          ", ParsedHost=[" + i_Host + "], URL=[" + p_URL + "]";
          mdzSys.i.WriteLog(c_ValidationLog, i_Msg); }
        return false; } }
    catch(Exception ex) {
        mdzSys.i.ErrorLog("mdzWebRequest.cs", "Validate()", "", ex);
        return false; }
    return true;
  }

}


public class mdzWebRequest
{
  string          c_URL;
  string          c_Method;
  string          c_Content;
  string          c_ContentType;
  string          c_UserName;
  string          c_Password;
  string          c_ExpectedFormat;
  int             c_MaxBinarySize;

  bool            c_UseProxy;
  string          c_ProxyURL;
  string          c_ProxyUserName;
  string          c_ProxyPassword;

  int             c_ErrNo;
  string          c_ErrMsg;
  HttpStatusCode  c_ResponseCode;
  string          c_ResponseType;
  string          c_ResponseTypeParams;
  bool            c_IsBinary;
  string          c_Response;
  byte[]          c_ResponseBinary;


  public const string   mdzwr_Method_Get                      = "GET";
  public const string   mdzwr_Method_Post                     = "POST";
  public const string   mdzwr_Method_Put                      = "PUT";

  public const string   mdzwr_ContentType_FormUrlEncoded      = "application/x-www-form-urlencoded";
  public const string   mdzwr_ContentType_Xml                 = "text/xml; encoding='utf-8'";

  public const string   mdzwr_ExpectedFormat_Detect           = "Detect";
  public const string   mdzwr_ExpectedFormat_Text             = "Text";
  public const string   mdzwr_ExpectedFormat_Binary           = "Binary";

  public const int      mdzwr_MaxBinarySize                   = 10485760;    // 10mb

  public const int      mdzwr_ErrNo_NoURL                     = 14001;
  public const int      mdzwr_ErrNo_InvalidMethod             = 14002;
  public const int      mdzwr_ErrNo_ProxyURLNotSet            = 14003;
  public const int      mdzwr_ErrNo_NotAllowed                = 14004;
  public const int      mdzwr_ErrNo_ProxySubmitError          = 14011;
  public const int      mdzwr_ErrNo_ProxyResopnseCode         = 14012;
  public const int      mdzwr_ErrNo_ProxyResopnse             = 14013;

//  Taken from https://www.w3.org/TR/xhtml1/dtds.html#h-A2

  const string          mdzwr_EntityDeclaration               = "<!DOCTYPE root [ <!ENTITY nbsp \"&#160;\"><!ENTITY iexcl \"&#161;\"><!ENTITY cent \"&#162;\"><!ENTITY pound \"&#163;\"><!ENTITY curren \"&#164;\"><!ENTITY yen \"&#165;\"><!ENTITY brvbar \"&#166;\"><!ENTITY sect \"&#167;\"><!ENTITY uml \"&#168;\"><!ENTITY copy \"&#169;\"><!ENTITY ordf \"&#170;\"><!ENTITY laquo \"&#171;\"><!ENTITY not \"&#172;\"><!ENTITY shy \"&#173;\"><!ENTITY reg \"&#174;\"><!ENTITY macr \"&#175;\"><!ENTITY deg \"&#176;\"><!ENTITY plusmn \"&#177;\"><!ENTITY sup2 \"&#178;\"><!ENTITY sup3 \"&#179;\"><!ENTITY acute \"&#180;\"><!ENTITY micro \"&#181;\"><!ENTITY para \"&#182;\"><!ENTITY middot \"&#183;\"><!ENTITY cedil \"&#184;\"><!ENTITY sup1 \"&#185;\"><!ENTITY ordm \"&#186;\"><!ENTITY raquo \"&#187;\"><!ENTITY frac14 \"&#188;\"><!ENTITY frac12 \"&#189;\"><!ENTITY frac34 \"&#190;\"><!ENTITY iquest \"&#191;\"><!ENTITY Agrave \"&#192;\"><!ENTITY Aacute \"&#193;\"><!ENTITY Acirc \"&#194;\"><!ENTITY Atilde \"&#195;\"><!ENTITY Auml \"&#196;\"><!ENTITY Aring \"&#197;\"><!ENTITY AElig \"&#198;\"><!ENTITY Ccedil \"&#199;\"><!ENTITY Egrave \"&#200;\"><!ENTITY Eacute \"&#201;\"><!ENTITY Ecirc \"&#202;\"><!ENTITY Euml \"&#203;\"><!ENTITY Igrave \"&#204;\"><!ENTITY Iacute \"&#205;\"><!ENTITY Icirc \"&#206;\"><!ENTITY Iuml \"&#207;\"><!ENTITY ETH \"&#208;\"><!ENTITY Ntilde \"&#209;\"><!ENTITY Ograve \"&#210;\"><!ENTITY Oacute \"&#211;\"><!ENTITY Ocirc \"&#212;\"><!ENTITY Otilde \"&#213;\"><!ENTITY Ouml \"&#214;\"><!ENTITY times \"&#215;\"><!ENTITY Oslash \"&#216;\"><!ENTITY Ugrave \"&#217;\"><!ENTITY Uacute \"&#218;\"><!ENTITY Ucirc \"&#219;\"><!ENTITY Uuml \"&#220;\"><!ENTITY Yacute \"&#221;\"><!ENTITY THORN \"&#222;\"><!ENTITY szlig \"&#223;\"><!ENTITY agrave \"&#224;\"><!ENTITY aacute \"&#225;\"><!ENTITY acirc \"&#226;\"><!ENTITY atilde \"&#227;\"><!ENTITY auml \"&#228;\"><!ENTITY aring \"&#229;\"><!ENTITY aelig \"&#230;\"><!ENTITY ccedil \"&#231;\"><!ENTITY egrave \"&#232;\"><!ENTITY eacute \"&#233;\"><!ENTITY ecirc \"&#234;\"><!ENTITY euml \"&#235;\"><!ENTITY igrave \"&#236;\"><!ENTITY iacute \"&#237;\"><!ENTITY icirc \"&#238;\"><!ENTITY iuml \"&#239;\"><!ENTITY eth \"&#240;\"><!ENTITY ntilde \"&#241;\"><!ENTITY ograve \"&#242;\"><!ENTITY oacute \"&#243;\"><!ENTITY ocirc \"&#244;\"><!ENTITY otilde \"&#245;\"><!ENTITY ouml \"&#246;\"><!ENTITY divide \"&#247;\"><!ENTITY oslash \"&#248;\"><!ENTITY ugrave \"&#249;\"><!ENTITY uacute \"&#250;\"><!ENTITY ucirc \"&#251;\"><!ENTITY uuml \"&#252;\"><!ENTITY yacute \"&#253;\"><!ENTITY thorn \"&#254;\"><!ENTITY yuml \"&#255;\"><!ENTITY quot \"&#34;\"><!ENTITY amp \"&#38;#38;\"><!ENTITY lt \"&#38;#60;\"><!ENTITY gt \"&#62;\"><!ENTITY apos \"&#39;\"><!ENTITY OElig \"&#338;\"><!ENTITY oelig \"&#339;\"><!ENTITY Scaron \"&#352;\"><!ENTITY scaron \"&#353;\"><!ENTITY Yuml \"&#376;\"><!ENTITY circ \"&#710;\"><!ENTITY tilde \"&#732;\"><!ENTITY ensp \"&#8194;\"><!ENTITY emsp \"&#8195;\"><!ENTITY thinsp \"&#8201;\"><!ENTITY zwnj \"&#8204;\"><!ENTITY zwj \"&#8205;\"><!ENTITY lrm \"&#8206;\"><!ENTITY rlm \"&#8207;\"><!ENTITY ndash \"&#8211;\"><!ENTITY mdash \"&#8212;\"><!ENTITY lsquo \"&#8216;\"><!ENTITY rsquo \"&#8217;\"><!ENTITY sbquo \"&#8218;\"><!ENTITY ldquo \"&#8220;\"><!ENTITY rdquo \"&#8221;\"><!ENTITY bdquo \"&#8222;\"><!ENTITY dagger \"&#8224;\"><!ENTITY Dagger \"&#8225;\"><!ENTITY permil \"&#8240;\"><!ENTITY lsaquo \"&#8249;\"><!ENTITY rsaquo \"&#8250;\"><!ENTITY euro \"&#8364;\"> ]><root>";


  public string           URL                 { get { return c_URL;                 }  set { c_URL             = value; } }
  public string           Method              { get { return c_Method;              }  set { c_Method          = value; } }
  public string           Content             { get { return c_Content;             }  set { c_Content         = value; } }
  public string           ContentType         { get { return c_ContentType;         }  set { c_ContentType     = value; } }
  public string           UserName            { get { return c_UserName;            }  set { c_UserName        = value; } }
  public string           Password            { get { return c_Password;            }  set { c_Password        = value; } }
  public string           ExpectedFormat      { get { return c_ExpectedFormat;      }  set { c_ExpectedFormat  = value; } }
  public int              MaxBinarySize       { get { return c_MaxBinarySize;       }  set { c_MaxBinarySize   = value; } }

  public bool             UseProxy            { get { return c_UseProxy;            }  set { c_UseProxy        = value; } }
  public string           ProxyURL            { get { return c_ProxyURL;            }  set { c_ProxyURL        = value; } }
  public string           ProxyUserName       { get { return c_ProxyUserName;       }  set { c_ProxyUserName   = value; } }
  public string           ProxyPassword       { get { return c_ProxyPassword;       }  set { c_ProxyPassword   = value; } }

  public int              ErrNo               { get { return c_ErrNo;               } }
  public string           ErrMsg              { get { return c_ErrMsg;              } }
  public HttpStatusCode   ResponseCode        { get { return c_ResponseCode;        } }
  public string           ResponseType        { get { return c_ResponseType;        } }
  public string           ResponseTypeParams  { get { return c_ResponseTypeParams;  } }
  public bool             IsBinary            { get { return c_IsBinary;            } }
  public string           Response            { get { return c_Response;            } }
  public byte[]           ResponseBinary      { get { return c_ResponseBinary;      } }


  public mdzWebRequest()
  {    
    try {
      c_URL                 = "";
      c_Method              = mdzwr_Method_Get;
      c_Content             = "";
      c_ContentType         = mdzwr_ContentType_FormUrlEncoded;
      c_UserName            = "";
      c_Password            = "";
      c_ExpectedFormat      = mdzwr_ExpectedFormat_Detect;
      c_MaxBinarySize       = mdzwr_MaxBinarySize;
      c_UseProxy            = false;
      c_ProxyURL            = "";
      c_ProxyUserName       = "";
      c_ProxyPassword       = "";
      try {
        c_ProxyURL          = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Host + "/mdzWebRequestProxy.php"; }
      catch {
        c_ProxyURL          = ""; }
      func_InitResponse(); }
    catch(Exception ex) {
      mdzSys.i.ErrorLog("mdzWebRequest.cs", "mdzWebRequest()", "", ex); }
  }


  public bool Submit()
  {
    bool    i_Result        = false;

    try {
      func_InitResponse();
      if (c_URL == "") {
        c_ErrNo                   = mdzwr_ErrNo_NoURL;
        c_ErrMsg                  = "No URL supplied";
        return false; }
      if (c_Method != mdzwr_Method_Get && c_Method != mdzwr_Method_Post && c_Method != mdzwr_Method_Put) {
        c_ErrNo                   = mdzwr_ErrNo_InvalidMethod;
        c_ErrMsg                  = "Invalid Method supplied";
        return false; }
      if (mdzWebRequestConfig.i.Validate(System.Web.HttpContext.Current.Request.UserHostAddress, c_URL) == false) {
        c_ErrNo                   = mdzwr_ErrNo_NotAllowed;
        c_ErrMsg                  = "Client IP or Host blocked";
        return false; }
      if (c_UseProxy == true) {
        if (c_ProxyURL == "") {
          c_ErrNo                 = mdzwr_ErrNo_ProxyURLNotSet;
          c_ErrMsg                = "Proxy URL not set";
          return false; }
        i_Result                  = Submit_Proxy(); }
      else {
        i_Result                  = Submit_Direct(); } }
    catch(Exception ex) {
      mdzSys.i.ErrorLog("mdzWebRequest.cs", "Submit()", "", ex);
      throw; }
    return i_Result;
  }


  private bool Submit_Direct()
  {
    HttpWebRequest  i_Request;
    HttpWebResponse i_Response;
    Stream          i_RequestStream;
    CredentialCache i_CredentialCache;
    StreamReader    i_Reader;
    BinaryReader    i_ReaderBin;
    byte[]          i_PostBytes;

    try {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
      System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      i_Request                   = (HttpWebRequest) WebRequest.Create(c_URL);
      i_Request.Method            = c_Method;

      if (c_UserName != "") {
        i_CredentialCache         = new CredentialCache();
        i_CredentialCache.Add(new System.Uri(c_URL), "Basic", new NetworkCredential(c_UserName, c_Password));
        i_Request.Credentials     = i_CredentialCache;
        i_Request.PreAuthenticate = true; }

      if (c_Content != "") {
        i_PostBytes               = System.Text.Encoding.ASCII.GetBytes(c_Content);
        i_Request.ContentType     = c_ContentType;
        i_Request.ContentLength   = i_PostBytes.Length;
        i_RequestStream           = i_Request.GetRequestStream();
        i_RequestStream.Write(i_PostBytes, 0, i_PostBytes.Length);
        i_RequestStream.Close(); }

      i_Response                  = (HttpWebResponse) i_Request.GetResponse();
      c_ResponseCode              = i_Response.StatusCode;
      func_SetResponseType(i_Response.ContentType);

      if (c_IsBinary == true) {
        i_ReaderBin               = new BinaryReader(i_Response.GetResponseStream());
        c_ResponseBinary          = i_ReaderBin.ReadBytes(c_MaxBinarySize);
        i_ReaderBin.Close(); }
      else {
        i_Reader                  = new StreamReader(i_Response.GetResponseStream(), System.Text.Encoding.UTF8);
        c_Response                = i_Reader.ReadToEnd();
        i_Reader.Close(); }

      i_Response.Close(); }
    catch(Exception ex) {
      mdzSys.i.ErrorLog("mdzWebRequest.cs", "Submit_Direct()", "", ex);
      throw; }
    return true;
  }


  private bool Submit_Proxy()
  {
    HttpWebRequest  i_Request;
    HttpWebResponse i_Response;
    Stream          i_RequestStream;
    CredentialCache i_CredentialCache;
    StreamReader    i_Reader;
    XmlDocument     i_Doc;

    string          i_PostData;
    byte[]          i_PostBytes;
    HttpStatusCode  i_ProxyResponseCode;
    string          i_ProxyResponseType   ="";
    string          i_ProxyResponse       = "";
    string          i_TempResponse        = "";
    string          i_ResponseBase64      = "";

    try {
      i_PostData = "URL=" + HttpUtility.UrlEncode(c_URL) +
                    "&Method=" + HttpUtility.UrlEncode(c_Method) +
                    "&Content=" + HttpUtility.UrlEncode(c_Content) +
                    "&ContentType=" + HttpUtility.UrlEncode(c_ContentType) +
                    "&UserName=" + HttpUtility.UrlEncode(c_UserName) +
                    "&Password=" + HttpUtility.UrlEncode(c_Password);

      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
      System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      i_Request                   = (HttpWebRequest) WebRequest.Create(c_ProxyURL);
      i_Request.Method            = mdzwr_Method_Post;

      if (c_ProxyUserName != "") {
        i_CredentialCache         = new CredentialCache();
        i_CredentialCache.Add(new System.Uri(c_ProxyURL), "Basic", new NetworkCredential(c_ProxyUserName, c_ProxyPassword));
        i_Request.Credentials     = i_CredentialCache;
        i_Request.PreAuthenticate = true; }

      i_PostBytes                 = System.Text.Encoding.ASCII.GetBytes(i_PostData);
      i_Request.ContentType       = mdzwr_ContentType_FormUrlEncoded;
      i_Request.ContentLength     = i_PostBytes.Length;
      i_RequestStream             = i_Request.GetRequestStream();
      i_RequestStream.Write(i_PostBytes, 0, i_PostBytes.Length);
      i_RequestStream.Close();

      i_Response                  = (HttpWebResponse) i_Request.GetResponse();
      i_Reader                    = new StreamReader(i_Response.GetResponseStream(), System.Text.Encoding.UTF8);
      i_ProxyResponseCode         = i_Response.StatusCode;
      i_ProxyResponseType         = i_Response.ContentType;
      i_ProxyResponse             = i_Reader.ReadToEnd();
      i_Reader.Close();
      i_Response.Close(); }
    catch(Exception ex) {
      mdzSys.i.ErrorLog("mdzWebRequest.cs", "Submit_Proxy()", "Submitting To Proxy", ex);
      c_ErrNo                     = mdzwr_ErrNo_ProxySubmitError;
      c_ErrMsg                    = "Error submitting To Proxy : [" + ex.Message + "]";
      return false; }

    try {
      if (i_ProxyResponseCode != HttpStatusCode.OK) {
        c_ErrNo                   = mdzwr_ErrNo_ProxyResopnseCode;
        c_ErrMsg                  = "Invalid Proxy response code : [" + i_ProxyResponseCode + "]";
        return false; }

      i_TempResponse              = i_ProxyResponse.Replace("<root>", mdzwr_EntityDeclaration);
      i_Doc                       = new XmlDocument();
       i_Doc.Load(new StringReader(i_TempResponse));
      c_ErrNo                     = mdzSys.i.XmlGetInt(i_Doc.DocumentElement, "ErrNo");
      c_ErrMsg                    = mdzSys.i.XmlGetString(i_Doc.DocumentElement, "ErrMsg");
      c_ResponseCode              = (HttpStatusCode) mdzSys.i.XmlGetInt(i_Doc.DocumentElement, "ResponseCode");
      func_SetResponseType(mdzSys.i.XmlGetString(i_Doc.DocumentElement, "ResponseType"));
      i_ResponseBase64            = mdzSys.i.XmlGetString(i_Doc.DocumentElement, "Response");

      c_ResponseBinary            = Convert.FromBase64String(i_ResponseBase64);
      if (c_IsBinary == true) {
        c_Response                = ""; }
      else {
        c_Response                = Encoding.UTF8.GetString(c_ResponseBinary);
        c_ResponseBinary          = new byte[0]; } }
    catch(Exception ex) {
      mdzSys.i.ErrorLog("mdzWebRequest.cs", "Submit_Proxy()", "Extracting Proxy Response", ex);
      mdzSys.i.WriteLog("~mdzWebRequest.log", i_TempResponse);
      c_ErrNo                     = mdzwr_ErrNo_ProxyResopnse;
      c_ErrMsg                    = "Error extracting Proxy response : [" + ex.Message + "]";
      return false; }
    return true;
  }


  public XmlElement ToXml(XmlElement p_Root)
  {
    XmlElement  i_Element;

    try {
      i_Element                   = mdzSys.i.XmlAddElement(p_Root, "mdzWebRequest");
      mdzSys.i.XmlAddElement(i_Element, "URL",                c_URL);
      mdzSys.i.XmlAddElement(i_Element, "Method",             c_Method);
      mdzSys.i.XmlAddElement(i_Element, "Content",            c_Content);
      mdzSys.i.XmlAddElement(i_Element, "ContentType",        c_ContentType);
      mdzSys.i.XmlAddElement(i_Element, "UserName",           c_UserName);
      mdzSys.i.XmlAddElement(i_Element, "Password",           c_Password);
      mdzSys.i.XmlAddElement(i_Element, "ExpectedFormat",     c_ExpectedFormat);
      mdzSys.i.XmlAddElement(i_Element, "MaxBinarySize",      c_MaxBinarySize.ToString());
      mdzSys.i.XmlAddElement(i_Element, "UseProxy",           c_UseProxy.ToString());
      mdzSys.i.XmlAddElement(i_Element, "ProxyURL",           c_ProxyURL);
      mdzSys.i.XmlAddElement(i_Element, "ProxyUserName",      c_ProxyUserName);
      mdzSys.i.XmlAddElement(i_Element, "ProxyPassword",      c_ProxyPassword);

      mdzSys.i.XmlAddElement(i_Element, "ErrNo",              c_ErrNo.ToString());
      mdzSys.i.XmlAddElement(i_Element, "ErrMsg",             c_ErrMsg);
      mdzSys.i.XmlAddElement(i_Element, "ResponseCode",       c_ResponseCode.ToString());
      mdzSys.i.XmlAddElement(i_Element, "ResponseType",       c_ResponseType);
      mdzSys.i.XmlAddElement(i_Element, "ResponseTypeParams", c_ResponseTypeParams);
      mdzSys.i.XmlAddElement(i_Element, "IsBinary",           c_IsBinary.ToString());
      mdzSys.i.XmlAddElement(i_Element, "Response",           c_Response);
      mdzSys.i.XmlAddElement(i_Element, "ResponseBinary",     Convert.ToBase64String(c_ResponseBinary)); }
    catch(Exception ex) {
      mdzSys.i.ErrorLog("mdzWebRequest.cs", "ToXml(Object)", "", ex);
      throw; }
    return i_Element;
  }


  private void func_InitResponse()
  {
    try {
      c_ErrNo                     = 0;
      c_ErrMsg                    = "";
      c_ResponseCode              = 0;
      c_ResponseType              = "";
      c_ResponseTypeParams        = "";
      c_IsBinary                  = false;
      c_Response                  = "";
      c_ResponseBinary            = new byte[0]; }
    catch(Exception ex) {
      mdzSys.i.ErrorLog("mdzWebRequest.cs", "func_InitResponse()", "", ex);
      throw; }
  }


  private void func_SetResponseType(string p_ContentType)
  {
    int        i_Pos;

    try {
      i_Pos                       = p_ContentType.IndexOf(";");
      if (i_Pos == -1) {
        c_ResponseType            = p_ContentType;
        c_ResponseTypeParams      = ""; }
      else {
        c_ResponseType            = p_ContentType.Substring(0, i_Pos);
        c_ResponseTypeParams      = p_ContentType.Substring(i_Pos + 1); }
      c_ResponseType              = c_ResponseType.Trim(' ');
      c_ResponseTypeParams        = c_ResponseTypeParams.Trim(' ');
      c_IsBinary                  = false;
      if (c_ExpectedFormat == mdzwr_ExpectedFormat_Binary) {
        c_IsBinary                = true;
        return; }
      if (c_ExpectedFormat == mdzwr_ExpectedFormat_Text) {
        return; }
      if (c_ResponseType.StartsWith("text/") == true) {
        return; }
      if (c_ResponseType == "application/xhtml+xml" || c_ResponseType == "application/xml" || c_ResponseType == "application/json") {
        return; }
      c_IsBinary                  = true; }
    catch(Exception ex) {
      mdzSys.i.ErrorLog("mdzWebRequest.cs", "func_SetResponseType('" + p_ContentType + "')", "", ex);
      throw; }
  }

}