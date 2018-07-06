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
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.HolidayService;
using TrustonTap.Common.Services.LoggingService;

namespace TrustonTap.Web.Controllers
{
    [Authorize]
    [ControllerMetadata("Holidays")]
    public class HolidayController : BaseController
    {
        private IHolidayService holidayService;
        private ILoggingService loggingService;

        public HolidayController(IHolidayService holidayService, ILoggingService loggingService)
        {
            this.holidayService = holidayService;
            this.loggingService = loggingService;
        }

        public ActionResult Index()
		{
            var model = holidayService.GetHolidays();

            return View(model);
		}

        public ActionResult EditModal(int id)
        {
            var holiday = holidayService.GetHoliday(id);

            return PartialView("_EditModal", holiday);
        }

        public ActionResult AddModal()
        {
            var holiday = new Holiday()
            {
                Date = DateTime.Now,
                Region = Region.eng
            };

            return PartialView("_EditModal", holiday);
        }

        [HttpPost]
        public ActionResult Edit(Holiday holiday)
        {
            var success = true;

            try
            {
                holidayService.SaveHoliday(holiday);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "The holiday has been amended.",
                "The holiday could not be amended.");

            return RedirectToSamePage();
        }

        public ActionResult Delete(int id)
        {
            var success = true;

            try
            {
                holidayService.DeleteHoliday(id);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                "The holiday has been deleted.",
                "The holiday could not be deleted.");
            return RedirectToSamePage();
        }

        public ActionResult Download()
        {
            var success = true;
            var year = DateTime.Now.Year;
            var resultCount = 0;

            try
            {
                resultCount = holidayService.UpdateHolidays(Region.eng, year);
                resultCount += holidayService.UpdateHolidays(Region.eng, year+1);
            }
            catch (Exception ex)
            {
                success = false;
                loggingService.LogException(ex);
            }

            SetFeedbackMessage(success,
                $"The holidays for {year} and {year+1} have been updated. {resultCount} dates found.",
                $"There has been an error whilst updating holidays for {year} and {year + 1}.");
            return RedirectToAction("Index");
        }
    }
}