using System;
using System.Collections.Generic;
using System.IO;
using GnomeSubtitles.Ui.Component;
using Gtk;
using Mono.Unix;

namespace GnomeSubtitles.Ui.Component
{
	
	public class FileCombo
	{	
		protected List<string> fileList;
		protected ComboBox filesCombo = null;
		protected Label label;
		
		public int Active {
			get{return filesCombo.Active > 1 ? filesCombo.Active - 2 : -1;}
			set{SetActiveItem(value);}
		}
		
		public string ActiveSelection {
			get{return filesCombo.Active > 1 ? fileList[filesCombo.Active -2] : null;}
		}
		
		/* Events */
	
		public event EventHandler FileSelectionChanged;
		
		public FileCombo (Label newlabel, ComboBox newfilesCombo) {
			filesCombo = newfilesCombo;
			label = newlabel;
			ComboBoxUtil.InitComboBox(filesCombo);
			
			label.Visible = true;
			filesCombo.Visible = true;
			
		}

		public void FillComboBox (IEnumerable<string> newlist) {
			ConnectComboBoxChangedSignal(false);
			
			fileList = new List<string>(newlist);
			(filesCombo.Model as ListStore).Clear();
				
			foreach (string file in fileList) {
				filesCombo.AppendText(Path.GetFileName(file));
			}
			filesCombo.PrependText("-");
			filesCombo.PrependText(Catalog.GetString("None"));
			filesCombo.Active = 0;
			
			ConnectComboBoxChangedSignal(true);
		}	
		
		private void SetActiveItem (int newactive) {
			ConnectComboBoxChangedSignal(false);
			
			if (newactive == -1)
				filesCombo.Active = 0;
			else
				filesCombo.Active = newactive + 2;
			
			ConnectComboBoxChangedSignal(true);
		}
		
	
		private void ConnectComboBoxChangedSignal (bool toconnect) {
			if (toconnect) {
				filesCombo.Changed += OnComboBoxChanged;
			} else {
				filesCombo.Changed -= OnComboBoxChanged;
			}
			
		}
		
		/*Event Handlers */
		
		private void OnComboBoxChanged (object sender, EventArgs args) {
			ComboBox o = filesCombo;
			if (FileSelectionChanged != null)
				FileSelectionChanged(o, args);
		}
	}
}