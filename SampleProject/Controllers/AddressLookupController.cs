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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using TrustonTap.Common.Services.AddressLookupService;
using TrustonTap.Common.Services.LoggingService;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Address Lookup", "Index")]
    public class AddressLookupController : BaseController
    {
        private IAddressLookupService addressLookupService;
        private ILoggingService loggingService;

        public AddressLookupController(
            IAddressLookupService addressLookupService,
            ILoggingService loggingService)
        {
            this.addressLookupService = addressLookupService;
            this.loggingService = loggingService;
        }

        public JsonResult SearchPostcodesJSON(string search)
        {
            var success = true;

            var postcodes = new List<string>();
            try
            {
                postcodes = addressLookupService.SearchPostcodes(search);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }
            return Json(new { items = postcodes, success = success }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTown(string postcode)
        {
            var success = true;

            var town = string.Empty;
            try
            {
                town = addressLookupService.GetTown(postcode);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }
            return Json(new { town = town, success = success }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetDrivingDistanceJSON(string address1, string address2)
        {
            var success = true;

            Route route = new Route();
            try
            { 
                route =  await addressLookupService.GetDrivingRouteAsync(address1, address2);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }
            return Json(new { route = route, success = success, display = route.ToString() }, JsonRequestBehavior.AllowGet);
        }

    }
}