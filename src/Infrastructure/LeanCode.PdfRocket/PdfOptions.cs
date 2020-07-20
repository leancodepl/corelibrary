namespace LeanCode.PdfRocket
{
    // Parameters description taken from https://www.html2pdfrocket.com/#htmltopdf
    public class PdfOptions
    {
        /// <summary>
        /// Value of left margin - default is 0
        /// </summary>
        public int? MarginLeft { get; set; }

        /// <summary>
        /// Value of right margin - default is 0
        /// </summary>
        public int? MarginRight { get; set; }

        /// <summary>
        /// Value of top margin - default is 0
        /// </summary>
        public int? MarginTop { get; set; }

        /// <summary>
        /// Value of bottom margin - default is 0
        /// </summary>
        public int? MarginBottom { get; set; }

        /// <summary>
        /// <c>true</c> to generate PDF in grayscale, false or leave blank for full colour
        /// </summary>
        public bool? UseGrayscale { get; set; }

        /// <summary>
        /// <c>true</c> to rotate page to landscape, false or leave blank for portrait
        /// </summary>
        public bool? UseLandscape { get; set; }

        /// <summary>
        /// <c>true</c> to turn html form fields into pdf form fields
        /// </summary>
        public bool? EnableForms { get; set; }

        /// <summary>
        /// <c>true</c> to reduce the quality, which may lower your network usage if quality is still satisfactory
        /// </summary>
        public bool? LowQuality { get; set; }

        /// <summary>
        /// override the default image quality percentage (94) and use your own
        /// </summary>
        public int? ImageQuality { get; set; }

        /// <summary>
        /// <c>true</c> to disable the intelligent shrinking process we use make the pixel/dpi ratio constant
        /// </summary>
        public bool? DisableShrinking { get; set; }

        /// <summary>
        /// <c>true</c> to disable running JS on page, otherwise javascript runs
        /// </summary>
        public bool? DisableJavascript { get; set; }

        /// <summary>
        /// Milliseconds to wait for JS to finish executing before converting the page.  Useful for ajax calls.
        /// </summary>
        public int? JavascriptDelay { get; set; }

        /// <summary>
        /// <c>true</c> to use the print media stylesheet, false or leave blank to use normal stylesheet
        /// </summary>
        public bool? UsePrintStylesheet { get; set; }

        /// <summary>
        /// Spacing between the header and the content in mm - default is 0
        /// </summary>
        public int? FooterSpacing { get; set; }

        /// <summary>
        /// Spacing between the footer and the content in mm - default is 0
        /// </summary>
        public int? HeaderSpacing { get; set; }

        /// <summary>
        /// Default size is A4 but you can use Letter, A0, A2, A3, A5, Legal, etc.
        /// </summary>
        public string? PageSize { get; set; }

        /// <summary>
        /// Page width - if you use this, you must also use PageWidth
        /// </summary>
        public int? PageWidth { get; set; }

        /// <summary>
        /// Page height - if you use this, you must also use PageHeight
        /// </summary>
        public int? PageHeight { get; set; }

        /// <summary>
        /// e.g. 800x600 - Set if you have custom scrollbars or css attribute overflow to emulate window size
        /// </summary>
        public string? ViewPort { get; set; }

        /// <summary>
        /// Explicitly set the DPI, which is 96 by default.  Also see Zoom settings
        /// </summary>
        public int? Dpi { get; set; }

        /// <summary>
        /// Default zoom is 1.00  You can use any floating point number, e.g. 0.5, 0.75, 1.10, 2.55, 3...
        /// </summary>
        public float? Zoom { get; set; }

        /// <summary>
        /// Must be either "pdf", "jpg", "png", "bmp" or "svg" if not supplied the default is PDF
        /// </summary>
        public string? OutputFormat { get; set; }

        /// <summary>
        /// Optionally the name you want the file to be called when downloading or $unique$ for guid
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// For URL conversions, creates a secure basic authentication connection to your server
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// For URL conversions, creates a secure basic authentication connection to your server
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Supply in format: NAME|||VALUE|||NAME|||VALUE (that's 2 cookies).  Often used for authentication.
        /// </summary>
        public string? Cookie { get; set; }

        /// <summary>
        /// To use a html header on each page - a string starting with &lt;DOCTYPE html&gt;
        /// </summary>
        public string? HeaderHtml { get; set; }

        /// <summary>
        /// To use a html footer on each page - a string starting with &lt;DOCTYPE html&gt;
        /// </summary>
        public string? FooterHtml { get; set; }

        /// <summary>
        /// To use a html header on each page - a url starting with http containing the html
        /// </summary>
        public string? HeaderUrl { get; set; }

        /// <summary>
        /// To use a html footer on each page - a url starting with http containing the html
        /// </summary>
        public string? FooterUrl { get; set; }

        /// <summary>
        /// Top left header text (can use replacement tags below)
        /// </summary>
        public string? HeaderLeft { get; set; }

        /// <summary>
        /// Top right header text (can use replacement tags below)
        /// </summary>
        public string? HeaderRight { get; set; }

        /// <summary>
        /// Bottom left footer (can use replacement tags below)
        /// </summary>
        public string? FooterLeft { get; set; }

        /// <summary>
        /// Bottom right footer (can use replacement tags below)
        /// </summary>
        public string? FooterRight { get; set; }

        /// <summary>
        /// Footer font names - Arial by default
        /// </summary>
        public string? FooterFontName { get; set; }

        /// <summary>
        /// Header font names - Arial by default
        /// </summary>
        public string? HeaderFontName { get; set; }

        /// <summary>
        /// The font sizes - 12 by default.  Use the the plain value (do not use px or pt)
        /// </summary>
        public int? FooterFontSize { get; set; }

        /// <summary>
        /// The font sizes - 12 by default.  Use the the plain value (do not use px or pt)
        /// </summary>
        public int? HeaderFontSize { get; set; }
    }
}
