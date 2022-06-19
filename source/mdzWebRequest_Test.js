
var ConfiguredTests = [
  ['Tag',
      'URL',
      'Method',
      'Content',
      'ContentType',
      'UserName',
      'Password',
      'ProcessAs'],
  ['Get returning Text : MyDocz CSS file',
      'http://www.mydocz.com/mydocz.css',
      'GET',
      '', '', '', '', 'auto'],
  ['Get returning JSON : MyDocz Sudoku Scores Samle',
      'http://www.mydocz.com/SudokuScoresSample.json',
      'GET',
      '', '', '', '', 'auto'],
  ['Get returning XML : MyDocz Sudoku Boards',
      'http://www.mydocz.com/SudokuBoards.xml',
      'GET',
      '', '', '', '', 'auto'],
  ['Get returning Image : MyDocz Sudoku Image',
      'http://www.mydocz.com/images/SudokuScreen.png',
      'GET',
      '', '', '', '', 'auto'],
  ['Get returning Web Page : MyDocz Home Page',
      'http://www.mydocz.com/',
      'GET',
      '', '', '', '', 'auto'],
  ['Get returning Web Page : HowsMySsl.com SSL analysis',
      'https://www.howsmyssl.com/',
      'GET',
      '', '', '', '', 'auto'],
  ['Get returning Web Page : Abode Login',
      'https://www.smartbooking.co.nz/login.php',
      'GET',
      '', '', '', '', 'auto'],
  ['Get returning Json : Joke API Joke',
      'https://v2.jokeapi.dev/joke/Any?blacklistFlags=racist,sexist,explicit',
      'GET',
      '', '', '', '', 'auto'],
  ['Post returning Image : Image-Charts.com',
      'https://image-charts.com/chart',
      'POST',
      'chbr=20&chco=CFECF7%2C27c9c2&chd=a%3A10000%2C50000%2C60000%2C80000%2C40000%7C50000%2C60000%2C100000%2C40000%2C20000&chdl=N%7CN-1&chdlp=r&chl=10%7C50%7C60%7C80%7C40%7C50%7C60%7C100%7C40%7C20&chs=700x450&cht=bvs&chtt=Revenue%20per%20month&chxl=0%3A%7CJan%7CFev%7CMar%7CAvr%7CMay&chxs=1N%2AcUSD0sz%2A%2C000000%2C14&chxt=x%2Cy',
      'application/x-www-form-urlencoded',
      '', '', 'auto'],
  ['Post returning Json : iTunes Search API',
      'https://itunes.apple.com/search',
      'POST',
      'term=The+Beatles&country=US&media=music&entity=album&attribute=artistTerm&limit=50&lang=en_us',
      'application/x-www-form-urlencoded',
      '', '', 'json']
   ];

/*

Request Functions

*/

function InitRequest() {
  var i_ConfiguredTest = document.getElementById('ConfiguredTest');
  var i_Option = null;
  var i = 1;
  
  for (i = 1; i < ConfiguredTests.length; i++) {
    i_Option = document.createElement("option");
    i_Option.text = ConfiguredTests[i][0];
    i_Option.value = ConfiguredTests[i][1];
    i_ConfiguredTest.add(i_Option); }
  UpdateDisplay();
}


function UpdateDisplay() {
  if (document.getElementById('Method').value == 'GET') {
    document.body.classList.add('NoContent'); }
  else {
    document.body.classList.remove('NoContent'); }
  if (document.getElementById('UseProxy')) {
    if (document.getElementById('UseProxy').checked == true) {
      document.body.classList.remove('NoProxy'); }
    else {
      document.body.classList.add('NoProxy'); } }
}


function ChangeTest() {
  var i_Tag = document.getElementById('ConfiguredTest').value;
  var i = 0;

  for (i = 1; i < ConfiguredTests.length; i++) {
    if (ConfiguredTests[i][1] == i_Tag) {
      document.getElementById('URL').value = ConfiguredTests[i][1];
      document.getElementById('Method').value = ConfiguredTests[i][2];
      document.getElementById('Content').value = ConfiguredTests[i][3];
      document.getElementById('ContentType').value = ConfiguredTests[i][4];
      document.getElementById('UserName').value = ConfiguredTests[i][5];
      document.getElementById('Password').value = ConfiguredTests[i][6];
      document.getElementById('ProcessAs').value = ConfiguredTests[i][7]; } }
  UpdateDisplay();
}


/*

Response Functions

*/

function InitResponse() {
  var i_ProcessAs = ProcessAs;
  var i_Response = atob(ResponseBase64);
  var i_Container = document.getElementById("Container");
  
  if (i_ProcessAs == "auto") {
    if (ResponseType.startsWith("text/html") == true) {
      i_ProcessAs = "webpage"; }
    else if (ResponseType.startsWith("image/") == true) {
      i_ProcessAs = "image"; }
    else if (ResponseType.startsWith("text/xml") == true) {
      i_ProcessAs = "xml"; }
    else if (ResponseType.startsWith("application/xml") == true) {
      i_ProcessAs = "xml"; }
    else if (ResponseType.startsWith("text/json") == true) {
      i_ProcessAs = "json"; }
    else if (ResponseType.startsWith("application/json") == true) {
      i_ProcessAs = "json"; }
    else if (ResponseType.startsWith("text/") == true) {
      i_ProcessAs = "text"; }
    else {
      i_ProcessAs = "binary"; } }
  
  switch (i_ProcessAs) {
    case "webpage":  Show_Html(i_Response, i_Container); break;
    case "image":    Show_Image(i_Container); break;
    case "json":     Show_JSON(i_Response, i_Container); break;
    case "xml":      Show_XML(i_Response, i_Container); break;
    case "text":     Show_Text(i_Response, i_Container); break; }
  Show_Binary(i_Response);
}
  

function Show_Text(p_Response, p_Container) {
  var i_Pre = null;
  var i_Response = "";

  i_Pre = document.createElement("pre");
  p_Container.appendChild(i_Pre);
  i_Response = i_Response.replaceAll("&", "&amp;");
  i_Response = p_Response.replaceAll("<", "&lt;");
//i_Response = i_Response.replaceAll("\n", "<br/>");
  i_Pre.innerHTML = i_Response;
}


function Show_Html(p_Response, p_Container) {
  var i_Frame = null;

  i_Frame = document.createElement("iframe");
  i_Frame.style.width = "100%";
  i_Frame.style.height = "80vh";
  p_Container.appendChild(i_Frame);
  i_Frame.contentWindow.document.open();
  i_Frame.contentWindow.document.write(p_Response);
  i_Frame.contentWindow.document.close();
}


function Show_Image(p_Container) {
  var i_Img = null;
  var i_Format = "";

  i_Format = ResponseType;
  if (i_Format.indexOf(";") != -1) {
    i_Format = i_Format.substring(0, i_Format.indexOf(";")); }
  i_Img = document.createElement("img");
  p_Container.appendChild(i_Img);
  i_Img.src = "data:" + i_Format + ";charset=utf-8;base64, " + ResponseBase64;
}


function Show_JSON_3pty(p_Response, p_Container) {
  var i_JSonObj = JSON.parse(p_Response);

  p_Container.appendChild(renderjson(i_JSonObj));
}


function Show_JSON(p_Response, p_Container) {
  var i_JSon = JSON.parse(p_Response);

  Show_JSON_Element(i_JSon, p_Container, true);
}


function Show_JSON_Element(p_Json, p_Container, p_Open) {
  var i_Key = "";
  var i_IsNull = false;
  var i_IsObj = false;
  var i_Element =null;
  var i_Item = null;
  var i_Tag = null;
  var i_Text = null;

  for (i_Key in p_Json) {
    if (p_Json[i_Key] !== null) {
      i_IsNull = false; }
    else {
      i_IsNull = true; }
    if (typeof(p_Json[i_Key])=="object") {
      i_IsObj = true; }
    else {
      i_IsObj = false; }
    i_Element = document.createElement("div");
    i_Element.classList.add("xmlElement");
    i_Element.classList.add(((i_IsObj == false) ? "xmlLeaf" : ((p_Open == true) ? "xmlOpen" : "xmlClosed")));
    i_Item = document.createElement("div");
    i_Item.classList.add("xmlDetails");
    i_Nav = document.createElement("div");
    i_Nav.classList.add("xmlNav");
    if (i_IsObj == true) {
      i_Nav.addEventListener("click", XML_Toggle); }
    i_Item.appendChild(i_Nav);
    i_Tag = document.createElement("div");
    i_Tag.classList.add("xmlTag");
    i_Tag.appendChild(document.createTextNode(i_Key));
    i_Item.appendChild(i_Tag);
    i_Text = document.createElement("div");
    i_Text.classList.add("xmlText");
    if (i_IsNull == false && i_IsObj == false) {
      i_Text.appendChild(document.createTextNode(p_Json[i_Key])); }
    i_Item.appendChild(i_Text);
    i_Element.appendChild(i_Item);
    if (i_IsNull == false && i_IsObj == true) {
      Show_JSON_Element(p_Json[i_Key], i_Element, false); }
    p_Container.appendChild(i_Element); }
}


function Show_XML(p_Response, p_Container) {
  var i_Parser = new DOMParser();
  var i_Doc = i_Parser.parseFromString(p_Response, "application/xml");

  p_Container.appendChild(ShowXML_Element(i_Doc.documentElement, true));
}


function ShowXML_Element(p_Element, p_Open) {
  var i_Element = document.createElement("div");
  var i_Item = null;
  var i_Param = null;
  var i_Tag = null;
  var i_Text = null;
  var i_Params = null;
  var i = 0;

  i_Element.classList.add("xmlElement");
  if (p_Element.children.length == 0) {
    i_Element.classList.add("xmlLeaf"); }
  else {
    if (p_Open == true) {
      i_Element.classList.add("xmlOpen"); }
    else {
      i_Element.classList.add("xmlClosed"); } }
  i_Item = document.createElement("div");
  i_Item.classList.add("xmlDetails");
  i_Nav = document.createElement("div");
  i_Nav.classList.add("xmlNav");
  if (p_Element.children.length != 0) {
    i_Nav.addEventListener("click", XML_Toggle); }
  i_Item.appendChild(i_Nav);
  i_Tag = document.createElement("div");
  i_Tag.classList.add("xmlTag");
  i_Tag.appendChild(document.createTextNode(p_Element.tagName));
  i_Item.appendChild(i_Tag);
  i_Text = document.createElement("div");
  i_Text.classList.add("xmlText");
  i_Text.appendChild(document.createTextNode(ShowXML_Element_Text(p_Element)));
  i_Item.appendChild(i_Text);
  i_Element.appendChild(i_Item);
  if (p_Element.hasAttributes()) {
    i_Params = p_Element.attributes;
    for(i = 0; i < i_Params.length; i++) {
      i_Param = document.createElement("div");
      i_Param.classList.add("xmlAttribute");
      i_Nav = document.createElement("div");
      i_Nav.classList.add("xmlNav");
      i_Param.appendChild(i_Nav);
      i_Tag = document.createElement("div");
      i_Tag.classList.add("xmlTag");
      i_Tag.appendChild(document.createTextNode("@" + i_Params[i].name));
      i_Param.appendChild(i_Tag);
      i_Text = document.createElement("div");
      i_Text.classList.add("xmlText");
      i_Text.appendChild(document.createTextNode(i_Params[i].value));
      i_Param.appendChild(i_Text);
      i_Element.appendChild(i_Param); } }
  for(i = 0; i < p_Element.children.length; i++) {
    i_Element.appendChild(ShowXML_Element(p_Element.children[i], false)); }
  return i_Element;
}


function ShowXML_Element_Text(p_Element) {
  var i_Text = "";
  var i = 0;

  for (i = 0; i < p_Element.childNodes.length; i++) {
    if (p_Element.childNodes[i].nodeType == 3) {
      i_Text = i_Text + p_Element.childNodes[i].nodeValue; } }
  return i_Text;
}


function XML_Toggle(p_Event) {
  var i_Nav = p_Event.target;
  var i_Element = i_Nav.parentNode.parentNode;

  if (i_Element.classList.contains("xmlOpen") == true) {
    i_Element.classList.remove("xmlOpen");
    i_Element.classList.add("xmlClosed");
    return;  }
  if (i_Element.classList.contains("xmlClosed") == true) {
    i_Element.classList.remove("xmlClosed");
    i_Element.classList.add("xmlOpen"); }
}

/*
<div class="xmlElement">
  <div class="xmlDetails">
    <div class="xmlNav xmlOpen" onclick="XML_Toggle(this);"></div>
    <div class="xmlTag">Root</div>
    <div class="xmlText"></div>
  </div>
  <div class="xmlElement">
    <div class="xmlDetails">
      <div class="collapse open"></div>
      <div class="xmlTag">Child</div>
      <div class="xmlText">My Name</div>
    </div>
    <div class="xmlAttribute"><div class="xmlTag">age</div><div class="xmlText">35</div></div>
  </div>
</div>
*/


function Show_Binary(p_Response) {
  var i_BinaryContainer = document.getElementById("BinaryContainer");
  var i_Html = "";
  var i_HtmlChars = "";
  var i_HtmlCodes = "";
  var i_LinePos = 0;
  var i_CharIdx = 0;
  var i_CharPos = 0;
  var i_Code = 0;
  var i_NumLenTest = "[" + p_Response.length + "]";
  var i_NumLen = 0 - (i_NumLenTest.length - 2);

  while (i_LinePos < p_Response.length) {
    i_HtmlChars = "";
    i_HtmlCodes = "";
    for (i_CharIdx = 0; i_CharIdx < 32; i_CharIdx++) {
      i_CharPos = i_LinePos + i_CharIdx;
      if (i_CharPos < p_Response.length) {
        i_Code = p_Response.charCodeAt(i_CharPos);
        if (i_Code == 60) {
          i_HtmlChars = i_HtmlChars + "&#60;"; }
        else if (i_Code == 38) {
          i_HtmlChars = i_HtmlChars + "&#38;"; }
        else if (i_Code > 31 && i_Code < 128) {
          i_HtmlChars = i_HtmlChars + p_Response.charAt(i_CharPos); }
        else {
          i_HtmlChars = i_HtmlChars + "."; }
        if (i_Code < 256) {
          i_HtmlCodes = i_HtmlCodes + ('00' + i_Code.toString(16).toUpperCase()).slice(-2) + " "; }
        else {
          i_HtmlCodes = i_HtmlCodes + "** "; } }
      else {
        i_HtmlChars = i_HtmlChars + " "; } }
    i_Html = i_Html + ('                ' + i_LinePos).slice(i_NumLen) + " : " + i_HtmlChars + " : " + i_HtmlCodes + "<br/>";
    i_LinePos = i_LinePos + 32; }
    i_BinaryContainer.innerHTML = i_Html;
}


