using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Book_Store.TagHelpers
{
    /// <summary>
    /// Replace @((price).ToString("#,##0", vi-VN))VND.
    /// Usage: <vnd-price value="@item.Price" />
    /// Output: <span class="vnd-price">125.000 VND</span>
    /// Optional css class: <vnd-price value="@item.Price" class="text-danger fw-bold" />
    /// </summary>
    [HtmlTargetElement("vnd-price")]
    public class PriceTagHelper : TagHelper
    {
        private static readonly CultureInfo ViVn = new CultureInfo("vi-VN");

        /// <summary>The price value in VND.</summary>
        public decimal Value { get; set; }

        /// <summary>Optional extra CSS classes on the rendered span.</summary>
        public string? Class { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;

            var cssClass = string.IsNullOrWhiteSpace(Class) ? "vnd-price" : $"vnd-price {Class}";

            output.Attributes.SetAttribute("class", cssClass);
            output.Content.SetContent($"{Value.ToString("#,##0", ViVn)} VND");
        }
    }
}
