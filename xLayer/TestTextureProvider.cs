using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;


using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;

namespace xLayer
{
    class TestTextureProvider : ResourceProvider, ITextureProvider
    {
        private Texture2D _texture;

        //////////////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTOR & DESTRUCTOR

        public TestTextureProvider( Device device, string pathToTexture)
        : base( device)
        {
            Device = device;
            _texture = Texture2D.FromFile( device, pathToTexture);
        }


        /// <summary>
        /// Destructor
        /// </summary>
        ~TestTextureProvider()
        {
            _texture.Dispose();
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        // METHODS

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public Texture2D GetTexture2D()
        {
            return _texture;
        }

        protected override Resource DoUpdate()
        {
            return GetTexture2D();
        }


    }
}
