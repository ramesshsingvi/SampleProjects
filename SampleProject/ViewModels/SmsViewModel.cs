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
using System.Web.Mvc;
using TrustonTap.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace TrustonTap.Web.ViewModels
{
    public class SmsViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }

        [Required]
        public string ToNumber { get; set; }

        [Required]
        [StringLength(160, ErrorMessage ="Sms message cannot be more than 160 chars")]
        public string Message { get; set; }
        public IEnumerable<SelectListItem> StandardResponses { get; set; }

        public bool IsPopup { get; set; }

        public SmsResponseSource ResponseSource { get; set; }

    }


}