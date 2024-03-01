namespace WinampNowPlayingToFile.Facade;

public struct textTemplate {
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

