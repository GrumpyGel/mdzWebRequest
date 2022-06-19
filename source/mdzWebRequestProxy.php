<?php
$Url = GetPostData('URL');
$Method = GetPostData('Method');
$Content = GetPostData('Content');
$ContentType = GetPostData('ContentType');
$UserName = GetPostData('UserName');
$Password = GetPostData('Password');

$ErrNo = 0;
$ErrMsg = "";
$ResponseCode = 0;
$ResponseType = "";
$Response = "";

$ch = curl_init($Url);

curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
curl_setopt ($ch, CURLOPT_SSLVERSION, 6);  //Force requsts to use TLS 1.2

curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false);
curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);

if ($Method == "POST") {
  curl_setopt($ch, CURLOPT_POST, 1);
  if ($Content != "") {
    curl_setopt($ch, CURLOPT_POSTFIELDS, $Content);
    if ($ContentType != "") {
      curl_setopt ($ch, CURLOPT_HTTPHEADER, Array("Content-Type: " . $ContentType)); } } }

$result = curl_exec($ch);

if ($result === false) {
  $ErrNo = curl_errno($ch);
  $ErrMsg = curl_error($ch); }
else {
  $Response = $result;
  $ResponseCode = curl_getinfo($ch, CURLINFO_RESPONSE_CODE);
  $ResponseType = curl_getinfo($ch, CURLINFO_CONTENT_TYPE); }

$Response = base64_encode($Response); 
$Xml = new DOMDocument();
$Root = $Xml->createElement('root');
$Root = $Xml->appendChild($Root);
XmlAddElement($Root, 'ErrNo', $ErrNo);
XmlAddElement($Root, 'ErrMsg', $ErrMsg);
XmlAddElement($Root, 'ResponseCode', $ResponseCode);
XmlAddElement($Root, 'ResponseType', $ResponseType);
XmlAddElement($Root, 'Response', $Response);

echo $Xml->saveXml();


function GetPostData($Name) {
  $Value = "";
  if (isset($_POST[$Name])) {
      $Value = $_POST[$Name]; }
  else {
      if (isset($_GET[$Name])) {
          $Value = $_GET[$Name]; } }
  return $Value;
}

function XmlAddElement($Parent, $Name, $Value) {
  $Ele = $Parent->ownerDocument->createElement($Name);
  $Ele->nodeValue = $Value;
  $Parent->appendChild($Ele);
}

?>