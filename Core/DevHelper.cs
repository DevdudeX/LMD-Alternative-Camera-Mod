using LMSR_AlternativeCameraMod.Config;
using LMSR_AlternativeCameraMod.Language;


namespace LMSR_AlternativeCameraMod;

internal class DevHelper
{
	private readonly State _state;
	private readonly InputHandler _input;
	private readonly CameraControl _camera;
	private readonly Hud _hud;
	private readonly LanguageConfig _lang;
	private readonly Configuration _cfg;


	public DevHelper(State state, InputHandler input, CameraControl camera, Hud hud, LanguageConfig lang, Configuration cfg)
	{
		_state = state;
		_input = input;
		_camera = camera;
		_hud = hud;
		_lang = lang;
		_cfg = cfg;
	}


	public void ProcessDevRequest()
	{
		if (_input.DevKey(12))
		{
			// write all config examples
			var langs = LanguageConfig.GetAvailableLanguages();
			foreach (var lang in langs)
			{
				var lc = LanguageConfig.Load(lang);
				var cfg = Configuration.CreateForLanguage(lc);
				cfg.Save();
			}
		}
	}
}
