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
using System.Xml.Serialization;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services.CarerService;

namespace TrustonTap.Web.ViewModels
{
    [XmlInclude(typeof(CarerSearchParameters))]
    public class CarerSearchViewModel : CarerSearchParameters
    {
        public CarerSearchViewModel()
        {
            Results = new List<Carer>();
        }

        [XmlIgnore]
        public List<Carer> Results { get; set; }
    }
}