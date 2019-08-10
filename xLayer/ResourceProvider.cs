using SlimDX.Direct3D11;

namespace xLayer
{
    /// <summary>
    /// Common superclass for shader resource providers in the xLayer system.
    /// </summary>
    abstract class ResourceProvider
    {
        /// <summary>
        /// The current resource. Will not be calculated newly but just grabbed.
        /// </summary>
        public Resource Resource { get; private set; }

        /// <summary>
        /// The d3d11 device.
        /// </summary>
        public Device Device { get; protected set; }


        //////////////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTOR & DESTRUCTOR

        public ResourceProvider(Device device)
        {
            Device = device;
            Resource = null;
        }

        ~ResourceProvider()
        {
            if (Resource != null)
                Resource.Dispose();
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        // METHODS


        /// <summary>
        /// Calculates the new Resource, Sets the Resource property to the current resource
        /// and returns it.
        /// </summary>
        /// <returns>A new Resource</returns>
        public Resource Update()
        {
            Resource = DoUpdate();
            return Resource;
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        // ABSTRACT HELPER


        /// <summary>
        /// Returns the raw resource, e.g. to combine it with other resources to create a bigger one.
        /// </summary>
        /// <returns></returns>
        protected abstract Resource DoUpdate();



        //////////////////////////////////////////////////////////////////////////////////////////
        // GETTERS & SETTERS


        /// <summary>
        /// Retrieves a ShaderResourceView.
        /// </summary>
        /// <returns>A fresh ShaderResourceView.</returns>
        public ShaderResourceView ShaderResourceView
        {
            get { return new ShaderResourceView(Device, Resource); }
        }


    }

}
