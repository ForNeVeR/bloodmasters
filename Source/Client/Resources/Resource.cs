/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// The Resource abstract class defines the behaviour of a
// Direct3D resource that must be reloaded on device reset.
// Different types of resources inherit from this class.

namespace CodeImp.Bloodmasters.Client
{
	// Abstract resource class
	public abstract class Resource
	{
		#region ================== Variables

		// This holds the file from which the resource is created
		protected string resourcereferencename = "";
		protected bool resourceloaded = false;

		#endregion

		#region ================== Properties

		public string Referencename { get { return resourcereferencename; } }
		public bool Loaded { get { return resourceloaded; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor: Resource must be created from a file
		public Resource(string referencename)
		{
			// Keep the reference name
			resourcereferencename = referencename;
		}

		// Destructor: Unload the resource if not already unloaded
		~Resource()
		{
			// Reload the resource if not already loaded
			if(resourceloaded) this.Unload();
		}

		#endregion

		#region ================== Public Functions

		// This must be overridden with the code to load the resource
		// and the overriding function must call this function
		public virtual void Load()
		{
			// Resource is now loaded
			resourceloaded = true;
		}

		// This reloads the resource by calling the Load function
		public void Reload()
		{
			// Reload the resource if not already loaded
			if(!resourceloaded) this.Load();
		}

		// This must be overridden with the code to unload the resource
		// and the overriding function must call this function
		public virtual void Unload()
		{
			// Resource is now unloaded
			resourceloaded = false;
		}

		// This destroys the resource completely
		public void Destroy()
		{
			// Call the destroy function to destroy me
			Direct3D.DestroyResource(resourcereferencename);
		}

		#endregion
	}
}
