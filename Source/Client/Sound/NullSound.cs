namespace CodeImp.Bloodmasters.Client
{
	/// <summary>
	/// Summary description for NullSound.
	/// </summary>
	public class NullSound : ISound
	{
		// Constructor
		public NullSound()
		{
		}

		public void ResetSettings()
		{
			// TODO:  Add NullSound.ResetSettings implementation
		}

		public void Update()
		{
			// TODO:  Add NullSound.Update implementation
		}

		public void SetRandomOffset()
		{
			// TODO:  Add NullSound.SetRandomOffset implementation
		}

		public void Play()
		{
			// TODO:  Add NullSound.Play implementation
		}

		void CodeImp.Bloodmasters.Client.ISound.Play(bool repeat)
		{
			// TODO:  Add NullSound.CodeImp.Bloodmasters.Client.ISound.Play implementation
		}

		void CodeImp.Bloodmasters.Client.ISound.Play(float volume, bool repeat)
		{
			// TODO:  Add NullSound.CodeImp.Bloodmasters.Client.ISound.Play implementation
		}

		public void Stop()
		{
			// TODO:  Add NullSound.Stop implementation
		}

		public bool Repeat
		{
			get
			{
				// TODO:  Add NullSound.Repeat getter implementation
				return false;
			}
		}

		public bool AutoDispose
		{
			get
			{
				// TODO:  Add NullSound.AutoDispose getter implementation
				return false;
			}
			set
			{
				// TODO:  Add NullSound.AutoDispose setter implementation
			}
		}

		public string Filename
		{
			get
			{
				// TODO:  Add NullSound.Filename getter implementation
				return null;
			}
		}

		public float Volume
		{
			get
			{
				// TODO:  Add NullSound.Volume getter implementation
				return 0;
			}
			set
			{
				// TODO:  Add NullSound.Volume setter implementation
			}
		}

		public bool Playing
		{
			get
			{
				// TODO:  Add NullSound.Playing getter implementation
				return false;
			}
		}

		public bool Positional
		{
			get
			{
				// TODO:  Add NullSound.Positional getter implementation
				return false;
			}
		}

		public Vector2D Position
		{
			get
			{
				// TODO:  Add NullSound.Position getter implementation
				return new Vector2D ();
			}
			set
			{
				// TODO:  Add NullSound.Position setter implementation
			}
		}

		public bool Disposed
		{
			get
			{
				// TODO:  Add NullSound.Disposed getter implementation
				return false;
			}
		}

		public void Dispose()
		{
			// TODO:  Add NullSound.Dispose implementation
		}
	}
}
