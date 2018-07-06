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

using TrustonTap.Web.ModelBinders;
using System;

namespace TrustonTap.Web
{
    public class ModelBindersConfig
    {
        public static void RegisterCustomModelBinders()
        {
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(DateTime), new DateTimeBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(DateTime?), new NullableDateTimeBinder());
        }
    }
}