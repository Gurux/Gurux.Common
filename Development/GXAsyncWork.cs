//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gurux.Common;
using System.Windows.Forms;

namespace Gurux.Common
{
    /// <summary>
    /// Method to execute.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="parameters"></param>
    public delegate object AsyncTransaction(System.Windows.Forms.Control sender, object[] parameters);
    
    /// <summary>
    /// Status of work is changed.
    /// </summary>
    /// <param name="work">Work to execute</param>
    /// <param name="sender">Sender Form.</param>
    /// <param name="parameters">Wrok parameters.</param>   
    /// <param name="state">New state.</param>
    /// <param name="text">Shown text.</param>
    public delegate void AsyncStateChangeEventHandler(GXAsyncWork work, System.Windows.Forms.Control sender, object[] parameters, AsyncState state, string text);

    /// <summary>
    /// Show progress form.
    /// </summary>
    /// <param name="form"></param>
    /// <param name="parent"></param>
    delegate void ShowProgressForm(Form form, Form parent);

    /// <summary>
    /// Close progress form.
    /// </summary>
    /// <param name="parent"></param>
    delegate void CloseProgressForm(Form parent);

    /// <summary>
    /// This class is used to start work that requires thread.
    /// </summary>
    public class GXAsyncWork
    {
        ManualResetEvent Done = new ManualResetEvent(false);
        string Text;
        Form Sender;
        AsyncTransaction Command;
        object[] Parameters;
        Thread Thread;
        AsyncStateChangeEventHandler OnAsyncStateChangeEventHandler;
        Form ProgressForm;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="command"></param>
        /// <param name="text"></param>
        /// <param name="parameters"></param>
        public GXAsyncWork(Form sender, AsyncStateChangeEventHandler e, AsyncTransaction command, string text, object[] parameters, Form progressForm)
        {
            ProgressForm = progressForm;
            ProgressForm.FormClosing += new FormClosingEventHandler(ProgressForm_FormClosing);
            Text = text;
            OnAsyncStateChangeEventHandler = e;
            Sender = sender;
            Command = command;
            Parameters = parameters;
        }

        void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cancel();
        }

        /// <summary>
        /// Result of async work.
        /// </summary>
        public object Result
        {
            get;
            private set;
        }

        void OnError(object sender, Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(Sender, ex.Message);
        }

        void OnShowProgressForm(Form form, Form parent)
        {
            try
            {
                //form.ShowDialog(parent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        void OnCloseProgressForm(Form form)
        {
            try
            {
                form.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void Run()
        {
            try
            {   
                /*
                if (Sender.InvokeRequired)
                {
                    Sender.Invoke(new ShowProgressForm(OnShowProgressForm), ProgressForm, Sender);
                }
                else
                {
                    OnShowProgressForm(ProgressForm, Sender);
                } 
                 * */
                Result = Command(Sender, Parameters);                
                if (Sender.InvokeRequired)
                {
                    Sender.BeginInvoke(OnAsyncStateChangeEventHandler, this, Sender, Parameters, AsyncState.Finish, null);
                }
                else
                {                    
                    OnAsyncStateChangeEventHandler(this, Sender, Parameters, AsyncState.Finish, null);
                }
            }
            catch (Exception ex)
            {
                if (Sender.InvokeRequired)
                {
                    Sender.Invoke(new ErrorEventHandler(OnError), Sender, ex);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(Sender, ex.Message);
                }
            }
            finally
            {
                ProgressForm.FormClosing -= new FormClosingEventHandler(ProgressForm_FormClosing);
                /*
                if (Sender.InvokeRequired)
                {
                    Sender.BeginInvoke(new CloseProgressForm(OnCloseProgressForm), ProgressForm);
                }
                else
                {
                    OnCloseProgressForm(ProgressForm);
                } 
                 * */
                Done.Set();
            }
        }

        /// <summary>
        /// Start work.
        /// </summary>
        public void Start()
        {
            Done.Reset();
            int w = ProgressForm.Width;
            int h = ProgressForm.Height;
            ProgressForm.Location = Sender.Location;
    //        ProgressForm.Width = w;
      //      ProgressForm.Height = h;
            ProgressForm.Show(Sender);
            OnAsyncStateChangeEventHandler(this, Sender, Parameters, AsyncState.Start, Text);
            Thread = new Thread(new ThreadStart(Run));
            Thread.IsBackground = true;
            Thread.Start();
        }

        /// <summary>
        /// Cancel work.
        /// </summary>
        public void Cancel()
        {
            try
            {                                
                OnAsyncStateChangeEventHandler(this, Sender, Parameters, AsyncState.Cancel, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Wait until work is done.
        /// </summary>
        /// <param name="wt">Wait time in ms.</param>
        public bool Wait(int wt)
        {
            DateTime start = DateTime.Now;
            while (!Done.WaitOne(100))
            {
                if (wt > 0 && (DateTime.Now - start).TotalMilliseconds > wt)
                {
                    ProgressForm.Close();
                    return false;
                }
                System.Windows.Forms.Application.DoEvents();
            }
            ProgressForm.Close();
            return true;
        }
    }
}
