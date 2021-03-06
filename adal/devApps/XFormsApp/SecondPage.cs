﻿//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace XFormsApp
{
    public class SecondPage : ContentPage
    {
        private readonly StringBuilder _logs = new StringBuilder();

        public string DrainLogs()
        {
            string output = _logs.ToString();
            _logs.Clear();
            return output;
        }

        private Label result;

        public SecondPage()
        {
            var acquireTokenButton = new Button
            {
                Text = "Acquire Token"
            };

            var acquireTokenSilentButton = new Button
            {
                Text = "Acquire Token Silent"
            };

            var conditionalAccessButton = new Button
            {
                Text = "Conditional Access"
            };

            var clearButton = new Button
            {
                Text = "Clear Cache"
            };

            result = new Label()
            {
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            var scrollView = new ScrollView()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Content = new StackLayout()
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Children =
                    {
                        result
                    }
                }
            };

            acquireTokenButton.Clicked += browseButton_Clicked;
            acquireTokenSilentButton.Clicked += acquireTokenSilentButton_Clicked;
            conditionalAccessButton.Clicked += conditionalAccessButton_Clicked;
            clearButton.Clicked += clearButton_Clicked;

            Thickness padding;
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    padding = new Thickness(0, 40, 0, 0);
                    break;
                default:
                    padding = new Thickness(0, 0, 0, 0);
                    break;
            }

            Content = new StackLayout
            {
                Padding = padding,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    acquireTokenButton,
                    acquireTokenSilentButton,
                    conditionalAccessButton,
                    clearButton,
                    scrollView
                }
            };

            void LogCallback(LogLevel level, string message, bool containsPii)
            {
                _logs.AppendLine(message);
            }

            LoggerCallbackHandler.LogCallback = LogCallback;
        }

        private async void acquireTokenSilentButton_Clicked(object sender, EventArgs e)
        {
            this.result.Text = string.Empty;
            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
            string output = string.Empty;
            try
            {
                AuthenticationResult result = await ctx.AcquireTokenSilentAsync("https://graph.windows.net", "de49ddaf-c7f8-4a06-8463-3c6ae124fe52").ConfigureAwait(false);
                output = "Signed in User - " + result.UserInfo.DisplayableId;
            }
            catch (Exception exc)
            {
                output = exc.Message;
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.result.Text += "Result : " + output;

                    this.result.Text += "Logs : " + DrainLogs();
                });
            }

        }

        public IPlatformParameters Parameters { get; set; }

        async void browseButton_Clicked(object sender, EventArgs e)
        {
            this.result.Text = string.Empty;
            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
            string output = string.Empty;
            try
            {
                AuthenticationResult result =
                    await
                        ctx.AcquireTokenAsync("https://graph.windows.net", "de49ddaf-c7f8-4a06-8463-3c6ae124fe52",
                            new Uri("adaliosapp://com.yourcompany.xformsapp"),
                            Parameters).ConfigureAwait(false);
                output = "Signed in User - " + result.UserInfo.DisplayableId;
            }
            catch (Exception exc)
            {
                output = exc.Message;
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.result.Text += "Result : " + output;

                    this.result.Text += "Logs : " + DrainLogs();
                });
            }

        }

        private async void conditionalAccessButton_Clicked(object sender, EventArgs e)
        {
            this.result.Text = string.Empty;
            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
            string output = string.Empty;
            string claims = "{\"access_token\":{\"polids\":{\"essential\":true,\"values\":[\"5ce770ea-8690-4747-aa73-c5b3cd509cd4\"]}}}";
            
            try
            {
                AuthenticationResult result = await ctx.AcquireTokenAsync("https://graph.windows.net", "<CLIENT_ID>",
                        new Uri("adaliosapp://com.yourcompany.xformsapp"),
                        Parameters, new UserIdentifier("<USER>", UserIdentifierType.OptionalDisplayableId), null, claims).ConfigureAwait(false);
                output = "Access Token: " + result.AccessToken;
            }

            catch (Exception exc)
            {
                output = exc.Message;
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.result.Text += "Result : " + output;

                    this.result.Text += "Logs : " + DrainLogs();
                });
            }
        }

        void clearButton_Clicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.result.Text = "Cache items before clear: " + TokenCache.DefaultShared.Count + Environment.NewLine;
                TokenCache.DefaultShared.Clear();
                this.result.Text += "Cache items after clear: " + TokenCache.DefaultShared.Count + Environment.NewLine;
            });
        }
    }
}
