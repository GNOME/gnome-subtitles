using System;
using GnomeSubtitles.Core;
using GnomeSubtitles.Ui.Component;
using Gtk;

namespace GnomeSubtitles.Ui.Component
{
	public class FilexEncodingCombo : FileCombo
	{
	protected EncodingComboBox encombo = null;
		
	protected ComboBox fileEncodingCombo = null;
	
	public EncodingDescription SelectedEncoding  {
		get{GetSelectedEncoding(); return encombo.ChosenEncoding;}		
	}
	
	public int EncomboActive {
		get{return fileEncodingCombo.Active;}
		set{fileEncodingCombo.Active = Active;}
	}
	
	public FilexEncodingCombo (Label newlabel, ComboBox newfilesCombo, ComboBox newfileEncodingCombo ) : base(newlabel, newfilesCombo) {
		fileEncodingCombo = newfileEncodingCombo;
		InitEncodingComboBox();	
		filesCombo.Changed += OnComboChanged;
			
		fileEncodingCombo.Visible = true;
	}
	
	private void InitEncodingComboBox () { 
		int fixedEncoding = -1;
		ConfigFileOpenEncoding encodingConfig = Base.Config.PrefsDefaultsFileOpenEncoding;
		if (encodingConfig == ConfigFileOpenEncoding.Fixed) {
			string encodingName = Base.Config.PrefsDefaultsFileOpenEncodingFixed;
			EncodingDescription encodingDescription = EncodingDescription.Empty;
			Encodings.Find(encodingName, ref encodingDescription);
			fixedEncoding = encodingDescription.CodePage;
		}
		this.encombo = new EncodingComboBox(fileEncodingCombo, true, null, fixedEncoding);
		/* Only need to handle the case of currentLocale, as Fixed is handled before and AutoDetect is the default behaviour */
		if (encodingConfig == ConfigFileOpenEncoding.CurrentLocale) {
			encombo.ActiveSelection = (int)encodingConfig;
		}
	}	
	
	private void GetSelectedEncoding () {
		if (Base.Config.PrefsDefaultsFileOpenEncodingOption == ConfigFileOpenEncodingOption.RememberLastUsed) {
			int activeAction = encombo.ActiveSelection;
			ConfigFileOpenEncoding activeOption = (ConfigFileOpenEncoding)Enum.ToObject(typeof(ConfigFileOpenEncoding), activeAction);
			if (((int)activeOption) >= ((int)ConfigFileOpenEncoding.Fixed))
				Base.Config.PrefsDefaultsFileOpenEncodingFixed = encombo.ChosenEncoding.Name;
			else
				Base.Config.PrefsDefaultsFileOpenEncoding = activeOption;
		}		
	}	
			
	private void OnComboChanged (object sender, EventArgs e) {
		encombo.ActiveSelection = 0;		
	}
	}
}