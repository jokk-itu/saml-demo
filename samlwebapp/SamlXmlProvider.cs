using dk.nita.saml20.Schema.Core;
using dk.nita.saml20.Schema.Protocol;
using dk.nita.saml20.Utils;
using dk.nita.saml20;
using Saml2.Authentication.Core.Bindings;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Saml2.Authentication.Core.Providers;
using System.IO.Compression;

namespace samlwebapp;

public class SamlXmlProvider : ISamlXmlProvider
{
    public XmlDocument GetDecodedSamlResponse(Saml2Response saml2Response)
    {
        var doc = new XmlDocument
        {
            XmlResolver = null,
            PreserveWhitespace = true
        };
        var base64SamlResponse = saml2Response.Response;
        var compressedSamlResponse = Convert.FromBase64String(base64SamlResponse);
        var decompressedSamlResponse = Decompress(compressedSamlResponse).ToArray();
        var samlResponse = Encoding.UTF8.GetString(decompressedSamlResponse);
        doc.LoadXml(samlResponse);
        return doc;
    }

    public XmlElement GetAssertion(XmlElement xmlElement, AsymmetricAlgorithm privateKey)
    {
        if (IsEncrypted(xmlElement))
        {
            return GetDecryptedAssertion(xmlElement, privateKey);
        }

        var assertionList = xmlElement.GetElementsByTagName(Assertion.ELEMENT_NAME, Saml2Constants.ASSERTION);

        var assertion = (XmlElement)assertionList[0];
        if (assertion == null)
        {
            throw new Saml2Exception("Missing assertion");
        }

        return assertion;
    }

    public LogoutResponse GetLogoutResponse(string logoutResponseMessage)
    {
        var doc = new XmlDocument
        {
            XmlResolver = null,
            PreserveWhitespace = true
        };
        doc.LoadXml(logoutResponseMessage);

        var logoutResponse =
            (XmlElement)doc.GetElementsByTagName(LogoutResponse.ELEMENT_NAME, Saml2Constants.PROTOCOL)[0];
        return Serialization.DeserializeFromXmlString<LogoutResponse>(logoutResponse.OuterXml);
    }

    public XmlElement GetArtifactResponse(Stream stream)
    {
        var parser = new HttpArtifactBindingParser(stream);
        if (!parser.IsArtifactResponse())
        {
            return null;
        }

        var status = parser.ArtifactResponse.Status;
        if (status.StatusCode.Value != Saml2Constants.StatusCodes.Success)
        {
            throw new Exception($"Illegal status: {status.StatusCode} for ArtifactResponse");
        }

        return parser.ArtifactResponse.Any.LocalName != Response.ELEMENT_NAME ? null : parser.ArtifactResponse.Any;
    }

    public Status GetLogoutResponseStatus(string logoutResponseMessage)
    {
        var doc = new XmlDocument
        {
            XmlResolver = null,
            PreserveWhitespace = true
        };
        doc.LoadXml(logoutResponseMessage);

        var statElem = (XmlElement)doc.GetElementsByTagName(Status.ELEMENT_NAME, Saml2Constants.PROTOCOL)[0];

        return Serialization.DeserializeFromXmlString<Status>(statElem.OuterXml);
    }

    public XmlElement GetDecryptedAssertion(XmlElement xmlElement, AsymmetricAlgorithm privateKey)
    {
        var encryptedList =
            xmlElement.GetElementsByTagName(EncryptedAssertion.ELEMENT_NAME, Saml2Constants.ASSERTION);
        var assertion = (XmlElement)encryptedList[0];
        if (assertion == null)
        {
            throw new Saml2Exception("Missing assertion");
        }

        var encryptedAssertion = new Saml2EncryptedAssertion((RSA)privateKey);
        encryptedAssertion.LoadXml(assertion);
        encryptedAssertion.Decrypt();

        return encryptedAssertion.Assertion.DocumentElement;
    }

    private static MemoryStream Decompress(byte[] input)
    {
        var output = new MemoryStream();

        using (var compressStream = new MemoryStream(input))
        using (var decompressor = new DeflateStream(compressStream, CompressionMode.Decompress))
            decompressor.CopyTo(output);

        output.Position = 0;
        return output;
    }

    private static bool IsEncrypted(XmlElement element)
    {
        var encryptedList = element.GetElementsByTagName(EncryptedAssertion.ELEMENT_NAME, Saml2Constants.ASSERTION);
        return encryptedList.Count == 1;
    }
}
