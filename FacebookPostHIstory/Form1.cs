﻿using System;
using System.Windows.Forms;
using Facebook;

namespace FacebookPostHIstory
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Uri GenerateLoginUrl(string appId, string appSecret, string permissions)
        {
            var fb = new FacebookClient();

            return fb.GetLoginUrl(new
            {
                client_id = appId,
                client_secret = appSecret,
                redirect_uri = "https://www.facebook.com/connect/login_success.html",
                response_type = "token",
                scope = permissions
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var url = GenerateLoginUrl("1093443474080502", "a84b2907d84c3bf4114981b528d1d05c", "user_posts");
            webBrowser1.Navigate(url);
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var fb = new FacebookClient();

            FacebookOAuthResult oauthResult;
            if (fb.TryParseOAuthCallbackUrl(e.Url, out oauthResult))
            {
                if (oauthResult.IsSuccess)
                {
                    var accesstoken = oauthResult.AccessToken;

                    var form2 = new Form2 {AccessToken = accesstoken};

                    Hide();
                    form2.FormClosed += (s, args) => Close();
                    form2.Show();
                }
                else
                {
                    var errorDescription = oauthResult.ErrorDescription;
                    var errorReason = oauthResult.ErrorReason;

                    MessageBox.Show(errorDescription + @": " + Environment.NewLine + errorReason);
                }
            }
        }
    }
}