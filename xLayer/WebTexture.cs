using System;

using Awesomium.Core;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;


using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;


namespace xLayer
{
    /// <summary>
    /// An implementation of the Texture Provider that can render websites 
    /// with help of the awesomium SDK.
    /// </summary>
    class WebTexture : ResourceProvider, ITextureProvider
    {

        private WebView _view;

        private Texture2D _texture;

        private BitmapSurface _surface;

        /// <summary>
        /// The device's context.
        /// </summary>
        public DeviceContext Context { get; set; }

        /// <summary>
        /// indicates, whether to update only when the surface is dirty, which can boost the overall framerate
        /// but can lead to frame-rate-dropping-like artifacts with movies or constantly moved content.
        /// Use it with static or almost static websites.
        /// 
        /// TODO / CAUTION: THIS HAS EFFECT ON ALL WebTextures!!! Means, setting to false drops update rate!! dunno why...
        /// </summary>
        public bool UpdateIfSurfaceIsNotDirty { get; set; }

               

        //////////////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTOR & DESTRUCTOR
        
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="device">The d3d11 device on which to connect the texture.</param>
        /// <param name="context">The context to be used for this webtexture.</param>
        /// <param name="url">The url to use.</param>
        /// /// <param name="updateIfSurfaceIsNotDirty">Look at member!</param>
        /// <param name="width">The webview's width.</param>
        /// <param name="height">The webview's height.</param>
        /// <param name="isTransparent">Defines, whether the background of the site shall be transparent or not.</param>
        public WebTexture(
            Device device, 
            DeviceContext context, 
            Uri url,
            bool updateIfSurfaceIsNotDirty = true,
            int width = 1920, 
            int height = 1080, 
            bool isTransparent = true)
            : base( device)
        {
            
            UpdateIfSurfaceIsNotDirty = updateIfSurfaceIsNotDirty;

            // setup awesomium view
            
            WebSession webSession = WebCore.CreateWebSession(new WebPreferences
            {
                CustomCSS = "::-webkit-scrollbar { width: 0px; height: 0px; } ",
            });


            _view               = WebCore.CreateWebView(width, height, webSession);
            _view.Source        = url;
            _view.IsTransparent = isTransparent;
            

            while (_view.IsLoading)
            {
                WebCore.Update();
            }

            // init texture

            Context = context;

            Texture2DDescription description = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Write,
                Format = SlimDX.DXGI.Format.B8G8R8A8_UNorm,
                Height = _view.Height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Dynamic,
                Width = _view.Width
            };

            _texture = new Texture2D(Device, description);

            _surface = _view.Surface as BitmapSurface;
        }


        /// <summary>
        /// Second constructor. Sets the context to the device's immediate context.
        /// </summary>
        /// <param name="device">The d3d11 device on which to connect the texture.</param>
        /// <param name="url">The url to use.</param>
        /// /// <param name="updateIfSurfaceIsNotDirty">Look at member!</param>
        /// <param name="width">The webview's width.</param>
        /// <param name="height">The webview's height.</param>
        /// <param name="isTransparent">Defines, whether the background of the site shall be transparent or not.</param>
        public WebTexture(
            Device device, 
            Uri url,
            bool updateIfSurfaceIsNotDirty = true,
            int width = 1920, 
            int height = 1080, 
            bool isTransparent = true)
            : this(device, device.ImmediateContext, url, updateIfSurfaceIsNotDirty, width, height, isTransparent)
        {}



        ~WebTexture()
        {
            // TODO dunno why, but causes exception once or thrice in a while
            _texture.Dispose();
            _view.Dispose();
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        // METHODS


        /// <summary>
        /// Retrieves the dynamic web texture.
        /// </summary>
        /// <returns>A SlimDX DirectX11 Texture of some website.</returns>
        public Texture2D GetTexture2D()
        {
            
            if (UpdateIfSurfaceIsNotDirty || _surface.IsDirty)
            {   
                // copy the image into a texture 2D
                DataBox dataBox = Context.MapSubresource(_texture, 0, 0, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
                dataBox.Data.WriteRange(_surface.Buffer, _view.Width * 4 * _view.Height);

                Context.UnmapSubresource(_texture, 0);

                _surface.IsDirty = false;
            }
            
            return _texture;
        }

        protected override Resource DoUpdate()
        {
            return GetTexture2D();
        }

        /// <summary>
        /// Retrieves the current bitmap surface of the internal Awesomium WebView.
        /// </summary>
        /// <returns>The BitmapSurface.</returns>
        public BitmapSurface GetBitmapSurface()
        {
            return _view.Surface as BitmapSurface;
        }



        /// <summary>
        /// Saves a png of the screen
        /// </summary>
        /// <param name="filePath"></param>
        public void SafeViewToPng(string filePath)
        {
            (_view.Surface as BitmapSurface).SaveToPNG(filePath, true);
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        // HELPERS

    }
}
