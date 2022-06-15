# mdzWebRequest
Wrapper for httpWebRequest allowing modern ciphers


[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]



<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/GrumpyGel/mdzWebRequest">
    <img src="source/images/SudokuScreen_2.png" alt="Logo" width="180">
  </a>

  <h3 align="center">mdzWebRequest</h3>

  <p align="center">
    Wrapper for httpWebRequest allowing modern ciphers
    <br />
    <br />
    <br />
    <a href="http://www.mydocz.com/mdzWebRequest_Test.aspx">View Demo</a>
    ·
    <a href="https://github.com/GrumpyGel/mdzWebRequest_Test/issues">Report Bug</a>
    ·
    <a href="https://github.com/GrumpyGel/mdzWebRequest_Test/issues">Request Feature</a>
  </p>
</p>



<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary><h2 style="display: inline-block">Table of Contents</h2></summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#installation--usage">Installation &amp; Usage</a></li>
    <li><a href="#documentation">Documentation</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgements">Acknowledgements</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

A 3rd Party that a site I manage integrates to updated their SSL connectivity and in doing so restricted the number of allowable ciphers for clients to connect with to 3.

The managed site operates in a .Net environment and its httpWebRequest classes use the underlying operating system's https commmunication facilities. The 3 ciphers were not supported on anything other than Windows Server 2022 and we were not in a position to migrate to this platform.

I therefore investigated placing another server in our network to operate as a 'proxy' for our calls. The 3rd party uses Linux and PHP, so this was a natural choice.

However, we have PHP installed on our Windows Servers, so investigated if curl within PHP would use these ciphers in a Windows environment.

They did indeed make use of these ciphers, so I have put together a mdzWebRequest package of PHP proxy and .Net class to wrap the .Net httpWebRequest class to optionally make use of the proxy.

<!-- GETTING STARTED -->

## Installation & Usage

Clone the repo
   ```sh
   git clone https://github.com/GrumpyGel/mdzWebRequest.git
   ```





<!-- DOCUMENTATION -->
## Documentation

To make a web request using mdzWebRequest perform the following:

<ol>
<li>Create an instance of mdzWebRequest, for example MyRequest = new mdzWebRequest();</li>
<li>Set the Request Properties</li>
<li>Envoke the Submit() Method</li>
<li>Check the Response Properties</li>
</ol>

### Request Properties

| Property | DataType | Description |
| --- | --- | --- |
| URL | string | The URL for the service you wish to request |
| Method | string | The request method, eg GET, POST, PUT. |
| Content | string | Any data to post |
| ContentType | string | Mime type for data to be posted, fopr example "application/x-www-form-urlencoded", "text/xml; encoding='utf-8'" |
| UserName | string | If authentification is required, the UserName |
| Password | string | If authentification is required, the Password |
| ExpectedFormat | string | The response can be returned as a string or binary (btye[]), see below for options |
| MaxBinarySize | int | If the response is Binary, this is the maximum allowable size |
| UseProxy | bool | If false, the request will be made using a httpWebRequest object, if True the request will be made via the mdzWebRequest_Proxy.php |
| ProxyURL | string | URL to access to proxy. |
| ProxyUserName | string | If authentification is required to access the proxy, the UserName |
| ProxyPassword | string | If authentification is required to access the proxy, the Password |

#### Expectedformat

The ExpectedFormat property may be set to one of the following:

| Value | Description |
| --- | --- |
| Text | If the response will be returned in the Response property as a string, onlt use when safe to do so |
| Binary | If the response will be returned in the ResponseBinary property as a byte[] |
| Detect | ResponseBinary property as a byte[] by default, but will be returned in the Response property as a string if the ResponseType is one of the following: "text/*", "application/xhtml+xml", "application/xml" or "application/json" |

### Response Properties

| Property | DataType | Description |
| --- | --- | --- |
| ErrNo | int | An error code that may be from mdzWebTRequest or culr if using the proxy, see below |
| ErrMsg | string | An error code that may be from mdzWebTRequest or culr if using the proxy, see below |
| ResponseCode | HttpStatusCode | The response status code returned by the server - see [https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view](https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode) |
| ResponseType | string | The response Mine Content Type.  Any parameters following the Content type in the header supplied by the server are stripped, for example "text/html; charset=utf-8" will return just "text/html" |
| ResponseTypeParams | string | Any parameters following the Content Type, for example "text/html; charset=utf-8" will return "charset=utf-8" |
| IsBinary | bool | If True, the response has been treated as Binary and is therefore provided in the ResponseBinary property.  If False the response is treated as Text and is provided in the Response property |
| Response | string | The response data returned by the server, if the IsBinary property is False |
| ResponseBinary | byte[] | The response data returned by the server, if the IsBinary property is True |





<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE` for more information.



<!-- CONTACT -->
## Contact

Email - [grumpygel@mydocz.com](mailto:grumpygel@mydocz.com)

Project Link: [https://github.com/GrumpyGel/mdzWebRequest](https://github.com/GrumpyGel/mdzWebRequest)



<!-- ACKNOWLEDGEMENTS -->
## Acknowledgements

* [Best-README-Template](https://github.com/othneildrew/Best-README-Template)






<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/GrumpyGel/mdzWebRequest.svg?style=for-the-badge
[contributors-url]: https://github.com/GrumpyGel/mdzWebRequest/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/GrumpyGel/mdzWebRequest.svg?style=for-the-badge
[forks-url]: https://github.com/GrumpyGel/mdzWebRequest/network/members
[stars-shield]: https://img.shields.io/github/stars/GrumpyGel/mdzWebRequest.svg?style=for-the-badge
[stars-url]: https://github.com/GrumpyGel/mdzWebRequest/stargazers
[issues-shield]: https://img.shields.io/github/issues/GrumpyGel/mdzWebRequest.svg?style=for-the-badge
[issues-url]: https://github.com/GrumpyGel/mdzWebRequest/issues
[license-shield]: https://img.shields.io/github/license/GrumpyGel/mdzWebRequest.svg?style=for-the-badge
[license-url]: https://github.com/GrumpyGel/mdzWebRequest/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/gerald-moull-41b5265
