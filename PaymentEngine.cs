using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OpenEdgeHPF
{
    internal class PaymentEngine
    {
        public enum CallType
        {
            otk,
            result
        };

        public enum TranType
        {
            CreditSale,
            DebitSale,
            CreditReturn,
            DebitReturn,
            AliasCreate,
            ResultsCall,
            CheckAlias,
            CreditEMV,
            CreditEMVReturn,
            CheckSale,
            CheckCredit
        };

        public static string gatewayURL = "https://test.t3secure.net/x-chargeweb.dll";
        public static string hpfURL = "https://integrator.t3secure.net/hpf/hpf.aspx";
        internal static string otk = null;


        public static string generateRequest(CallType reason, TranType type, string amount)
        {
            string request;

            //Create StringBuilder for OTK request to gateway
            StringBuilder sbRequest = new StringBuilder();

            //Create new instance of XML settings, set indent
            XmlWriterSettings ws = new XmlWriterSettings();
            ws.Indent = true;

            //Create OTK request, generates XML request
            using (XmlWriter xmlWriter = XmlWriter.Create(sbRequest, ws))
            {
                //Start XML writer
                xmlWriter.WriteStartDocument();
                //StartRoot element
                xmlWriter.WriteStartElement("GatewayRequest");


                //Start XWeb credentials
                xmlWriter.WriteStartElement("XWebID");
                xmlWriter.WriteString("800000001355");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("TerminalID");
                xmlWriter.WriteString("80023063");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("AuthKey");
                xmlWriter.WriteString("h0sREUeuwYMHrGaCnlM6AhpxNy232dGz");
                xmlWriter.WriteEndElement();
                //End XWeb credentials

                xmlWriter.WriteStartElement("Industry");
                xmlWriter.WriteString("RETAIL");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("DeviceType");
                xmlWriter.WriteString("EMV");
                xmlWriter.WriteEndElement();


                xmlWriter.WriteStartElement("SpecVersion");
                xmlWriter.WriteString("XWebSecure3.6");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("Amount");
                xmlWriter.WriteString(amount);
                xmlWriter.WriteEndElement();

                //Transaction Type Writer

                xmlWriter.WriteStartElement("TransactionType");
                xmlWriter.WriteString("CreditSaleTransaction");
                xmlWriter.WriteEndElement();

                //Results call Writer
                if (type == TranType.ResultsCall)
                {
                    xmlWriter.WriteStartElement("XWebID");
                    xmlWriter.WriteString("800000001355");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("TerminalID");
                    xmlWriter.WriteString("80023063");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("AuthKey");
                    xmlWriter.WriteString("h0sREUeuwYMHrGaCnlM6AhpxNy232dGz");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("OTK");
                    xmlWriter.WriteString(otk);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("ResponseMode");
                    xmlWriter.WriteString("POLL");
                    xmlWriter.WriteEndElement();
                }

                // Else if for not
                else
                {

                }
                //End root element
                xmlWriter.WriteEndElement();
                //End XML writer
                xmlWriter.WriteEndDocument();
            }
            request = sbRequest.ToString();

            return request;
        }

        public static string callGateway(string request)
        {
            string response;

            try
            {
                //Create connection to XWeb test gateway
                HttpWebRequest tokRequest = (HttpWebRequest) WebRequest.Create(gatewayURL);

                //Connection settings
                tokRequest.KeepAlive = false;
                tokRequest.Timeout = 60000;
                tokRequest.Method = "POST";

                //Connection settings
                byte[] tokByteArray = Encoding.ASCII.GetBytes(request);
                tokRequest.ContentType = "application/x-www-form-urlencoded";
                tokRequest.ContentLength = tokByteArray.Length;

                //Create OTK request stream to gateway
                Stream tokDataStream = tokRequest.GetRequestStream();

                //Stream settings 
                tokDataStream.Write(tokByteArray, 0, tokByteArray.Length);
                tokDataStream.Close();

                //Create and save connection response
                WebResponse tokResponse = tokRequest.GetResponse();

                tokDataStream = tokResponse.GetResponseStream();
                StreamReader tokReader = new StreamReader(tokDataStream);


                //Read and save connection response
                response = tokReader.ReadToEnd();

                //Close connections
                tokReader.Close();
                tokDataStream.Close();
                tokResponse.Close();


                return response;
            }
            catch (Exception EX)
            {
                return response = EX.Message;
            }
        }

        public static string parseXML(string toParse, CallType reason)
        {
            string parsedData = null;

            //Parse gateway reponse for OTK
            using (XmlTextReader xmlReader = new XmlTextReader(new StringReader(toParse)))
            {
                while (xmlReader.Read())
                {
                    while (xmlReader.ReadToFollowing("ResponseCode"))
                    {
                        string responseCode = xmlReader.ReadElementContentAsString();

                        if (reason == CallType.otk)
                        {
                            if (responseCode == "100")
                            {
                                while (xmlReader.ReadToFollowing("OTK"))
                                {
                                    parsedData = xmlReader.ReadElementContentAsString();
                                }
                            }
                        }
                        else
                        {
                            parsedData = responseCode;
                        }
                    }
                }

                xmlReader.Close();

                return parsedData;
            }
        }     

    }
}
