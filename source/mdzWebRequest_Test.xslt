<?xml version="1.0" encoding="Windows-1252"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="xml" indent="yes" omit-xml-declaration="yes"/>


<xsl:template match="/root">
  <html>
    <head>
      <title>MyDocz mdzWebRequest_Test</title>
      <meta http-equiv="Content-Type" content="text/html" charset="utf-8"/>
      <meta charset="utf-8"/>
      <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no"/>
      <link rel="stylesheet" href="/mdzWebRequest_Test.css"/>
      <script language="javascript" src="/mdzWebRequest_Test.js" type="text/javascript"></script>
      <script>
        var Action = `<xsl:value-of select="Action"/>`;
        var ProcessAs = `<xsl:value-of select="ProcessAs"/>`;
        var ResponseType = `<xsl:value-of select="mdzWebRequest/ResponseType"/>`;
        var ResponseBase64 = `<xsl:value-of select="ResponseBase64"/>`;
      </script>
    </head>
    <xsl:choose>
      <xsl:when test="/root/Action = 'Submit'">
        <body onload="InitResponse();">
          <div class="HomeLink"><a href="/">MyDocz Home</a></div>
          <h3>mdzWebRequest Response Details:</h3>
          <xsl:apply-templates mode="Response" select="mdzWebRequest"/>
        </body>
      </xsl:when>
      <xsl:otherwise>
        <body onload="InitRequest();">
          <div class="HomeLink"><a href="/">MyDocz Home</a></div>
          <h3>mdzWebRequest Request Details:</h3>
          <xsl:apply-templates mode="Request" select="mdzWebRequest"/>
        </body>
      </xsl:otherwise>
    </xsl:choose>
  </html>
</xsl:template>


<xsl:template mode="Request" match="mdzWebRequest">
  <form action="mdzWebRequest_Test.aspx" method="post">
    <div class="UserInput">
      <div>
        <div>Configured Tests</div>
        <div><select id="ConfiguredTest" name="ConfiguredTest" onchange="ChangeTest();"><option selected="selected" value=""></option></select></div>
      </div>
      <div class="gap">
        <div>URL</div>
        <div><input type="text" id="URL" name="URL" value="{URL}"/></div>
      </div>
      <div>
        <div>Method</div>
        <div>
          <input type="text" list="Methods" id="Method" name="Method" value="{Method}" onchange="UpdateDisplay();"/>
          <datalist id="Methods">
            <option value="GET"/>
            <option value="POST"/>
            <option value="PUT"/>
          </datalist>
        </div>
      </div>
      <div class="ForContent">
        <div>Content</div>
        <div><input type="text" id="Content" name="Content" value="{Content}"/></div>
      </div>
      <div class="ForContent">
        <div>ContentType</div>
        <div>
          <input type="text" list="ContentTypes" id="ContentType" name="ContentType" value="{ContentType}"/>
          <datalist id="ContentTypes">
            <option value="application/x-www-form-urlencoded"/>
            <option value="text/xml; encoding='utf-8'"/>
          </datalist>
        </div>
      </div>
      <div class="gap">
        <div>UserName</div>
        <div><input type="text" id="UserName" name="UserName" value="{UserName}"/></div>
      </div>
      <div>
        <div>Password</div>
        <div><input type="text" id="Password" name="Password" value="{Password}"/></div>
      </div>
      <div class="gap">
        <div>ExpectedFormat</div>
        <div>
          <input type="text" list="ExpectedFormats" name="ExpectedFormat" value="{ExpectedFormat}"/>
          <datalist id="ExpectedFormats">
            <option value="Detect"/>
            <option value="Text"/>
            <option value="Binary"/>
          </datalist>
        </div>
      </div>
      <div>
        <div>MaxBinarySize</div>
        <div><input type="text" name="MaxBinarySize" value="{MaxBinarySize}"/></div>
      </div>
      <xsl:choose>
        <xsl:when test="/root/SelectProxy = 'True'">
          <div class="gap">
            <div>Use Proxy</div>
            <div>
              <input type="checkbox" id="UseProxy" name="UseProxy" value="True" onchange="UpdateDisplay();">
                <xsl:if test="UseProxy = 'True'"><xsl:attribute name="checked">checked</xsl:attribute></xsl:if>
              </input>
            </div>
          </div>
          <div class="ForProxy">
            <div>ProxyURL</div>
            <div><input type="text" name="ProxyURL" value="{ProxyURL}"/></div>
          </div>
          <div class="ForProxy">
            <div>ProxyUserName</div>
            <div><input type="text" name="ProxyUserName" value="{ProxyUserName}"/></div>
          </div>
          <div class="ForProxy">
            <div>ProxyPassword</div>
            <div><input type="text" name="ProxyPassword" value="{ProxyPassword}"/></div>
          </div>
        </xsl:when>
        <xsl:otherwise>
          <div class="gap">
            <div>Proxy</div>
            <div>For security purposes, the Proxy is always used and will only allow 2 requests per 10 seconds across all users.</div>
          </div>
        </xsl:otherwise>
      </xsl:choose>
      <div class="gap">
        <div>Process As</div>
        <div>
          <select id="ProcessAs" name="ProcessAs" data-value="{ProcessAs}">
            <option value="auto">Auto Detect</option>
            <option value="text">Text</option>
            <option value="json">JSON</option>
            <option value="xml">XML</option>
            <option value="webpage">Web Page</option>
            <option value="binary">Binary</option>
          </select><br/>
        </div>
      </div>
      <div class="gap"><div></div><div><input type="submit" name="Action" value="Submit"/></div></div>
    </div>
    </form>
    <p>
      The 'Process As' option selects how this test will process the response.<br/>
      The table below shows how responses are processed given the returned ResponseType if Auto Detect is selected.<br/>
      For Images, Auto Detect must be selected so that the image format is identified.
    </p>
    <table>
      <thead>
        <tr>
          <td>Process As</td>
          <td>Auto Detect<br/>ContentType</td>
          <td>Decription</td>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td>Text</td>
          <td>text/*</td>
          <td>The response is displayed as-is.</td>
        </tr>
        <tr>
          <td>JSON</td>
          <td>application/json<br/>text/json</td>
          <td>A collapsable tree presentation of the received json is made.</td>
        </tr>
        <tr>
          <td>XML</td>
          <td>application/xml<br/>text/xml</td>
          <td>A collapsable tree presentation of the received xml is made.</td>
        </tr>
        <tr>
          <td>Image</td>
          <td>image/*</td>
          <td>An img element is created and the response encoded to Base64 and set on its src attribute.</td>
        </tr>
        <tr>
          <td>Web&#160;Page</td>
          <td>text/html</td>
          <td>An iframe element is created and the response used as its HTML.<br/>References to images, css, js files (etc) will not load unless their src URL's are fully qualified.</td>
        </tr>
        <tr>
          <td>Binary</td>
          <td>Any other</td>
          <td>All requests present a hex dump of the response at the bottom of the page.  When processed as Binary, only the hex dump is presented.</td>
        </tr>
      </tbody>
    </table>
</xsl:template>


<xsl:template mode="Response" match="mdzWebRequest">
  <div class="Properties">
    <div>
      <div class="Property"><div>URL</div><div><xsl:value-of select="URL"/>&#160;</div></div>
      <div class="Property"><div>Method</div><div><xsl:value-of select="Method"/>&#160;</div></div>
      <div class="Property"><div>Content</div><div><xsl:value-of select="Content"/>&#160;</div></div>
      <div class="Property"><div>ContentType</div><div><xsl:value-of select="ContentType"/>&#160;</div></div>
      <div class="Property"><div>UserName</div><div><xsl:value-of select="UserName"/>&#160;</div></div>
      <div class="Property"><div>Password</div><div><xsl:value-of select="Password"/>&#160;</div></div>
      <div class="Property"><div>ExpectedFormat</div><div><xsl:value-of select="ExpectedFormat"/>&#160;</div></div>
      <div class="Property"><div>MaxBinarySize</div><div><xsl:value-of select="MaxBinarySize"/>&#160;</div></div>
      <div class="Property"><div>UseProxy</div><div><xsl:value-of select="UseProxy"/>&#160;</div></div>
      <div class="Property"><div>ProxyURL</div><div><xsl:value-of select="ProxyURL"/>&#160;</div></div>
      <div class="Property"><div>ProxyUserName</div><div><xsl:value-of select="ProxyUserName"/>&#160;</div></div>
      <div class="Property"><div>ProxyPassword</div><div><xsl:value-of select="ProxyPassword"/>&#160;</div></div>
      <hr/>
      <div class="Property"><div>ErrNo</div><div><xsl:value-of select="ErrNo"/>&#160;</div></div>
      <div class="Property"><div>ErrMsg</div><div><xsl:value-of select="ErrMsg"/>&#160;</div></div>
      <div class="Property"><div>ResponseCode</div><div><xsl:value-of select="ResponseCode"/>&#160;</div></div>
      <div class="Property"><div>ResponseType</div><div><xsl:value-of select="ResponseType"/>&#160;</div></div>
      <div class="Property"><div>ResponseTypeParams</div><div><xsl:value-of select="ResponseTypeParams"/>&#160;</div></div>
      <div class="Property"><div>IsBinary</div><div><xsl:value-of select="IsBinary"/>&#160;</div></div>
    </div>
  </div>
  <p>Response :</p>
  <div id="Container"></div>
  <p>Response Binary:</p>
  <pre id="BinaryContainer"></pre>
</xsl:template>


<xsl:include href="mdz_Include.xslt"/>


</xsl:stylesheet> 
