using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.Owin.Security;
using Owin;
using System.Web;
using System.Web.Mvc;
using TrustonTap.Common.Services;
using TrustonTap.Web.Models;
using TrustonTap.Web.Modules;

namespace TrustonTap.Web
{
    public partial class Startup
    {
        public void ConfigureDependencies(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            // Register your MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register Modules
            builder.RegisterModule<ServiceModule>();

            //builder.RegisterType<ApplicationUserManager>().AsSelf().InstancePerRequest();
            //builder.RegisterType<ApplicationSignInManager>().AsSelf().InstancePerRequest();
            //builder.Register(c => new UserStore<ApplicationUser, UserRoles, UserClaim, UserLogin, ApplicationRole, Guid>("ApplicationDatabase")).AsImplementedInterfaces().InstancePerRequest();
            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication).As<IAuthenticationManager>();
            //builder.Register(c => new IdentityFactoryOptions<ApplicationUserManager>
            //{
            //    DataProtectionProvider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("Application​")
            //});

            //Services
            builder.RegisterType<WebsiteContext>().As<ServiceContext>().InstancePerRequest();


            // Run other optional steps, like registering model binders,
            // web abstractions, etc., then set the dependency resolver
            // to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Register the Autofac middleware FIRST, then the Autofac MVC middleware.
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();
        }
    }
}