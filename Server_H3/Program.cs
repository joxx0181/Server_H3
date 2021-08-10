using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Server_H3
{
    // This class represents my first WebServer!
    public class WebServer
    {
        // Create a HttpListener object!
        private readonly HttpListener myListener = new HttpListener();

        // Func is a built-in generic delegate type, which in this case take an incoming HTTP request and return a string value!
        private readonly Func<HttpListenerRequest, string> myResponse;

        public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
        {
            // Using IsSupported property to detect, whether an HttpListener object can be used with the current operating system!
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("OBS OBS: Windows XP, Server 2003 or later are required");
            }

            // URI (Uniform Resource Identifier) prefix string is composed of a scheme (http or https), a host, an optional port, an optional path and end in a forward slash ("/")!
            // In this case a complete prefix string is http://localhost:8080/Jo-AnnaServer/
            if (prefixes == null || prefixes.Count == 0)
            {
                throw new ArgumentException("OBS OBS: prefixes are required");
            }

            if (method == null)
            {
                throw new ArgumentException("The responder method are required");
            }

            // Add the prefixes!
            foreach (string myPrefix in prefixes)
            {
                myListener.Prefixes.Add(myPrefix);
            }

            myResponse = method;

            // Start listening for client requests!
            myListener.Start();
        }

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes) : this(prefixes, method)
        {
        }

        // Method to get the server running!
        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Console.WriteLine("\n\nThis Webserver is running...\nEnter this URI prefix: http://localhost:8080/Jo-AnnaServer/ ");

                // The thread runs the while loop to continuously listen for http requests!
                while (myListener.IsListening)
                    {
                         
                        ThreadPool.QueueUserWorkItem( myContext =>
                        {
                           HttpListenerContext ctx = myContext as HttpListenerContext;
 
                                if (ctx == null)
                                {
                                    return;
                                }

                                // Get a response stream and write the response to it!
                                string rstr = myResponse(ctx.Request);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
 
   
                                // Close the response stream!
                                if (ctx != null)
                                {
                                    ctx.Response.OutputStream.Close();
                                }
                            
                        }, myListener.GetContext());
                    }
            });
        }

        // Method to stop and close the server!
        public void Stop()
        {
            myListener.Stop();
            myListener.Close();
        }
    }

    internal class Program
    {
        public static string SendResponse(HttpListenerRequest request)
        {
            // Construct a response!
            return string.Format("<HTML><BODY style=\"background: linear-gradient(45deg, red, gold, lightgreen, gold, red); font-size: 50px; font-weight: bold; text-align: center;\">This is my welcome page.<br><br>You arrived at {0} <br></BODY></HTML>", DateTime.Now);

        }

        private static void Main(string[] args)
        {
            // Create a server and send response to client!
            WebServer joannaServer = new WebServer(SendResponse, "http://localhost:8080/Jo-AnnaServer/");

            // Call the Run method!
            joannaServer.Run();

            Console.WriteLine("\n\nWelcome to a simple webserver.\nHosted by Jo-Anna.\n\nPress a key to stop the server from running!!");
            Console.ReadKey();

            // Call the Stop method!
            joannaServer.Stop();
        }
    }
}
