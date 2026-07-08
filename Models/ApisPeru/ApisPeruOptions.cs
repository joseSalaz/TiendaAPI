using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ApisPeru
{
    public class ApisPeruOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string CompanyToken { get; set; } = string.Empty;

        public string InvoiceSendPath { get; set; } = "/api/v1/invoice/send";
        public string InvoicePdfPath { get; set; } = "/api/v1/invoice/pdf";
        public string InvoiceXmlPath { get; set; } = "/api/v1/invoice/xml";
        public string InvoiceStatusPath { get; set; } = "/api/v1/invoice/status";
        public string NoteSendPath { get; set; } = "/api/v1/note/send";
        public string NotePdfPath { get; set; } = "/api/v1/note/pdf";
        public string NoteXmlPath { get; set; } = "/api/v1/note/xml";
    }
}
