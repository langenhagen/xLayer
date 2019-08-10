using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;


namespace xLayer
{
    /// <summary>
    /// Common interface for texture providers in the xLayer system.
    /// </summary>
    interface ITextureProvider
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        Texture2D GetTexture2D();
    }
}
