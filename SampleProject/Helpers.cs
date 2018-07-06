using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using TrustonTap.Common;
using TrustonTap.Common.Models;

namespace TrustonTap.Web
{
    public static class Helpers
    {
        public static MvcHtmlString CustomEnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

            var items =
                values
                .Where(x=>!IsObsolete(x))
                .Select(
                   value =>
                   new SelectListItem
                   {
                       Text = GetEnumDescription(value),
                       Value = Convert.ToInt32(value).ToString(),
                       Selected = value.Equals(metadata.Model)
                   });
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return htmlHelper.DropDownListFor(expression, items, attributes);
        }

        public static IHtmlString RenderGridEnumFilter(this HtmlHelper helper, string gridName, string filterWidgetName, Type enumType)
        {
            var opts = new StringBuilder();
            foreach (var v in Enum.GetValues(enumType))
            {
                opts.AppendFormat(@"sel.append('<option ' + (""{0}"" == v ? 'selected=""selected""' : '') + ' value=""{0}"">{1}</option>');", Convert.ToInt32(v), GetEnumDescription(v));
            }
            var script = string.Format(@"
<script type=""text/javascript"">
function {0}() {{
    this.getAssociatedTypes = function () {{ return ['{0}']; }};
    this.onShow = function () {{ }};
    this.showClearFilterButton = function () {{ return true; }};
    this.onRender = function (container, lang, typeName, values, cb, data) {{
        var _this = this;
        var _cb = cb;
        container.append('<select class=""grid-filter-type {0}List form-control""></select>');
        var sel = container.find('.{0}List');
        var v = (values.length > 0 ? values[0] : {{ filterType: 1, filterValue: '' }}).filterValue;
        {1}
        sel.change(function () {{ _cb([{{ filterValue: $(this).val(), filterType: 1 }}]); }});
    }};
}}
$(function () {{
    pageGrids.{2}.addFilterWidget(new {0}());
}});
</script>
        ", filterWidgetName, opts, gridName);
            return helper.Raw(script);
        }

        public static MvcHtmlString CustomEnumDropDownList<TModel>(this HtmlHelper<TModel> htmlHelper, string name, Type enumType, object htmlAttributes)
        {
            return htmlHelper.CustomEnumDropDownList(name, enumType, null, null, htmlAttributes);
        }

        public static MvcHtmlString CustomEnumDropDownList<TModel>(this HtmlHelper<TModel> htmlHelper, string name, Type enumType, string selectedName, string selectedValue, object htmlAttributes)
        {
            var values = Enum.GetValues(enumType);

            var foundSelected = false;
            var items = new List<SelectListItem>();
            foreach (var value in values)
            {
                var itemText = GetEnumDescription(value);
                var item = new SelectListItem
                {
                    Text = itemText,
                    Value = Convert.ToInt32(value).ToString()
                };

                if (!foundSelected && (Convert.ToInt32(value).ToString() == selectedValue || itemText == selectedName))
                {
                    item.Selected = true;
                    foundSelected = true;
                }
                items.Add(item);
            }

            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return htmlHelper.DropDownList(name, items, attributes);
        }

        private class ScriptBlock : IDisposable
        {
            private const string scriptsKey = "scripts";
            public static List<string> pageScripts
            {
                get
                {
                    if (HttpContext.Current.Items[scriptsKey] == null)
                        HttpContext.Current.Items[scriptsKey] = new List<string>();
                    return (List<string>)HttpContext.Current.Items[scriptsKey];
                }
            }

            WebViewPage webPageBase;

            public ScriptBlock(WebViewPage webPageBase)
            {
                this.webPageBase = webPageBase;
                this.webPageBase.OutputStack.Push(new StringWriter());
            }

            public void Dispose()
            {
                pageScripts.Add(((StringWriter)this.webPageBase.OutputStack.Pop()).ToString());
            }
        }

        public static IDisposable BeginScripts(this HtmlHelper helper)
        {
            return new ScriptBlock((WebViewPage)helper.ViewDataContainer);
        }

        public static MvcHtmlString PageScripts(this HtmlHelper helper)
        {
            return MvcHtmlString.Create(string.Join(Environment.NewLine, ScriptBlock.pageScripts.Select(s => s.ToString())));
        }

        public static string SplitCamelCase(this string text)
        {
            return Regex.Replace(text.ToString(), "(\\B[A-Z0-9])", " $1");
        }

        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static bool IsObsolete<TEnum>(TEnum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attributes = (ObsoleteAttribute[])field.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            return attributes.Length > 0;
        }

        [Obsolete("Use PaymentMethod instead")]
        public static string GetBadge(this PaymentProvider paymentProvider)
        {
            var icon = "";
            switch (paymentProvider)
            {
                case PaymentProvider.Cheque:
                    icon = "fa-money-bill-alt";
                    break;
                case PaymentProvider.Unknown:
                    icon = "fa-question-circle";
                    break;
                case PaymentProvider.Bacs:
                    icon = "fa-university";
                    break;
                case PaymentProvider.GoCardless:
                    icon = "fa-newspaper";
                    break;
                case PaymentProvider.Stripe:
                    icon = "fa-credit-card";
                    break;
            }

            if (String.IsNullOrWhiteSpace(icon))
            {
                return string.Empty;
            }
            else
            {
                return String.Format(@"<i title='{0}' class='fa {1}'></i>", paymentProvider.GetDescription(), icon);
            }
        }

        public static string GetBadge(this PaymentMethod paymentMethod)
        {
            var icon = "";
            switch (paymentMethod)
            {
                case PaymentMethod.Cheque:
                    icon = "fa-money-bill-alt";
                    break;
                case PaymentMethod.Cash:
                    icon = "fa-money-bill-alt";
                    break;
                case PaymentMethod.GoCardless:
                    icon = "fa-university";
                    break;
                case PaymentMethod.Stripe:
                    icon = "fa-credit-card";
                    break;
                case PaymentMethod.BankTransfer:
                    icon = "fa-university";
                    break;
            }

            if (String.IsNullOrWhiteSpace(icon))
            {
                return string.Empty;
            } else
            {
                return String.Format(@"<i title='{0}' class='fa {1}'></i>", paymentMethod.GetDescription(), icon);
            }
        }
    }
}