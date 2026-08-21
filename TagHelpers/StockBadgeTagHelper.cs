using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Book_Store.TagHelpers
{
    /// <summary>
    /// Renders a stock status indicator in one of two modes:
    ///
    /// LABEL mode (default) — used on ProductDetail, always renders, shows count:
    ///   <stock-badge stock="@Model.Stock" />
    ///   → <span class="meta-value stock-in">Còn hàng (12)</span>
    ///   → <span class="meta-value stock-low">Sắp hết (3)</span>
    ///   → <span class="meta-value stock-out">Hết hàng</span>
    ///
    /// BADGE mode — used on ProductList card image overlay, renders nothing if in stock:
    ///   <stock-badge stock="@book.Stock" mode="badge" />
    ///   → <span class="badge-out">Hết hàng</span>   (stock == 0)
    ///   → <span class="badge-low">Sắp hết</span>    (stock 1–5)
    ///   → (nothing rendered)                         (stock > 5)
    /// </summary>
    [HtmlTargetElement("stock-badge")]
    public class StockBadgeTagHelper : TagHelper
    {
        /// <summary>Current stock quantity of the book.</summary>
        public int Stock { get; set; }

        /// <summary>Display mode: "label" (default) or "badge".</summary>
        public string Mode { get; set; } = "label";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;

            if (Mode == "badge")
            {
                // Card image overlay — render nothing when stock is healthy
                if (Stock == 0)
                {
                    output.Attributes.SetAttribute("class", "badge-out");
                    output.Content.SetContent("Hết hàng");
                }
                else if (Stock <= 5)
                {
                    output.Attributes.SetAttribute("class", "badge-low");
                    output.Content.SetContent("Sắp hết");
                }
                else
                {
                    // Suppress the tag entirely — no badge shown for healthy stock
                    output.SuppressOutput();
                }
            }
            else
            {
                // Label mode — always renders, includes count for in-stock states
                if (Stock > 5)
                {
                    output.Attributes.SetAttribute("class", "meta-value stock-in");
                    output.Content.SetContent($"Còn hàng ({Stock})");
                }
                else if (Stock > 0)
                {
                    output.Attributes.SetAttribute("class", "meta-value stock-low");
                    output.Content.SetContent($"Sắp hết ({Stock})");
                }
                else
                {
                    output.Attributes.SetAttribute("class", "meta-value stock-out");
                    output.Content.SetContent("Hết hàng");
                }
            }
        }
    }
}
