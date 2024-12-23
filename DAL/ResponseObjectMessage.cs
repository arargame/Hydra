using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DAL
{
    public class ResponseObjectMessage
    {
        public string? Title { get; set; } = null;
        public string? Text { get; set; } = null;
        public bool ShowWhenSuccess { get; set; }
        public string? RedirectionLink { get; set; } = null;
        public string? RedirectionLinkText { get; set; } = "Back";

        public ResponseObjectMessage(string title, string text, bool showWhenSuccess = true)
        {
            Title = title;
            Text = text;
            ShowWhenSuccess = showWhenSuccess;
        }

        public ResponseObjectMessage SetRedirectionLink(string link, string linkText = "Back")
        {
            RedirectionLink = link;
            RedirectionLinkText = linkText;
            return this;
        }
    }

}
