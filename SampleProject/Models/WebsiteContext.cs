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

using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Security.Claims;
using TrustonTap.Common.Models;
using TrustonTap.Common.Services;

namespace TrustonTap.Web.Models
{
    public class WebsiteContext : ServiceContext
    {
        public WebsiteContext(IAuthenticationManager authenticationManager)
        {
            var user = authenticationManager.User;

            var email = user.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).SingleOrDefault();
            var userId = Convert.ToInt32(user.Claims.Where(c => c.Type == ClaimTypes.System).Select(c => c.Value).SingleOrDefault());
            var username = user.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
            var name = user.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            this.User = new InternalUser()
            {
                ID = userId,
                UserName = username,
                Email = email,
                Name = name
            };
        }
    }
}