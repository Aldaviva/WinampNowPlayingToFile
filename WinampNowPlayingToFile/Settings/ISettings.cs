using System;
using System.Collections.Generic;

namespace WinampNowPlayingToFile.Settings; 

public interface ISettings {

    struct textTemplate {
		public textTemplate(string fileName = null!, string text = null!) {
            this.fileName = fileName!;
            this.text = text!;
		}

		public string fileName { get; set; }
        public string text { get; set; }

		public override string ToString() {
			return text;
		}
	}

    string albumArtFilename { get; set; }

    abstract List<textTemplate> textTemplates { get; set; }

    event EventHandler settingsUpdated;

    void load();
    ISettings loadDefaults();
    textTemplate getDefault();
    void save();

}