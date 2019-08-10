using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

namespace xLayer
{
    class EntityDescription
    {
        public Device Device                            { get; set; }
        public DeviceContext Context                    { get; set; }
        public PrimitiveTopology PrimitiveTopology      { get; set; }
        public VertexShader VertexShader                { get; set; }
        public ShaderSignature VSInputSignature         { get; set; }
        public PixelShader PixelShader                  { get; set; }
        public ResourceProvider[] PSResourceProviders   { get; set; }
    }
}
