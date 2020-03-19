using System;
using System.IO;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.IO.Compression;

namespace ServerAPI.ViewModels
{
    public class Certificate
    {
        public X509Certificate2 Cert;

        public void LoadCertificate(string certificate)
        {
            Cert = new X509Certificate2(StringToByteArray(certificate));
        }

        public void LoadCertificate(byte[] certificate)
        {
            Cert = new X509Certificate2();
            Cert.Import(certificate);
        }

        private static byte[] StringToByteArray(string st)
        {
            byte[] bytes = new byte[st.Length];
            for (int i = 0; i < st.Length; i++)
            {
                bytes[i] = (byte)st[i];
            }

            return bytes;
        }
    }

    public class Response
    {
        private XmlDocument _xmlDoc;
        private readonly Certificate _certificate;

        public Response(AccountSettings accountSettings)
        {
            _certificate = new Certificate();
            _certificate.LoadCertificate(accountSettings.Certificate);
        }

        public void LoadXml(string xml)
        {
            _xmlDoc = new XmlDocument
            {
                PreserveWhitespace = true,
                XmlResolver = null
            };
            _xmlDoc.LoadXml(xml);
        }

        public void LoadXmlFromBase64(string response)
        {
            Encoding enc = new ASCIIEncoding();
            LoadXml(enc.GetString(Convert.FromBase64String(response)));
        }

        public bool IsValid()
        {
            bool status = true;

            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            XmlNodeList nodeList = _xmlDoc.SelectNodes("//ds:Signature", manager);

            SignedXml signedXml = new SignedXml(_xmlDoc);
            signedXml.LoadXml((XmlElement)nodeList[0]);

            status &= signedXml.CheckSignature(_certificate.Cert, true);

            DateTime? notBefore = NotBefore();
            status &= !notBefore.HasValue || (notBefore <= DateTime.Now);

            DateTime? notOnOrAfter = NotOnOrAfter();
            status &= !notOnOrAfter.HasValue || (notOnOrAfter > DateTime.Now);

            return status;
        }

        public DateTime? NotBefore()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            XmlNodeList nodes = _xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:Conditions", manager);
            string value = null;
            if (nodes != null && nodes.Count > 0 && nodes[0] != null && nodes[0].Attributes != null &&
                nodes[0].Attributes["NotBefore"] != null)
            {
                value = nodes[0].Attributes["NotBefore"].Value;
            }

            return value != null ? DateTime.Parse(value) : (DateTime?)null;
        }

        public DateTime? NotOnOrAfter()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            XmlNodeList nodes = _xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:Conditions", manager);
            string value = null;
            if (nodes != null && nodes.Count > 0 && nodes[0] != null && nodes[0].Attributes != null &&
                nodes[0].Attributes["NotOnOrAfter"] != null)
            {
                value = nodes[0].Attributes["NotOnOrAfter"].Value;
            }

            return value != null ? DateTime.Parse(value) : (DateTime?)null;
        }

        public string GetAttributeValue(string attributeValue)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            XmlNode node =
                _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/" + attributeValue, manager);
            if (node != null)
            {
                return node.InnerText;
            }
            return "";
        }

        public string GetSessionIndex()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            XmlNode node =
                _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AuthnStatement", manager);
            return node.Attributes["SessionIndex"].Value;
        }
    }

    public class AccountSettings
    {
        public string Certificate { get; }
        public string IdpSsoTargetUrl { get; }
        public string IdpSsoLogoutUrl { get; }

        public AccountSettings(string cer, string idp)
        {
            Certificate = cer;
            IdpSsoTargetUrl = idp;
        }

        public AccountSettings(string cer, string idp, string idpLogout)
        {
            Certificate = cer;
            IdpSsoTargetUrl = idp;
            IdpSsoLogoutUrl = idpLogout;
        }
    }

    public class AppSettings
    {
        public AppSettings(string assertionUrl, string issuer)
        {
            AssertionConsumerServiceUrl = assertionUrl;
            Issuer = issuer;
        }

        public string AssertionConsumerServiceUrl;
        public string Issuer = "https://www.atims.com";
    }

    public class AuthRequest
    {
        public string Id;
        private readonly string _issueInstant;
        private readonly AppSettings _appSettings;
        private readonly AccountSettings _accountSettings;

        public enum AuthRequestFormat
        {
            Base64 = 1
        }

        public AuthRequest(AppSettings appSettings, AccountSettings accountSettings)
        {
            _appSettings = appSettings;
            _accountSettings = accountSettings;

            Id = "_" + Guid.NewGuid().ToString();
            _issueInstant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public string GetRequest(AuthRequestFormat format)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings { OmitXmlDeclaration = true };

                using (XmlWriter xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("saml2p", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("AssertionConsumerServiceURL", _appSettings.AssertionConsumerServiceUrl);
                    xw.WriteAttributeString("Destination", _accountSettings.IdpSsoTargetUrl);
                    xw.WriteAttributeString("ForceAuthn", "false");
                    xw.WriteAttributeString("ID", Id);
                    xw.WriteAttributeString("IsPassive", "false");
                    xw.WriteAttributeString("IssueInstant", _issueInstant);
                    xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
                    xw.WriteAttributeString("ProviderName", _appSettings.Issuer);
                    xw.WriteAttributeString("Version", "2.0");

                    xw.WriteStartElement("saml2", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString(_appSettings.Issuer);
                    xw.WriteEndElement();

                    xw.WriteStartElement("saml2p", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("AllowCreate", "true");
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
                    xw.WriteEndElement();

                    xw.WriteStartElement("saml2p", "RequestedAuthnContext", "urn:oasis:names:tc:SAML:2.0:protocol");


                    xw.WriteStartElement("saml2", "AuthnContextClassRef", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString("urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport");
                    xw.WriteEndElement();

                    xw.WriteEndElement();

                    xw.WriteEndElement();
                }

                if (format == AuthRequestFormat.Base64)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(sw.ToString());
                    using (MemoryStream output = new MemoryStream())
                    {
                        using (DeflateStream zip = new DeflateStream(output, CompressionMode.Compress))
                        {
                            zip.Write(bytes, 0, bytes.Length);
                        }
                        string base64 = Convert.ToBase64String(output.ToArray());
                        return base64;
                    }
                }

                return null;
            }
        }

        public string LogoutRequest(AuthRequestFormat format, string sessionIndex)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings { OmitXmlDeclaration = true };

                using (XmlWriter xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("saml2p", "LogoutRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Destination", _accountSettings.IdpSsoLogoutUrl);
                    xw.WriteAttributeString("ID", Id);
                    xw.WriteAttributeString("IssueInstant", _issueInstant);
                    xw.WriteAttributeString("Version", "2.0");

                    xw.WriteStartElement("saml2", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString(_appSettings.Issuer);
                    xw.WriteEndElement();

                    xw.WriteStartElement("saml2", "NameId", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress");
                    xw.WriteString("email goes here"); //email missing
                    xw.WriteEndElement();

                    xw.WriteStartElement("saml2p", "SessionIndex");
                    xw.WriteString(sessionIndex);
                    xw.WriteEndElement();

                    xw.WriteEndElement();
                }

                if (format == AuthRequestFormat.Base64)
                {
                    byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(sw.ToString());
                    return Convert.ToBase64String(toEncodeAsBytes);
                }
            }

            return null;
        }
    }
}
