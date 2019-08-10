using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;

using Awesomium.Core;

using Device    = SlimDX.Direct3D11.Device;
using Resource  = SlimDX.Direct3D11.Resource;
using D3DBuffer = SlimDX.Direct3D11.Buffer;


namespace xLayer
{
    class XLayer
    {
        //private readonly Color4           _backColor = new Color4(1, 0, 1);
                                         
        private          RenderForm       _form;
                         
        private          Device           _device;
        private          SwapChain        _swapChain;
        private          RenderTargetView _renderTarget;


        // the vertex shader and its signature.
        private VertexShader    _vertexShader;
        private ShaderSignature _vsInputSignature;


        /// Stores all pixel shaders.
        private Dictionary<string, PixelShader> _pixelShaders = new Dictionary<string, PixelShader>();

        /// Stores all the resource providers and their current resources.
        // TODO change the string keys to something faster like ...e.g.. ints ....
        private Dictionary<string, ResourceProvider> _providers = new Dictionary<string, ResourceProvider>();


        private List<Entity> _rectangles = new List<Entity>();



        /////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTOR & DESTRUCTOR

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var xLayer = new XLayer();

            xLayer.Init("xLayer");
            xLayer.Prepare();


            xLayer.RenderWindow();

           /* var t1 = new Thread(supertest);
            var t2 = new Thread(supertest);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();*/
        }

        static void supertest()
        {
            var xLayer = new XLayer();

            xLayer.Init("xLayer");
            xLayer.Prepare();

            xLayer.RenderWindow();
        }

        /////////////////////////////////////////////////////////////////////////////////
        // C'tor and D'tor

        public XLayer()
        {
        }


        ~XLayer()
        {
            WebCore.Shutdown();


            foreach (var pair in _pixelShaders)
            {
                pair.Value.Dispose();
            }


            _vertexShader.Dispose();
            _vsInputSignature.Dispose();
            
            _renderTarget.Dispose();
            _swapChain.Dispose();
            _device.Dispose();
        }


        /////////////////////////////////////////////////////////////////////////////////
        // METHODS

        [Obsolete("just for testing")]
        public void Prepare()
        {

            var context = _device.ImmediateContext;


            // setup resources
            _providers.Add("at1", new TestTextureProvider(_device, "res/alphatest.png"));
            _providers.Add("bg1", new TestTextureProvider(_device, "res/xLayer.png"));
            _providers.Add("wb1", new WebTexture(_device, "http://www.libpng.org/pub/png/pngintro.html".ToUri()));
            _providers.Add("yt1", new WebTexture(_device, "http://www.youtube.com/watch?v=KxWBv4y3m4w".ToUri()));
            _providers.Add("yt2", new WebTexture(_device, "http://www.youtube.com/watch?v=0EurfDnlD5A".ToUri()));
            _providers.Add("yt3", new WebTexture(_device, "http://www.youtube.com/watch?v=qdwUkYrHosk".ToUri()));
            _providers.Add("yt4", new WebTexture(_device, "http://www.youtube.com/watch?v=9w5RHGBvf9A".ToUri()));

            // descriptor easifies the creation of entities, e.g. rectangles.
            EntityDescription entityDescription = new EntityDescription
            {
                Device = _device,
                Context = _device.ImmediateContext,
                VertexShader = _vertexShader,
                VSInputSignature = _vsInputSignature,
                PrimitiveTopology = PrimitiveTopology.TriangleStrip
            };
            

            entityDescription.PixelShader = _pixelShaders["Simple"];
            entityDescription.PSResourceProviders =  new ResourceProvider[] { _providers["bg1"] };
            var rect1 = new Rectangle( new Vector2( 0f,  0f), new Vector2( 1f,  1f), entityDescription);

          
            entityDescription.PSResourceProviders = new ResourceProvider[] { _providers["yt1"] };
            var rect2 = new Rectangle(new Vector2(0f, 0f), new Vector2(.5f, .5f), entityDescription);


            entityDescription.PixelShader = _pixelShaders["AlphaMap"];
            entityDescription.PSResourceProviders = new ResourceProvider[] { _providers["yt2"], _providers["at1"] };
            var rect3 = new Rectangle(new Vector2(0, .5f), new Vector2(.5f, .5f), entityDescription);


            entityDescription.PSResourceProviders = new ResourceProvider[] { _providers["yt3"], _providers["wb1"] };
            var rect4 = new Rectangle(new Vector2(.5f, 0), new Vector2(.5f, .5f), entityDescription);


            entityDescription.PixelShader = _pixelShaders["GrayscaleAlphaMap"];
            entityDescription.PSResourceProviders = new ResourceProvider[] { _providers["yt4"], _providers["yt2"] };
            var rect5 = new Rectangle(new Vector2(.5f, .5f), new Vector2(.5f, .5f), entityDescription);


            _rectangles.Add(rect1);
            _rectangles.Add(rect2);
            _rectangles.Add(rect3);
            _rectangles.Add(rect4);
            _rectangles.Add(rect5);

        }


        /// <summary>
        /// TODO doc
        /// </summary>
        public void Init( string windowName, int top = 10, int left = 10, int width = 1280, int height = 1024)
        {
            InitAwesomium();

            _form = new RenderForm
            {
                Text = windowName,
                Icon = new System.Drawing.Icon("res/x.ico"),
                FormBorderStyle = FormBorderStyle.Sizable, // set to NONE for pseudo full screen
                StartPosition   = FormStartPosition.Manual,
                
                Top = top,
                Left = left,
                Width = width,
                Height = height
            };


            // The characteristics of our swap chain
            var swapChainDescription = new SwapChainDescription
            {
                BufferCount = 2, // front & back buffer
                Usage = Usage.RenderTargetOutput,
                OutputHandle = _form.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1 /*samples per pixel*/, 0 /*anti-aliasing level*/),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };


            Device.CreateWithSwapChain(DriverType.Hardware,
                                        DeviceCreationFlags.None,
                                        swapChainDescription,
                                        out _device,
                                        out _swapChain);



            // create a view of our render target, which is the backbuffer of the swap chain
            using (var res = Resource.FromSwapChain<Texture2D>(_swapChain, 0))
                _renderTarget = new RenderTargetView(_device, res);



            // create a viewport
            var context = _device.ImmediateContext;
            var viewport = new Viewport(0.0f, 0.0f, _form.ClientSize.Width, _form.ClientSize.Height);
            context.OutputMerger.SetTargets(_renderTarget);
            context.Rasterizer.SetViewports(viewport);

            
            // load and compile the vertex shader & get its input signature
            using (var bytecode = ShaderBytecode.CompileFromFile("res/simple.fx", "vertex_shader", "vs_4_0", ShaderFlags.None, EffectFlags.None))
            {
                _vsInputSignature = ShaderSignature.GetInputSignature(bytecode);
                _vertexShader = new VertexShader(_device, bytecode);
            }
            


            // enable alpha blending
            var blendStateDescription = new BlendStateDescription
            {
                AlphaToCoverageEnable  = false,
                IndependentBlendEnable = false,
            };

            blendStateDescription.RenderTargets[0].BlendEnable           = true;
            blendStateDescription.RenderTargets[0].SourceBlend           = BlendOption.SourceAlpha;
            blendStateDescription.RenderTargets[0].DestinationBlend      = BlendOption.InverseSourceAlpha;
            blendStateDescription.RenderTargets[0].BlendOperation        = BlendOperation.Add;
            blendStateDescription.RenderTargets[0].SourceBlendAlpha      = BlendOption.One;
            blendStateDescription.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
            blendStateDescription.RenderTargets[0].BlendOperationAlpha   = BlendOperation.Add;
            blendStateDescription.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;


            BlendState TransparentBS = BlendState.FromDescription(_device, blendStateDescription);

            context.OutputMerger.BlendState = TransparentBS;

            
            // initialize all pixel shaders
            InitPixelShaders();


            #region Input Handling

            // prevent DXGI handling of alt+enter, which doesn't work properly with Winforms
            using (var factory = _swapChain.GetParent<Factory>())
                factory.SetWindowAssociation(_form.Handle, WindowAssociationFlags.IgnoreAltEnter);


            // add input handler
            _form.KeyDown += HandleInput;


            // handle form size changes
            /* TODO do as u wish
            _form.UserResized += (o, e) =>
            {
                _renderTarget.Dispose();

                _swapChain.ResizeBuffers(2, 0, 0, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
                using (var resource = Resource.FromSwapChain<Texture2D>(_swapChain, 0))
                    _renderTarget = new RenderTargetView(_device, resource);

                context.OutputMerger.SetTargets(_renderTarget);
            };
            */

            #endregion
        }



        /// <summary>
        /// Handles the rendering.
        /// </summary>
        public void RenderWindow()
        {
            var context = _device.ImmediateContext;

            // render loop ****************************************************
            MessagePump.Run(_form, () =>
            {
                Util.CalculateFrameRate();

                // clear the render target .. if you want to...
                //context.ClearRenderTargetView(_renderTarget, __backColor);

                // update all the resources once
                foreach (var resProvider in _providers)
                    resProvider.Value.Update();

                // connect resources to rectangle
                for (int i = 0; i < _rectangles.Count; ++i )
                {
                    _rectangles[i].Draw();
                }


                _swapChain.Present(0, PresentFlags.None);

            }); 
            // ****************************************************************

        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        // HELPERS


        // Handles the keyboard input.
        // Your augmentation goes here.
        private void HandleInput(Object o, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
                _swapChain.IsFullScreen = !_swapChain.IsFullScreen;
            else if (e.KeyCode == Keys.Tab)
                Console.WriteLine(Util.GetFrameRate());
        }


        /// <summary>
        /// Initializes Awesomium.
        /// </summary>
        private void InitAwesomium()
        {
            // Initialize the WebCore with some configuration settings.
            WebCore.Initialize(new WebConfig
            {
                LogPath = Environment.CurrentDirectory,
                LogLevel = LogLevel.Verbose,
                RemoteDebuggingPort = 1225
            });
        }


        /// <summary>
        /// Initializes all pixel shaders.
        /// Your own ps initialization should go here, too.
        /// </summary>
        private void InitPixelShaders()
        {
            // load, compile and set the pixel shaders
            
            PixelShader ps;

            using (var bytecode = ShaderBytecode.CompileFromFile("res/simple.fx", "pixel_shader", "ps_4_0", ShaderFlags.None, EffectFlags.None))
                ps = new PixelShader(_device, bytecode);

            _pixelShaders.Add("Simple", ps);


            using (var bytecode = ShaderBytecode.CompileFromFile("res/alphamap.fx", "pixel_shader", "ps_4_0", ShaderFlags.None, EffectFlags.None))
                ps = new PixelShader(_device, bytecode);

            _pixelShaders.Add("AlphaMap", ps);


            using (var bytecode = ShaderBytecode.CompileFromFile("res/grayscalealphamap.fx", "pixel_shader", "ps_4_0", ShaderFlags.None, EffectFlags.None))
                ps = new PixelShader(_device, bytecode);

            _pixelShaders.Add("GrayscaleAlphaMap", ps);

        }
        
    }
}
