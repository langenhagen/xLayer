using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;

using D3DBuffer = SlimDX.Direct3D11.Buffer;
using SlimDX.D3DCompiler;

namespace xLayer
{
    abstract class Entity
    {
        public Device Device { get; protected set; }

        public DeviceContext Context { get; set; }


        public PrimitiveTopology PrimitiveTopology { get; protected set; }

        public InputLayout InputLayout { get; protected set; }


        public D3DBuffer VertexBuffer { get; protected set; }


        public VertexShader VertexShader { get; set; }

        public VertexBufferBinding VertexBufferBinding { get; protected set; }

        public ShaderSignature VertexShaderSignature { get; set; }



        public PixelShader PixelShader { get; set; }

        public ResourceProvider[] ResourceProviders { get; set; }


        /////////////////////////////////////////////////////////////////////////////////
        // Ctor & Dtor

        public Entity( EntityDescription desc)
        {
            Device  = desc.Device;
            Context = desc.Context;
            PrimitiveTopology = desc.PrimitiveTopology;
            VertexShader = desc.VertexShader;
            VertexShaderSignature = desc.VSInputSignature;
            PixelShader = desc.PixelShader;
            ResourceProviders = desc.PSResourceProviders;
        }

        public Entity(
            Device device,
            DeviceContext context,
            PrimitiveTopology topology,
            VertexShader vertexShader,
            ShaderSignature vsSignature,
            PixelShader pixelShader,
            ResourceProvider[] resourceProviders)
        {
            Device = device;
            Context = context;
            PrimitiveTopology = topology;
            VertexShader = vertexShader;
            VertexShaderSignature = vsSignature;
            PixelShader = pixelShader;
            ResourceProviders = resourceProviders;
        }

        ~Entity()
        {
            // TODO Dispositions?
            
            if( VertexBuffer.Disposed)
                VertexBuffer.Dispose();
            
            if( InputLayout.Disposed)
                InputLayout.Dispose();
        }

        /////////////////////////////////////////////////////////////////////////////////
        // METHODS

        public void Draw()
        {
            // change vertex shader (if necessary)
            if (Context.VertexShader.Get() != VertexShader)
                Context.VertexShader.Set(VertexShader);

            // change pixel shader (if necessary)
            if (Context.PixelShader.Get() != PixelShader)
                Context.PixelShader.Set(PixelShader);


            if( Context.InputAssembler.PrimitiveTopology != PrimitiveTopology)
                Context.InputAssembler.PrimitiveTopology = PrimitiveTopology;

            Context.InputAssembler.InputLayout = InputLayout;
            Context.InputAssembler.SetVertexBuffers(0, VertexBufferBinding);

            Context.PixelShader.SetShaderResources(ShaderResourceViews, 0, ShaderResourceViews.Length);

            
            Context.Draw(GetNumVertices(), 0);


            foreach (var view in ShaderResourceViews)
            {
                view.Dispose();
            }
        }


        /////////////////////////////////////////////////////////////////////////////////
        // HELPERS


        /////////////////////////////////////////////////////////////////////////////////
        // ABSTRACT METHODS

        public abstract int GetNumVertices();

        ////////////////////////////////////////////////////////////////////////////////
        // GETTERS & SETTERS
        
        public ShaderResourceView[] ShaderResourceViews
        {
            get
            {
                ShaderResourceView[] ret = new ShaderResourceView[ResourceProviders.Length];

                for (int i = 0; i < ret.Length; ++i)
                    ret[i] = ResourceProviders[i].ShaderResourceView;

                return ret;
            }
        }
    }
}
