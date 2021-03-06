using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MyTcpClient
{
    // Need class with TlsClient in inheritance chain
//     class MyTlsClient : DefaultTlsClient
//     {
//         public override TlsAuthentication GetAuthentication()
//         {
//             return new MyTlsAuthentication();
//         }
//     }
// 
//     // Need class to handle certificate auth
//     class MyTlsAuthentication : TlsAuthentication
//     {
//         public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
//         {
//             // return client certificate
//             return null;
//         }
// 
//         public void NotifyServerCertificate(Certificate serverCertificate)
//         {
//             // validate server certificate
//         }
//     }

    public class Class1
    {
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        public static bool TryGetCookies(Uri url, out string[] cookies)
        {
            TcpClient client = new TcpClient(url.Host, url.Port);
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
                );

            try
            {
                sslStream.AuthenticateAsClient(url.Host);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
            }
            Console.WriteLine("Authentication Success.");

            var sr = new SecureRandom();
//             TlsClientProtocol handler = new TlsClientProtocol(client.GetStream(), sr);
//             handler.Connect(new MyTlsClient());
            var hdr = new StringBuilder();
            hdr.AppendLine(string.Format("GET {0} HTTP/1.1", url.PathAndQuery));
            hdr.AppendLine(string.Format("Host: {0}", url.Host));
            hdr.AppendLine("Content-Type: text/xml; charset=utf-8");
            hdr.AppendLine("Accept: */*");
            hdr.AppendLine("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36");
            hdr.AppendLine("Connection: keep-alive");
            hdr.AppendLine();

            Console.WriteLine(hdr.ToString());

            var dataToSend = Encoding.UTF8.GetBytes(hdr.ToString());
            // sr.NextBytes(dataToSend);

            //            var stream = handler.Stream;
            var stream = sslStream;
            stream.Write(dataToSend, 0, dataToSend.Length);
            stream.Flush();

            var array = new System.Collections.Generic.List<string>();
            int totalRead = 0;
            string response = "";
            byte[] buff = new byte[1000];
            do
            {
                totalRead = stream.Read(buff, 0, buff.Length);
                var sub = Encoding.UTF8.GetString(buff, 0, totalRead);
                response += sub;
            } while (totalRead == buff.Length);

            foreach (var myString in response.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if(myString.StartsWith("Set-Cookie: "))
                {
                    array.Add(myString.Replace("Set-Cookie: ", ""));
                }
            }

            cookies = new string[array.Count];
            int i = 0;
            foreach (var myString in array)
            {
                cookies[i] = myString;
            }
            return true;
        }
    }
}
