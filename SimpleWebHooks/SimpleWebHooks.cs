using PX.Data.Webhooks;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;

using PX.Data;
using System;
using PX.Objects.AR;
using PX.Objects.IN;

namespace Simple.WebHooks
{
    // This builds 2 webhooks to keep them both as simple as possible.
    // The code in the first one will fail, but if you run it from, say VendorMaint wtih this it will run just fine
    // :
    //public PXAction<PX.Objects.AP.VendorR> BtnTEKARHead;
    //[PXButton(CommitChanges = true)]
    //[PXUIField(DisplayName = "Send AR Header")]
    //protected void btnTEKARHead()
    //{
    //    SendHeader();
    //}

    public class SuperSimpleWebHookHandler : IWebhookHandler
    {

        public static IDisposable GetAdminScope()
        {
            var userName = "admin";
            if (PXDatabase.Companies.Length > 0)
            {
                var company = PXAccess.GetCompanyName();
                if (string.IsNullOrEmpty(company))
                {
                    company = PXDatabase.Companies[0];
                }
                userName = userName + "@" + company;
            }
            return new PXLoginScope(userName);
        }

        public static bool SendHeader()
        {
            using (var scope = GetAdminScope()) // This is how it runs in the webhook, in the admin scope, so thought I could give it a test.
            {
                ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
                ARInvoice theInvoice = new ARInvoice();
                theInvoice.DocType = "INV";
                theInvoice = graph.Document.Insert(theInvoice);
                theInvoice.CustomerID = 7144; // Hard coded to simplify. These are valid values.
                theInvoice.InvoiceNbr = "TEST123";
                graph.Document.Update(theInvoice);
                graph.Save.Press();
            }
            return true;
        }

        public async Task<System.Web.Http.IHttpActionResult> ProcessRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Any hit to this endpoint should try and make an invoice header.
            using (var scope = GetAdminScope())
            {
                if (request.Method == HttpMethod.Post)
                {
                    string jsonContent = request.Content.ReadAsStringAsync().Result;
                    SendHeader();
                }
            }
            return new OkResult(request);
        }

    }
    public class SimpleItemWebHookHandler : IWebhookHandler
    {
        public static IDisposable GetAdminScope()
        {
            var userName = "admin";
            if (PXDatabase.Companies.Length > 0)
            {
                var company = PXAccess.GetCompanyName();
                if (string.IsNullOrEmpty(company))
                {
                    company = PXDatabase.Companies[0];
                }
                userName = userName + "@" + company;
            }
            return new PXLoginScope(userName);
        }

        public static void SendItem()
        {
            InventoryItemMaint graph = PXGraph.CreateInstance<InventoryItemMaint>();
            InventoryItem theItem = new InventoryItem();
            theItem.InventoryCD = "TESTITEM";
            theItem = graph.Item.Insert(theItem);
            theItem.ItemClassID = 49; // "ALLOTHER" from stock data
            theItem.Descr = "Test Item Description";
            graph.Item.Update(theItem);
            graph.Save.Press();
        }

        public async Task<System.Web.Http.IHttpActionResult> ProcessRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Any hit to this endpoint will try and make an item (note the inventorycd is hard coded and does require autonumbering be off) so it will only run once)
            using (var scope = GetAdminScope())
            {
                if (request.Method == HttpMethod.Post)
                {
                    string jsonContent = request.Content.ReadAsStringAsync().Result;
                    SendItem();
                }
            }
            return new OkResult(request);
        }

    }

}