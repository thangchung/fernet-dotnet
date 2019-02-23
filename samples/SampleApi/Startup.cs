using Fernet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SampleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.Audience = "api1";
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                var token = context.Request.Headers["Authorization"].ToString();

                if (!string.IsNullOrEmpty(token))
                {
                    var fernetToken = token.Substring("bearer".Length + 1, token.Length - "bearer".Length - 1);

                    // this key should store in the KeyVault service, then we can securely access in anywhere
                    var key = "cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4=".UrlSafe64Decode();
                    var jwt64Token = SimpleFernet.Decrypt(key, fernetToken, out var timestamp);
                    var jwtToken = jwt64Token.UrlSafe64Encode().FromBase64String();

                    // we set it to authorization header, then the internal stack will work normally
                    context.Request.Headers["Authorization"] = $"Bearer {jwtToken}";
                }

                await next.Invoke();
            });

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
