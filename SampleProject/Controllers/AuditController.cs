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

using System;
using System.Web.Mvc;
using TrustonTap.Common.Services.AuditService;
using TrustonTap.Web.ViewModels;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Audit History")]
    public class AuditController : BaseController
    {
        private IAuditService auditService;

        public AuditController(IAuditService auditService)
        {
            this.auditService = auditService;
        }

		public ActionResult Index(AuditLogSearchViewModel model)
        {
            if(model == null)
            {
                model = new AuditLogSearchViewModel();
            }

            model.Results = auditService.SearchAuditHistory(model.SearchString, model.EventType, model.DateFrom, model.DateTo);

            return View(model);
        }
	}
}