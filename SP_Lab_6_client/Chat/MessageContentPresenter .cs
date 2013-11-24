using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SP_Lab_6_client.Chat
{
    public class MessageContentPresenter : ContentControl
    {
        /// <summary>
        /// The DataTemplate to use when Message.Side == Side.Me
        /// </summary>
        public static DataTemplate MeTemplate { get; set; }

        /// <summary>
        /// The DataTemplate to use when Message.Side == Side.You
        /// </summary>
        public static DataTemplate YouTemplate { get; set; }

        static MessageContentPresenter()
        {
            var w = new ChatWindow("");
            MeTemplate = (DataTemplate)w.FindResource("MeTemplate");
            YouTemplate = (DataTemplate)w.FindResource("YouTemplate");
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            if (newContent as ClientMessage == null)
                return;
            // apply the required template
            var message = newContent as ClientMessage;
            if (message.Side == MessageSide.Me)
            {
                ContentTemplate = MeTemplate;
            }
            else
            {
                ContentTemplate = YouTemplate;
            }
        }
    }
}
