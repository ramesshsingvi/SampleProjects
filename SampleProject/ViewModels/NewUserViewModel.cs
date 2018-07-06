﻿/*
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

using System.ComponentModel;
using TrustonTap.Common.Models;

namespace TrustonTap.Web.ViewModels
{
    public class NewUserViewModel
    {
        public User User { get; set; }
        public int? BookingId { get; set; }
        public string Title { get; set; }
        public UserType UserType { get; set; }
        public bool GenerateUsername { get; set; }
    }

    public enum UserType
    {
        Unknown,
        [Description("Customer")]
        Customer,
        [Description("Payer")]
        Payer,
        [Description("Care Recipient")]
        CareRecipient
    }
}