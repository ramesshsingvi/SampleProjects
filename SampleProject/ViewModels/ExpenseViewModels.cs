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

using TrustonTap.Common.Models;

namespace TrustonTap.Web.ViewModels
{
    public class ExpenseViewModel : Expense
    {
        public decimal Miles { get; set; }
        public int TimesheetID { get; set; }

        public static ExpenseViewModel ToViewModel(Expense expense)
        {
            var viewModel = new ExpenseViewModel()
            {
                AgreementID = expense.AgreementID,
                CreatedDate = expense.CreatedDate,
                CarerPaymentGenerated = expense.CarerPaymentGenerated,
                CarerID = expense.CarerID,
                CustomerPaymentGenerated = expense.CustomerPaymentGenerated,
                Notes = expense.Notes,
                ID = expense.ID,
                AmountClaimed = expense.AmountClaimed,
                ExpenseDate = expense.ExpenseDate,
                ExpenseStatusID = expense.ExpenseStatusID,
                ExtraInfo = expense.ExtraInfo,
                ExpenseTypeID = expense.ExpenseTypeID
            };

            return viewModel;
        }
    }
}