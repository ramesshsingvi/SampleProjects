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

using Autofac;
using System;
using System.Linq;
using TrustonTap.Services.AddressLookupService.MapProviders.TomTom;
using TrustonTap.Services.AuditService.EventStores;
using TrustonTap.Services.DocumentService.StorageProviders;
using TrustonTap.Services.LoggingService;
using TrustonTap.Services.MessagingService.SmsProviders;

namespace TrustonTap.Web.Modules
{
    public class ServiceModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                            .Where(t => t.Name.EndsWith("Service"))
                .WithParameter("auditEventStore", new DatabaseEventStore())
                .WithParameter("storageProvider", new DatabaseStorageProvider())
                .WithParameter("smsProvider", new TwilioSmsProvider())
                .WithParameter("mapProvider", new TomTomMapProvider())
                .WithParameter("logger", Log4NetLogger.Instance)
                .AsImplementedInterfaces()
                .InstancePerRequest();
        }
    }
}