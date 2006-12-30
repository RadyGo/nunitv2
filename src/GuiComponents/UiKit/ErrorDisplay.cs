using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using NUnit.Util;
using NUnit.Core;
using CP.Windows.Forms;

namespace NUnit.UiKit
{
	/// <summary>
	/// Summary description for ErrorDisplay.
	/// </summary>
	public class ErrorDisplay : System.Windows.Forms.UserControl, TestObserver
	{
		private ISettings settings;
		int hoverIndex = -1;
		private System.Windows.Forms.Timer hoverTimer;
		TipWindow tipWindow;

		private System.Windows.Forms.ListBox detailList;
		public CP.Windows.Forms.ExpandingTextBox stackTrace;
		public System.Windows.Forms.Splitter tabSplitter;
		private System.Windows.Forms.ContextMenu detailListContextMenu;
		private System.Windows.Forms.MenuItem copyDetailMenuItem;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ErrorDisplay()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			if ( !this.DesignMode )
			{
				settings = NUnit.Util.Services.UserSettings;

				int splitPosition = settings.GetSetting( "Gui.ResultTabs.ErrorsTabSplitterPosition", tabSplitter.SplitPosition );
				if ( splitPosition >= tabSplitter.MinSize && splitPosition < this.ClientSize.Height )
					this.tabSplitter.SplitPosition = splitPosition;

				stackTrace.AutoExpand = settings.GetSetting( "Gui.ResultTabs.ErrorsTab.ToolTipsEnabled", true );
				stackTrace.WordWrap = settings.GetSetting( "Gui.ResultTabs.ErrorsTab.WordWrapEnabled", true );
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.detailList = new System.Windows.Forms.ListBox();
			this.tabSplitter = new System.Windows.Forms.Splitter();
			this.stackTrace = new CP.Windows.Forms.ExpandingTextBox();
			this.detailListContextMenu = new System.Windows.Forms.ContextMenu();
			this.copyDetailMenuItem = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// detailList
			// 
			this.detailList.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.detailList.Font = new System.Drawing.Font("Courier New", 8.25F);
			this.detailList.HorizontalExtent = 2000;
			this.detailList.HorizontalScrollbar = true;
			this.detailList.ItemHeight = 16;
			this.detailList.Location = new System.Drawing.Point(0, 0);
			this.detailList.Name = "detailList";
			this.detailList.ScrollAlwaysVisible = true;
			this.detailList.Size = new System.Drawing.Size(496, 128);
			this.detailList.TabIndex = 1;
			this.detailList.MouseHover += new System.EventHandler(this.OnMouseHover);
			this.detailList.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.detailList_MeasureItem);
			this.detailList.MouseMove += new System.Windows.Forms.MouseEventHandler(this.detailList_MouseMove);
			this.detailList.MouseLeave += new System.EventHandler(this.detailList_MouseLeave);
			this.detailList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.detailList_DrawItem);
			this.detailList.SelectedIndexChanged += new System.EventHandler(this.detailList_SelectedIndexChanged);
			// 
			// tabSplitter
			// 
			this.tabSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.tabSplitter.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.tabSplitter.Location = new System.Drawing.Point(0, 128);
			this.tabSplitter.MinSize = 100;
			this.tabSplitter.Name = "tabSplitter";
			this.tabSplitter.Size = new System.Drawing.Size(496, 9);
			this.tabSplitter.TabIndex = 3;
			this.tabSplitter.TabStop = false;
			this.tabSplitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.tabSplitter_SplitterMoved);
			// 
			// stackTrace
			// 
			this.stackTrace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stackTrace.Font = new System.Drawing.Font("Courier New", 9.75F);
			this.stackTrace.Location = new System.Drawing.Point(0, 137);
			this.stackTrace.Multiline = true;
			this.stackTrace.Name = "stackTrace";
			this.stackTrace.ReadOnly = true;
			this.stackTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.stackTrace.Size = new System.Drawing.Size(496, 151);
			this.stackTrace.TabIndex = 2;
			this.stackTrace.Text = "";
			this.stackTrace.WordWrap = false;
			// 
			// detailListContextMenu
			// 
			this.detailListContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								  this.copyDetailMenuItem});
			// 
			// copyDetailMenuItem
			// 
			this.copyDetailMenuItem.Index = 0;
			this.copyDetailMenuItem.Text = "Copy";
			this.copyDetailMenuItem.Click += new System.EventHandler(this.copyDetailMenuItem_Click);
			// 
			// ErrorDisplay
			// 
			this.Controls.Add(this.stackTrace);
			this.Controls.Add(this.tabSplitter);
			this.Controls.Add(this.detailList);
			this.Name = "ErrorDisplay";
			this.Size = new System.Drawing.Size(496, 288);
			this.ResumeLayout(false);

		}
		#endregion

		public bool FailureToolTips
		{
			get { return this.stackTrace.AutoExpand; }
			set { this.stackTrace.AutoExpand = value; }
		}

		public void Initialize()
		{
		}

		public void Clear()
		{
			detailList.Items.Clear();
			detailList.ContextMenu = null;
			stackTrace.Text = "";
		}

		public void OnOptionsChanged()
		{
			this.stackTrace.AutoExpand = settings.GetSetting( "Gui.ResultTabs.ErrorsTab.ToolTipsEnabled ", false );
			bool wordWrap = settings.GetSetting( "Gui.ResultTabs.ErrorsTab.WordWrapEnabled", true );
		
			if ( this.stackTrace.WordWrap != wordWrap )
			{
				this.stackTrace.WordWrap = wordWrap;

				this.detailList.BeginUpdate();
				ArrayList copiedItems = new ArrayList( detailList.Items );
				this.detailList.Items.Clear();
				foreach( object item in copiedItems )
					this.detailList.Items.Add( item );
				this.detailList.EndUpdate();
				this.stackTrace.WordWrap = wordWrap;
			}
		}

		public void InsertTestResultItem( TestResult result )
		{
			TestResultItem item = new TestResultItem(result);
			detailList.BeginUpdate();
			detailList.Items.Insert(detailList.Items.Count, item);
			detailList.EndUpdate();
		}

		#region DetailList Events
		/// <summary>
		/// When one of the detail failure items is selected, display
		/// the stack trace and set up the tool tip for that item.
		/// </summary>
		private void detailList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TestResultItem resultItem = (TestResultItem)detailList.SelectedItem;
			//string stackTrace = resultItem.StackTrace;
			stackTrace.Text = resultItem.StackTrace;

			//			toolTip.SetToolTip(detailList,resultItem.GetToolTipMessage());
			detailList.ContextMenu = detailListContextMenu;
		}

		private void detailList_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
		{
			TestResultItem item = (TestResultItem) detailList.Items[e.Index];
			//string s = item.ToString();
			SizeF size = settings.GetSetting( "Gui.ResultTabs.ErrorsTab.WordWrapEnabled", false )
				? e.Graphics.MeasureString(item.ToString(), detailList.Font, detailList.ClientSize.Width )
				: e.Graphics.MeasureString(item.ToString(), detailList.Font );
			e.ItemHeight = (int)size.Height;
			e.ItemWidth = (int)size.Width;
		}

		private void detailList_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
		{
			if (e.Index >= 0) 
			{
				e.DrawBackground();
				TestResultItem item = (TestResultItem) detailList.Items[e.Index];
				bool selected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? true : false;
				Brush brush = selected ? SystemBrushes.HighlightText : SystemBrushes.WindowText;
				RectangleF layoutRect = e.Bounds;
				if (settings.GetSetting( "Gui.ResultTabs.ErrorsTab.WordWrapEnabled", true ) && layoutRect.Width > detailList.ClientSize.Width )
					layoutRect.Width = detailList.ClientSize.Width;
				e.Graphics.DrawString(item.ToString(),detailList.Font, brush, layoutRect);
				
			}
		}

		private void copyDetailMenuItem_Click(object sender, System.EventArgs e)
		{
			if ( detailList.SelectedItem != null )
				Clipboard.SetDataObject( detailList.SelectedItem.ToString() );
		}

		private void OnMouseHover(object sender, System.EventArgs e)
		{
			if ( tipWindow != null ) tipWindow.Close();

			if ( settings.GetSetting( "Gui.ResultTabs.ErrorsTab.ToolTipsEnabled", false ) && hoverIndex >= 0 && hoverIndex < detailList.Items.Count )
			{
				Graphics g = Graphics.FromHwnd( detailList.Handle );

				Rectangle itemRect = detailList.GetItemRectangle( hoverIndex );
				string text = detailList.Items[hoverIndex].ToString();

				SizeF sizeNeeded = g.MeasureString( text, detailList.Font );
				bool expansionNeeded = 
					itemRect.Width < (int)sizeNeeded.Width ||
					itemRect.Height < (int)sizeNeeded.Height;

				if ( expansionNeeded )
				{
					tipWindow = new TipWindow( detailList, hoverIndex );
					tipWindow.ItemBounds = itemRect;
					tipWindow.TipText = text;
					tipWindow.Expansion = TipWindow.ExpansionStyle.Both;
					tipWindow.Overlay = true;
					tipWindow.WantClicks = true;
					tipWindow.Closed += new EventHandler( tipWindow_Closed );
					tipWindow.Show();
				}
			}		
		}

		private void tipWindow_Closed( object sender, System.EventArgs e )
		{
			tipWindow = null;
			hoverIndex = -1;
			ClearTimer();
		}

		private void detailList_MouseLeave(object sender, System.EventArgs e)
		{
			hoverIndex = -1;
			ClearTimer();
		}

		private void detailList_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ClearTimer();

			hoverIndex = detailList.IndexFromPoint( e.X, e.Y );	

			if ( hoverIndex >= 0 && hoverIndex < detailList.Items.Count )
			{
				// Workaround problem of IndexFromPoint returning an
				// index when mouse is over bottom part of list.
				Rectangle r = detailList.GetItemRectangle( hoverIndex );
				if ( e.Y > r.Bottom )
					hoverIndex = -1;
				else
				{
					hoverTimer = new System.Windows.Forms.Timer();
					hoverTimer.Interval = 800;
					hoverTimer.Tick += new EventHandler( OnMouseHover );
					hoverTimer.Start();
				}
			}
		}

		private void ClearTimer()
		{
			if ( hoverTimer != null )
			{
				hoverTimer.Stop();
				hoverTimer.Dispose();
			}
		}

		private void stackTrace_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if ( e.KeyCode == Keys.A && e.Modifiers == Keys.Control )
			{
				stackTrace.SelectAll();
			}
		}

		//		private void enableWordWrapCheckBox_CheckedChanged(object sender, System.EventArgs e)
		//		{
		//			this.detailList.BeginUpdate();
		//			ArrayList copiedItems = new ArrayList( detailList.Items );
		//			this.detailList.Items.Clear();
		//			foreach( object item in copiedItems )
		//				this.detailList.Items.Add( item );
		//			this.detailList.EndUpdate();
		//			this.stackTrace.WordWrap = this.enableWordWrapCheckBox.Checked;
		//		}

		#endregion

		private void tabSplitter_SplitterMoved( object sender, SplitterEventArgs e )
		{
			settings.SaveSetting( "Gui.ResultTabs.ErrorsTabSplitterPosition", tabSplitter.SplitPosition );
		}

		#region TestObserver Members
		public void Subscribe(ITestEvents events)
		{
		}
		#endregion
	}
}