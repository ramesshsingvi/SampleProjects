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
using TrustonTap.Common.Models;

namespace TrustonTap.Web.ViewModels
{
    public class AuditLogSearchViewModel
    {
        public AuditLogSearchViewModel()
        {
            this.DateFrom = DateTime.Now.AddDays(-7);
            this.DateTo = DateTime.Now;
            this.EventType = AuditEventType.Unknown;
        }

        public string SearchString { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public AuditEventType EventType { get; set; }
   
        public bool Admin { get; set; }

        public List<AuditEvent> Results { get; set; }

    }
}