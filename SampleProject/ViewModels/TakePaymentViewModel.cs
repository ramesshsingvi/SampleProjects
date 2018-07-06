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

using System.Collections.Generic;
using TrustonTap.Common.Models;

namespace TrustonTap.Web.ViewModels
{
    public class TakePaymentViewModel
    {
        public TakePaymentViewModel() : base()
        {
            this.Allocations = new Dictionary<CustomerStatement, decimal>();
        }

        public Dictionary<CustomerStatement, decimal> Allocations { get; set; }

        public Payer Payer { get; set; }

        public string Notes { get; set; }

        public decimal Amount { get; set; }

        public string Reference { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string PaymentDate { get; set; }

        public decimal TotalOutstanding { get; set; }
    }
}