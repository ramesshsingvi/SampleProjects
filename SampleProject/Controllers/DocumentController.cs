/*
' Copyright (c) 2017  Blueclover Consulting Ltd
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System.Web.Mvc;
using TrustonTap.Common;
using TrustonTap.Common.Services.DocumentService;
using TrustonTap.Common.Services.PaymentService;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    public class DocumentController : BaseController
    {
        private IDocumentService documentService;
        private IPaymentService paymentService;

        public DocumentController(
            IDocumentService documentService,
            IPaymentService paymentService)
        {
            this.documentService = documentService;
            this.paymentService = paymentService;
        }

        public FileResult DownloadDocument(int id)
        {
            var document = documentService.GetDocument(id);
            var file = new FileContentResult(document.DocumentContent, document.MimeType)
            {
                FileDownloadName = document.Filename
            };

            return file;
        }

        public FileResult DownloadPaymentSchedule(int id)
        {
            var documentContent = paymentService.ExportPaymentSchedule(id);
            var mimeType = Utilities.GetMimeType(".xlsx");

            var file = new FileContentResult(documentContent, mimeType)
            {
                FileDownloadName = "Export.xlsx"
            };

            return file;
        }
    }
}