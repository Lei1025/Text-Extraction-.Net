using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Web;
using System.Xml;
using System.Text;

namespace Savvypro
{
    public class OCR
    {
        public OCR(string ApplicationId, string Password, string FilePath)
        {
            this.ApplicationId = ApplicationId;
            this.Password = Password;
            this.FilePath = FilePath;

            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string toEncode = ApplicationId + ":" + Password;
            string baseEncoded = Convert.ToBase64String(encoding.GetBytes(toEncode));
            EncodedAuth = baseEncoded;
        }

        protected string EncodedAuth = "";

        /// <summary>
        /// User applicationId
        /// </summary>
        protected string ApplicationId { get; set; }
        /// <summary>
        /// User password
        /// </summary>
        protected string Password { get; set; }
        /// <summary>
        /// Virtual file path on web server
        /// </summary>
        protected string FilePath { get; set; }
        /// <summary>
        /// Network credentials
        /// </summary>
        protected ICredentials Credentials
        {
            get
            {
                if (_credentials == null)
                {
                    _credentials = string.IsNullOrEmpty(ApplicationId) || string.IsNullOrEmpty(Password)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(ApplicationId, Password);
                }
                return _credentials;
            }
        }
        /// <summary>
        /// Network proxy
        /// </summary>
        protected IWebProxy Proxy
        {
            get
            {
                if (_proxy == null)
                {
                    _proxy = WebRequest.DefaultWebProxy;
                    _proxy.Credentials = CredentialCache.DefaultCredentials;
                }
                return _proxy;
            }
        }

        /// <summary>
        /// Gets file processing result, specified by provided parameters, and returns it as downloadable resource
        /// </summary>
        /// <param name="applicationId">Application id</param>
        /// <param name="password">Password</param>
        /// <param name="fileName">Virtual file path on web server</param>
        /// <param name="language">Recognition language</param>
        /// <param name="exportFormat">Recognition export format</param>
        /// <remarks>Language and export formats specification can be obtained from "https://ocrsdk.com/documentation/apireference/processImage/"</remarks>
        public string GetResult(string filePath, string language, string exportFormat)
        {
            // Specifying new post request filling it with file content
            var url = string.Format("https://cloud-westus.ocrsdk.com/processImage?language={0}&exportFormat={1}", language, exportFormat);
            var localPath = HttpContext.Current.Server.MapPath(filePath);
            var request = CreateRequest(url, "POST", Credentials, Proxy, EncodedAuth);
            FillRequestWithContent(request, localPath);
            // Getting task id from response
            var response = GetResponse(request);
            var taskId = GetTaskId(response);
            // Checking if task is completed and downloading result by provided url
            url = string.Format("https://cloud-westus.ocrsdk.com/getTaskStatus?taskId={0}", taskId);
            var resultUrl = string.Empty;
            var status = string.Empty;
            while (status != "Completed")
            {
                System.Threading.Thread.Sleep(1000);
                request = CreateRequest(url, "GET", Credentials, Proxy, EncodedAuth);
                response = GetResponse(request);
                status = GetStatus(response);
                resultUrl = GetResultUrl(response);
            }
       
         request = (HttpWebRequest)WebRequest.Create(resultUrl);
            XmlDocument xml = new XmlDocument();
            String responseString;
            using (HttpWebResponse result = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = result.GetResponseStream())
                {
                    HttpContext.Current.Response.ContentType = "application/octet-stream";
                    HttpContext.Current.Response.Headers.Add("Content-Disposition", String.Format("attachment; filename=\"{0}\"", "result." + GetExtension(exportFormat)));
                    //var length = copyStream(stream, HttpContext.Current.Response.OutputStream);
                    //HttpContext.Current.Response.Headers.Add("Content-Length", length.ToString());
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    responseString = reader.ReadToEnd();
                }
            }

            responseString = responseString.Length == 0 ? "Sorry, no result for this images" : responseString;
            return responseString;         
        }
        /// <summary>
        /// Creates new request with defined parameters
        /// </summary>
        protected static HttpWebRequest CreateRequest(string url, string method, ICredentials credentials, IWebProxy proxy, string EncodedAuth)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/octet-stream";
            request.Credentials = credentials;
            request.Method = method;
            request.Proxy = proxy;

            //encode id and password into head
            request.Headers.Add("Authorization", "Basic " + EncodedAuth);

            return request;
        }
        /// <summary>
        /// Adds content from local file to request stream
        /// </summary>
        protected static void FillRequestWithContent(HttpWebRequest request, string contentPath)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(contentPath)))
            {
                request.ContentLength = reader.BaseStream.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    byte[] buffer = new byte[reader.BaseStream.Length];
                    while (true)
                    {
                        int bytesRead = reader.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                        stream.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
        /// <summary>
        /// Gets response xml document
        /// </summary>
        protected static XDocument GetResponse(HttpWebRequest request)
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    return XDocument.Load(new XmlTextReader(stream));
                }
            }
        }
        /// <summary>
        /// Gets file processing task id from response document
        /// </summary>
        protected static string GetTaskId(XDocument doc)
        {
            var id = string.Empty;
            var task = doc.Root.Element("task");
            if (task != null)
            {
                id = task.Attribute("id").Value;
            }
            return id;
        }
        /// <summary>
        /// Gets task's processing status from response document
        /// </summary>
        protected static string GetStatus(XDocument doc)
        {
            var status = string.Empty;
            var task = doc.Root.Element("task");
            if (task != null)
            {
                status = task.Attribute("status").Value;
            }
            return status;
        }
        /// <summary>
        /// Gets result url to download from response document
        /// </summary>
        /// <remarks> Result url will be available only after task status set to "Complete"</remarks>
        protected static string GetResultUrl(XDocument doc)
        {
            var resultUrl = string.Empty;
            var task = doc.Root.Element("task");
            if (task != null)
            {
                resultUrl = task.Attribute("resultUrl") != null ? task.Attribute("resultUrl").Value : string.Empty;
            }
            return resultUrl;
        }
        /// <summary>
        /// Gets result file extension by export format
        /// </summary>
        protected static string GetExtension(string exportFormat)
        {
            var extension = string.Empty;
            switch (exportFormat.ToLower())
            {
                case "txt":
                    extension = "txt";
                    break;
                case "rtf":
                    extension = "rtf";
                    break;
                case "docx":
                    extension = "docx";
                    break;
                case "xlsx":
                    extension = "xlsx";
                    break;
                case "pptx":
                    extension = "pptx";
                    break;
                case "pdfsearchable":
                case "pdftextandimages":
                    extension = "pdf";
                    break;
                case "xml":
                    extension = "xml";
                    break;
            }
            return extension;
        }
        /// <summary>
        /// Copies input stream to output, returns stream length
        /// </summary>
        private static int copyStream(Stream input, Stream output)
        {
            var buffer = new byte[8 * 1024];
            var streamLength = 0;
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
                streamLength += len;
            }
            return streamLength;
        }
        /*
        public void Page_Load(object sender, EventArgs e)
        {
            //!!!Please provide application id and password and remove this line. !!!
            // To create an application and obtain a password,
            // register at https://cloud.ocrsdk.com/Account/Register
            // More info on getting your application id and password at
            // https://ocrsdk.com/documentation/faq/#faq3
            // Name of application you created
            ApplicationId = "myApplicationId";
            // The password from e-mail sent by server after application was created
            Password = "myApplicationPassword";

            //!!!Provide your file here and remove this line!!!
            FilePath = "FileToRecognize.png";
            GetResult(ApplicationId, Password, FilePath, "English,German", "txt");
        }
        */
        private IWebProxy _proxy;
        private ICredentials _credentials;
    }
}
